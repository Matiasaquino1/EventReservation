using AutoMapper;
using EventReservations.Dto;
using EventReservations.Profiles;
using EventReservations.Models;
using EventReservations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Security.Claims;
using Newtonsoft.Json;

namespace EventReservations.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
        }

        // POST: api/payments/process
        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDto request)
        {
            try
            {
                var payment = await _paymentService.ProcessPaymentAsync(
                    request.ReservationId,
                    request.Amount,
                    request.Currency,
                    request.PaymentMethodId
                );

                var paymentDto = _mapper.Map<PaymentRequestDto>(payment);

                return Ok(new
                {
                    message = "Pago procesado correctamente",
                    payment = paymentDto
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.StripeError.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });     
            }
        }

        // POST: api/payments/create-payment-intent
        [HttpPost("create-payment-intent")]
        [Authorize] 
        public async Task<IActionResult> CreatePaymentIntent([FromBody] decimal amount)
        {
            var paymentIntent = await _paymentService.CreatePaymentIntentAsync(amount);
            return Ok(new { clientSecret = paymentIntent.ClientSecret });
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            string json = string.Empty;

            try
            {
                using var reader = new StreamReader(HttpContext.Request.Body);
                json = await reader.ReadToEndAsync();
                _logger.LogInformation("Webhook recibido: {Json}", json);


                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogWarning("Webhook recibido sin contenido en el body.");
                    return BadRequest(new { error = "El body del webhook está vacío." });
                }

                var stripeSignature = Request.Headers["Stripe-Signature"].FirstOrDefault();
                var webhookSecret = _configuration["Stripe:WebhookSecret"];

                Stripe.Event stripeEvent;

                if (!string.IsNullOrEmpty(webhookSecret) && !string.IsNullOrEmpty(stripeSignature))
                {
                    // Validación real con Stripe
                    stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);
                    _logger.LogInformation("Webhook verificado correctamente (modo producción): {Type}", stripeEvent.Type);
                }
                else
                {
                    // Para desarrollo: parse manual y seguro
                    _logger.LogWarning("Procesando webhook sin verificación de firma (modo desarrollo).");

                    dynamic temp = JsonConvert.DeserializeObject<dynamic>(json);

                    if (temp == null || temp.type == null)
                    {
                        _logger.LogWarning("Webhook inválido: JSON sin campo 'type'.");
                        return BadRequest(new { error = "JSON inválido o incompleto." });
                    }

                    var eventData = new Stripe.EventData();
                    if (temp?.data != null)
                    {
                        eventData.Object = temp.data.@object;
                    }

                    stripeEvent = new Stripe.Event
                    {
                        Id = temp?.id ?? Guid.NewGuid().ToString(),
                        Type = temp?.type,
                        Data = eventData,
                        Created = DateTime.UtcNow,
                        Livemode = false
                    };
                }

                // Procesar el evento en mi servicio.
                await _paymentService.ProcessWebhookAsync(stripeEvent);

                _logger.LogInformation("Webhook procesado correctamente: {Type}", stripeEvent.Type);
                return Ok(new { received = true });
            }
            catch (StripeException sex)
            {
                _logger.LogWarning(sex, "Error validando firma o formato del webhook de Stripe");
                return BadRequest(new { error = sex.Message });
            }
            catch (JsonException jex)
            {
                _logger.LogError(jex, "Error al deserializar el JSON del webhook");
                return BadRequest(new { error = "Formato JSON inválido." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando webhook de Stripe");
                return StatusCode(500, new { error = "Error interno del servidor." });
            }
        }

        //Historial del usuario autenticado o admin
        [HttpGet("history")]
        [Authorize]
        public async Task<IActionResult> GetHistory()
        {
            try
            {
                IEnumerable<Payment> payments;

                if (User.IsInRole("Admin"))
                {
                    payments = await _paymentService.GetAllPaymentsAsync();
                }
                else
                {
                    var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (claim == null) return Unauthorized();
                    if (!int.TryParse(claim.Value, out var userId)) return Unauthorized();

                    payments = await _paymentService.GetPaymentsByUserIdAsync(userId);
                }

                var dtos = payments.Select(p => _mapper.Map<PaymentRequestDto>(p));
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetHistory payments");
                return StatusCode(500, new { error = ex.Message });
            }
        }


    }
}



