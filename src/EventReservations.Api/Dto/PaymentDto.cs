using EventReservations.Models;

namespace EventReservations.Dto
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public required string Currency { get; set; }
        public PaymentStatuses Status { get; set; }
        // Ejemplo de campo adicional

    }
}
