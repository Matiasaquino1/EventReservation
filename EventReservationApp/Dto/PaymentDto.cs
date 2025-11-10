namespace EventReservations.Dto
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public required string Currency { get; set; }
        public required string Status { get; set; } // Ejemplo de campo adicional

    }
}
