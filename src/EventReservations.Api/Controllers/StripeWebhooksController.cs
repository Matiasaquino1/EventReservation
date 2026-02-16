using EventReservations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace EventReservations.Controllers
{
    [ApiController]
    [Route("api/webhooks/stripe")]
    public class StripeWebhooksController : ControllerBase
    {
        private readonly IStripeWebhookService _stripeWebhookService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeWebhooksController> _logger;

        public StripeWebhooksController(
            IStripeWebhookService stripeWebhookService,
            IConfiguration configuration,
            ILogger<StripeWebhooksController> logger)
        {
            _stripeWebhookService = stripeWebhookService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Handle()
        {
            string json;

            using (var reader = new StreamReader(HttpContext.Request.Body))
            {
                json = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogWarning("Webhook Stripe recibido con body vacío.");
                return BadRequest();
            }

            try
            {
                var stripeSignature = Request.Headers["Stripe-Signature"].FirstOrDefault();
                var webhookSecret = _configuration["Stripe:WebhookSecret"];

                Stripe.Event stripeEvent;

                if (!string.IsNullOrEmpty(webhookSecret) && !string.IsNullOrEmpty(stripeSignature))
                {
                    stripeEvent = EventUtility.ConstructEvent(
                        json,
                        stripeSignature,
                        webhookSecret
                    );
                }
                else
                {
                    // local
                    _logger.LogWarning("Webhook Stripe sin verificación de firma (DEV).");
                    stripeEvent = EventUtility.ParseEvent(json);
                }

                // Delegar lógica al dominio
                await _stripeWebhookService.HandleEventAsync(stripeEvent);

                return Ok(new { received = true });
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Error Stripe procesando webhook.");
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno procesando webhook Stripe.");
                return StatusCode(500);
            }
        }
    }
}
