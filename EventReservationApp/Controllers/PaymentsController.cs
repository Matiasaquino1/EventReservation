using AutoMapper;
using EventReservations.Dto;
using EventReservations.Profiles;
using EventReservations.Models;
using EventReservations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace EventReservations.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IMapper _mapper;

        public PaymentsController(IPaymentService paymentService, IMapper mapper)
        {
            _paymentService = paymentService;
            _mapper = mapper;
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



