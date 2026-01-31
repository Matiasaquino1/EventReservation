
using Stripe;

namespace EventReservations.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int ReservationId { get; set; }  // Relación con Reservation
        public PaymentStatuses? Status { get; set; }   // Ej: "Succeeded", "Failed", "Pending"
        public decimal Amount { get; set; }  // Snapshot 
        public DateTime PaymentDate { get; set; }  // Fecha del pago
        public string StripePaymentIntentId { get; set; } = string.Empty;
        public string? FailureReason { get; set; }

        public Reservation? Reservation { get; set; }
    }
}
