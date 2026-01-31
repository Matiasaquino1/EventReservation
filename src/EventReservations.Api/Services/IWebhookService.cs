using EventReservations.Data;
using EventReservations.Models;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace EventReservations.Services
{
    public interface IStripeWebhookService
    {
        Task HandleEventAsync(Stripe.Event stripeEvent);

    }
    public class StripeWebhookService(
        ApplicationDbContext context,
        ILogger<StripeWebhookService> logger) : IStripeWebhookService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<StripeWebhookService> _logger = logger;

        // ===============================
        // ENTRY POINT
        // ===============================
        public async Task HandleEventAsync(Stripe.Event stripeEvent)
        {
            // IDEMPOTENCIA POR EVENT ID
            var alreadyProcessed = await _context.StripeWebhookEvents
                .AnyAsync(e => e.StripeEventId == stripeEvent.Id);

            if (alreadyProcessed)
            {
                _logger.LogInformation(
                    "Webhook duplicado ignorado: {EventId}",
                    stripeEvent.Id
                );
                return;
            }

            if (stripeEvent.Data.Object is not PaymentIntent intent)
                return;

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                switch (stripeEvent.Type)
                {
                    case EventTypes.PaymentIntentSucceeded:
                        await HandlePaymentSucceededAsync(intent);
                        break;

                    case EventTypes.PaymentIntentPaymentFailed:
                        await HandlePaymentFailedAsync(intent);
                        break;
                }

                // Registrar evento procesado
                _context.StripeWebhookEvents.Add(new StripeWebhookEvent
                {
                    StripeEventId = stripeEvent.Id,
                    Type = stripeEvent.Type,
                    ProcessedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ===============================
        // PAYMENT SUCCEEDED
        // ===============================
        private async Task HandlePaymentSucceededAsync(PaymentIntent intent)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == intent.Id);

            if (payment == null)
            {
                _logger.LogWarning(
                    "PaymentIntent {IntentId} sin pago asociado",
                    intent.Id
                );
                return;
            }

            if (payment.Status == PaymentStatuses.Succeeded)
                return; // idempotencia de dominio

            payment.Status = PaymentStatuses.Succeeded;
            payment.PaymentDate = DateTime.UtcNow;

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ReservationId == payment.ReservationId);

            if (reservation != null &&
                reservation.Status == ReservationStatuses.Pending)
            {
                reservation.Status = ReservationStatuses.Confirmed;
            }

            _logger.LogInformation(
                "Pago confirmado por webhook. Reserva {ReservationId}",
                payment.ReservationId
            );
        }

        // ===============================
        // PAYMENT FAILED
        // ===============================
        private async Task HandlePaymentFailedAsync(PaymentIntent intent)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == intent.Id);

            if (payment == null)
                return;

            payment.Status = PaymentStatuses.Failed;

            _logger.LogWarning(
                "Pago fallido por webhook. PaymentIntent {IntentId}",
                intent.Id
            );
        }

    }   
}
