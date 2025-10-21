namespace EventReservations.Dto
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string Status { get; set; }
        public DateTime ReservationDate { get; set; }
    }
}
