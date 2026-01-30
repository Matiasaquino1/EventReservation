namespace EventReservations.Models
{
    public class PaymentStatuses
    {
        public const string Pending = "Pending"; // creada, sin pagar
        public const string Succeeded = "Succeeded"; // pago ok
        public const string Failed = "Failed"; // Fallo el pago o cancelo el user 
    }
}
