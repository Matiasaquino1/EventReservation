namespace EventReservations.Dto
{
    public class UpdateEventDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? EventDate { get; set; }
        public string? Location { get; set; }
        public decimal? Price { get; set; }
        public int? TotalTickets { get; set; } 
        public string? Status { get; set; }
        public int Id { get; internal set; }
    }
}
