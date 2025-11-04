namespace EventReservations.Dto
{
    public class EventDto
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EventDate { get; set; } = DateTime.UtcNow;
        public string Location { get; set; }
        public int TicketsAvailable { get; set; }
    }
}
