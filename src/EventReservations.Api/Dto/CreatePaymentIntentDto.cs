using System.ComponentModel.DataAnnotations; // Para validaciones

namespace EventReservations.Dto
{
    public class CreatePaymentIntentDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0.")]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "usd";

        [Required]
        [MinLength(1, ErrorMessage = "PaymentMethodId es requerido.")]
        public string PaymentMethodId { get; set; } = string.Empty;

        [Required]
        public int EventId { get; set; } 
        public int NumberOfTickets { get; set; } = 1;
    }

}

