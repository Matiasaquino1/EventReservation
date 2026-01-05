namespace EventReservations.Dto
{
    public class CreateEventDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime EventDate { get; set; }
        public decimal Price { get; set; }
        public int TotalTickets { get; set; }
    }
}
