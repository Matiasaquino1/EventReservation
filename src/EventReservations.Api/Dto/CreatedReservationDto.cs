namespace EventReservations.Dto
{
    public class CreatedReservationDto
    {
        public int EventId { get; set; }
        public decimal TotalAmount { get; set; }  // Para calcular pago
    }
}
