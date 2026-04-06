namespace EventReservations.Dto
{
    public class ConfirmationEmailDataDto
    {
        public string ToEmail { get; set; }
        public string UserName { get; set; }
        public string EventTitle { get; set; }
        public DateTime? EventDate { get; set; }
        public int NumberOfTickets { get; set; }
        public decimal Amount { get; set; }
        public int ReservationId { get; set; }
    }
}
