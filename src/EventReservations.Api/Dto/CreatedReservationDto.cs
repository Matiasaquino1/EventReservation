namespace EventReservations.Dto
{
    public class CreatedReservationDto
    {
        public int EventId { get; set; }
        public int NumberOfTickets { get; set; }
        public decimal Amount { get; set; }
    }
}
