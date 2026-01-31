using System.ComponentModel.DataAnnotations; // Para validaciones

namespace EventReservations.Dto
{
    public class CreatePaymentIntentDto
    {
        [Required]
        public int ReservationId { get; set; } 
    }

}

