namespace EventReservations.Dto
{
    public class UserReservationDto
    {
        public int ReservationId { get; set; }
        public string EventTitle { get; set; }
        public DateTime ReservationDate { get; set; }
        public string Status { get; set; }
    }
}
