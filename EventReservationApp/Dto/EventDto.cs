namespace EventReservations.Dto
{
    public class EventDto
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public DateTime DateTime { get; set; }
        public string Location { get; set; }
        public int TicketsAvailable { get; set; }
    }
}
