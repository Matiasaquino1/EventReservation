using EventReservations.Data;
using EventReservations.Models;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace EventReservations.Services
{
    public class IWebhookService
    {
        public interface IStripeWebhookService
        {
            Task HandleEventAsync(Stripe.Event stripeEvent);
        }
        public class StripeWebhookService : IStripeWebhookService
        {
            private readonly ApplicationDbContext _context;
            private readonly ILogger<StripeWebhookService> _logger;

            public StripeWebhookService(
                ApplicationDbContext context,
                ILogger<StripeWebhookService> logger)
            {
                _context = context;
                _logger = logger;
            }

            public async Task HandleEventAsync(Stripe.Event stripeEvent)
            {
                if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
                {
                    var intent = stripeEvent.Data.Object as PaymentIntent ?? throw new InvalidOperationException("Evento Stripe inválido");
                    await HandlePaymentSucceeded(intent);
                }
            }

            private async Task HandlePaymentSucceeded(PaymentIntent intent)
            {
                using var tx = await _context.Database.BeginTransactionAsync();

                var reservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.PaymentIntentId == intent.Id);

                if (reservation == null || reservation.Status == ReservationStatuses.Confirmed)
                    return;

                var paymentExists = await _context.Payments
                    .AnyAsync(p => p.StripePaymentIntentId == intent.Id);

                if (!paymentExists)
                {
                    _context.Payments.Add(new Payment
                    {
                        ReservationId = reservation.ReservationId,
                        Amount = reservation.Amount,
                        Status = PaymentStatuses.Succeeded,
                        StripePaymentIntentId = intent.Id,
                        PaymentDate = DateTime.UtcNow
                    });
                }

                reservation.Status = ReservationStatuses.Confirmed;

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                _logger.LogInformation(
                    "Reserva {ReservationId} confirmada por webhook",
                    reservation.ReservationId
                );
            }
        }

    }
}
