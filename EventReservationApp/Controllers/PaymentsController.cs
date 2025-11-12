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
using System.ComponentModel.DataAnnotations; // Para validación

namespace EventReservations.Controllers
{
    /// <summary>
    /// Controlador para gestionar pagos y transacciones relacionadas con reservas de eventos.
    /// Utiliza Stripe para procesar pagos y webhooks.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IReservationService _reservationService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            IReservationService reservationService,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _reservationService = reservationService;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Procesa un pago para una reserva específica utilizando Stripe.
        /// </summary>
        /// <param name="request">Datos del pago, incluyendo ReservationId, Amount, Currency y PaymentMethodId.</param>
        /// <returns>Un objeto con mensaje de éxito y detalles del pago procesado.</returns>
        /// <response code="200">Pago procesado correctamente.</response>
        /// <response code="400">Error en la solicitud o en Stripe.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPost("process")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
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
                if (payment.Status == "Succeeded")
                { 
                    await _reservationService.ConfirmPaymentAndDecrementTicketsAsync(request.ReservationId);
                    _logger.LogInformation("Pago confirmado y entradas decrementadas para reserva {ReservationId}", request.ReservationId);
                }
                var paymentDto = _mapper.Map<PaymentDto>(payment);
                return Ok(new
                {
                    message = "Pago procesado correctamente",
                    payment = paymentDto
                });
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Error procesando pago con Stripe");
                return BadRequest(new { error = "Error en el procesamiento del pago. Verifique los datos." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno procesando pago");
                return StatusCode(500, new { error = "Error interno del servidor." });
            }
        }

        /// <summary>
        /// Crea un Payment Intent en Stripe para iniciar un pago.
        /// Requiere autenticación.
        /// </summary>
        /// <param name="request">Datos para crear el intent, incluyendo Amount y Currency.</param>
        /// <returns>El clientSecret del Payment Intent.</returns>
        /// <response code="200">Payment Intent creado correctamente.</response>
        /// <response code="400">Datos inválidos.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPost("create-payment-intent")]
        [Authorize]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentDto request)
        {
            try
            {
                var paymentIntent = await _paymentService.CreatePaymentIntentAsync(request.Amount, request.Currency);
                return Ok(new { clientSecret = paymentIntent.ClientSecret});
              
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando Payment Intent");
                return StatusCode(500, new { error = "Error interno del servidor." });
            }
        }

        /// <summary>
        /// Maneja webhooks de Stripe para eventos de pago.
        /// No requiere autenticación para permitir recepción externa.
        /// </summary>
        /// <returns>Confirmación de recepción del webhook.</returns>
        /// <response code="200">Webhook procesado correctamente.</response>
        /// <response code="400">Error en la solicitud o validación.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPost("webhook")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
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

                // Procesar el evento en mi servicio, agregado: check para evitar duplicados si es necesario
                await _paymentService.ProcessWebhookAsync(stripeEvent);

                _logger.LogInformation("Webhook procesado correctamente: {Type}", stripeEvent.Type);
                return Ok(new { received = true });
                
            }
            catch (StripeException sex)
            {
                _logger.LogWarning(sex, "Error validando firma o formato del webhook de Stripe");
                return BadRequest(new { error = "Error en la validación del webhook." });
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

        /// <summary>
        /// Obtiene el historial de pagos. Para usuarios normales, devuelve sus propios pagos; para admins, todos los pagos.
        /// Requiere autenticación.
        /// </summary>
        /// <returns>Lista de pagos en formato DTO.</returns>
        /// <response code="200">Historial obtenido correctamente.</response>
        /// <response code="401">Usuario no autorizado.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet("history")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<PaymentDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(object), 500)]
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

                // Cambiado: Mapear a PaymentDto (DTO de salida)
                var dtos = payments.Select(p => _mapper.Map<PaymentDto>(p));
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo historial de pagos");
                return StatusCode(500, new { error = "Error interno del servidor." });
            }
        }
    }


}



