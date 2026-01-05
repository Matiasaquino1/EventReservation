namespace EventReservations.Dto
{
    public class EventDto
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public decimal Price { get; set; }
        public int TotalTickets { get; set; }
        public int TicketsAvailable { get; set; }
    }
}
