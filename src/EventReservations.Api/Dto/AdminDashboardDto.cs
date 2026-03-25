namespace EventReservations.Dto
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalReservations { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<EventStatDto> TopEvents { get; set; } = new();
    }
}
