using System.Text.Json.Serialization;

namespace EventReservations.Dto
{
    public class ConfirmPaymentDto
    {
        [JsonPropertyName("paymentIntentId")]
        public required string PaymentIntentId { get => paymentIntentId; set => paymentIntentId = value; }
    }
}
