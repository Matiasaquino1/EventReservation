namespace EventReservations.Dto
{
    public class CreateEventDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public DateTime EventDate { get; set; }
        public int TicketsAvailable { get; set; }
    }
    public class UpdateEventDto : CreateEventDto 
    {
        public int Id { get; set; }
    }
}
