namespace EventReservations.Dto
{
    public class PaymentRequestDto
    {
        public int ReservationId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public string PaymentMethodId { get; set; } = string.Empty;
    }
}
