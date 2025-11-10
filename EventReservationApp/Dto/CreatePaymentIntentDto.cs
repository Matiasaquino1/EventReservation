using System.ComponentModel.DataAnnotations; // Para validaciones

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

    // Campos adicionales sugeridos para la reserva (agrega si AutoMapper los necesita)
    [Required]
    public int EventId { get; set; } // ID del evento a reservar

    [Required]
    public int UserId { get; set; } // ID del usuario (puedes obtenerlo del token en el controlador)

    // Otros campos opcionales (e.g., NumberOfTickets)
    public int NumberOfTickets { get; set; } = 1;
}
