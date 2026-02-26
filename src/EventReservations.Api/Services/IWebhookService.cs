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

        public async Task HandleEventAsync(Stripe.Event stripeEvent)
        {
            var alreadyProcessed = await _context.StripeWebhookEvents
                .AnyAsync(e => e.StripeEventId == stripeEvent.Id);

            if (alreadyProcessed)
            {
                _logger.LogInformation("Webhook duplicado ignorado: {EventId}", stripeEvent.Id);
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

                _context.StripeWebhookEvents.Add(new StripeWebhookEvent
                {
                    StripeEventId = stripeEvent.Id,
                    Type = stripeEvent.Type,
                    ProcessedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogCritical("¡CONFLICTO DE COMPRA! El usuario pagó pero ya no hay stock para la reserva {Id}", stripeEvent.Id);
                await tx.RollbackAsync();
                throw;
            }
        }

        private async Task HandlePaymentSucceededAsync(PaymentIntent intent)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == intent.Id);

            if (payment == null)
            {
                _logger.LogWarning("PaymentIntent {IntentId} sin pago asociado", intent.Id);
                return;
            }

            if (payment.Status != PaymentStatuses.Succeeded)
            {
                payment.Status = PaymentStatuses.Succeeded;
                payment.PaymentDate = DateTime.UtcNow;
            }

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ReservationId == payment.ReservationId);

            if (reservation == null)
            {
                _logger.LogWarning("Reserva no encontrada para pago {PaymentId}", payment.PaymentId);
                return;
            }

            if (reservation.Status == ReservationStatuses.Pending)
            {
                var eventModel = await _context.Events
                    .FirstOrDefaultAsync(e => e.EventId == reservation.EventId)
                    ?? throw new InvalidOperationException("Evento no encontrado para confirmar reserva.");

                if (eventModel.TicketsAvailable < reservation.NumberOfTickets)
                {
                    throw new InvalidOperationException(
                        $"Entradas insuficientes para confirmar reserva {reservation.ReservationId} desde webhook.");
                }

                eventModel.TicketsAvailable -= reservation.NumberOfTickets;
                if (eventModel.TicketsAvailable == 0)
                {
                    eventModel.Status = "SoldOut";
                }

                reservation.Status = ReservationStatuses.Confirmed;
            }

            _logger.LogInformation(
                "Pago confirmado por webhook. Reserva {ReservationId}, EventId {EventId}",
                reservation.ReservationId,
                reservation.EventId);
            
        }

        private async Task HandlePaymentFailedAsync(PaymentIntent intent)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == intent.Id);

            if (payment == null)
                return;

            payment.Status = PaymentStatuses.Failed;

            _logger.LogWarning(
                "Pago fallido por webhook. PaymentIntent {IntentId}",                
                intent.Id);
        }

    }   
}
