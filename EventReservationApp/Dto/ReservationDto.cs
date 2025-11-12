namespace EventReservations.Dto
{
    public class ReservationDto
    {
        public int ReservationId { get; set; }  
        public int UserId { get; set; }
        public int EventId { get; set; }
        public string? Status { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
