namespace EventReservations.Models
{
    public class Reservation
    {
        public int ReservationId { get; set; }
        public int UserId { get; set; }  // Relación con User
        public int EventId { get; set; }  // Relación con Event
        public string Status { get; set; }  // Ej: "Pending", "Confirmed", "Cancelled"
        public DateTime ReservationDate { get; set; }  // Fecha de la reserva
    }
}
