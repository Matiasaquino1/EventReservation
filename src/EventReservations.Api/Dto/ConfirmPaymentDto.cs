using System.Text.Json.Serialization; 

namespace EventReservations.Dto
{
    public class ConfirmPaymentDto
    {
        [JsonPropertyName("paymentIntentId")] 
        public string PaymentIntentId { get; set; } = string.Empty;
    }
}
