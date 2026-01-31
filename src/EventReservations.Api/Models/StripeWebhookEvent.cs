namespace EventReservations.Models
{
    public class StripeWebhookEvent
    {
        public int Id { get; set; }

        public string StripeEventId { get; set; } = null!;
        public string Type { get; set; } = null!;
        public DateTime ProcessedAt { get; set; }
    }

}
