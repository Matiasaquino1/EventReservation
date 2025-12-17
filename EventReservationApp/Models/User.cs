using EventReservations.Enums;

namespace EventReservations.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.UtcNow;

        public ICollection<Reservation> Reservations { get; set; }

    }

}
