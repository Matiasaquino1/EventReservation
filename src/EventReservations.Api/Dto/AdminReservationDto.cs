namespace EventReservations.Dto
{
    public class AdminReservationDto : ReservationDto
    {
        public string Email { get; set; }
        public string EventTitle { get; set; }
        public string OrganizerEmail { get; set; }

    }
}
