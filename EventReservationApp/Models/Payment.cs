using EventReservations.Enums;

namespace EventReservations.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int ReservationId { get; set; }  // Relación con Reservation
        public PaymentStatus Status { get; set; }  // Ej: "Succeeded", "Failed", "Pending"
        public decimal Amount { get; set; }  // Monto del pago
        public DateTime PaymentDate { get; set; }  // Fecha del pago
        public string StripePaymentIntentId { get; set; } = string.Empty;

        public Reservation Reservation { get; set; }
    }
}
