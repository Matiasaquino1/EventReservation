using System.ComponentModel.DataAnnotations;

namespace EventReservations.Dto
{
    public class UpdateUserDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "User";
    }
}
