
namespace EventReservations.Models
{
    public class Reservation
    {
        public int ReservationId { get; set; }

        public int UserId { get; set; }  // Relación con User
        public int EventId { get; set; }  // Relación con Event
        public string? PaymentIntentId { get; set; } // ID del intento de pago en Stripe

        public ReservationStatuses Status { get; set; } = ReservationStatuses.Pending;  // "Pending" = creada, sin pagar, "Confirmed"= pago ok, "Cancelled"= cancelada por usuario.
        public DateTime ReservationDate { get; set; }  // Fecha de la reserva
        public DateTime CreatedAt { get; set; } //Fecha de creada/realizada la reserva
        public int NumberOfTickets { get; set; } // Cantidad de tickets
        public decimal Amount { get; set; }  // Monto del pago


        public User User { get; set; }
        public Event Event { get; set; }

        public ICollection<Payment> Payments { get; set; }

    }
}
