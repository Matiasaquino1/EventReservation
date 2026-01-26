namespace EventReservations.Models
{
    public class ReservationStatuses
    {
        public const string Pending = "Pending"; // creada, sin pagar
        public const string Confirmed = "Confirmed"; // "Confirmed"= pago ok
        public const string Cancelled = "Cancelled"; //"Cancelled"= cancelada por usuario. 
    }
}
