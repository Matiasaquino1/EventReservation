using EventReservations.Models;

namespace EventReservations.Repositories
{
    public interface IStripeWebhookEventRepository
    {
        Task<bool> ExistsAsync(string stripeEventId);
        Task AddAsync(StripeWebhookEvent stripeEvent);
    }

}
