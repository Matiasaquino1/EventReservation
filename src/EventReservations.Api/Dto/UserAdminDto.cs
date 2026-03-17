namespace EventReservations.Dto
{
    public class UserAdminDto
    {
        public int UserId { get; set; }
        public  string Name { get; set; }
        public  string Email { get; set; }
        public  string Role { get; set; }
        // Incluimos una lista simplificada de sus reservas
        public List<UserReservationDto> Reservations { get; set; }
    }
}
