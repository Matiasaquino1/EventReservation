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

                // Mapear a DTO para la respuesta
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
            string json;
            try
            {
                using var reader = new StreamReader(HttpContext.Request.Body);
                json = await reader.ReadToEndAsync();
                var stripeSignature = Request.Headers["Stripe-Signature"].FirstOrDefault();
                var webhookSecret = _configuration["Stripe:WebhookSecret"];

                Stripe.Event stripeEvent;

                if (!string.IsNullOrEmpty(webhookSecret) && !string.IsNullOrEmpty(stripeSignature))
                {
                    stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);
                }
                else
                {
                    // Fallback: deserializar sin verificar firma (solo para dev, no recomendado en prod)
                    stripeEvent = JsonConvert.DeserializeObject<Stripe.Event>(json);
                }

                await _paymentService.ProcessWebhookAsync(stripeEvent);

                return Ok();
            }
            catch (StripeException sex)
            {
                _logger.LogWarning(sex, "Stripe webhook error");
                return BadRequest(new { error = sex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando webhook de Stripe");
                return StatusCode(500, new { error = ex.Message });
            }
        }

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


        // POST: api/payments/webhook
        //[HttpPost("webhook")]
        //[AllowAnonymous]
        //public async Task<IActionResult> Webhook()
        //{
        //    var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        //    var stripeEvent = EventUtility.ConstructEvent(json, new Dictionary<string, string>(), new StripeOptions().ApiKey);

        //    if (stripeEvent.Type == "checkout.session.completed" || stripeEvent.Type == "payment_intent.succeeded")
        //    {
        //        await _paymentService.ProcessWebhookAsync(stripeEvent);
        //        return Ok();
        //    }

        //    return BadRequest();
        //}
    }
}



