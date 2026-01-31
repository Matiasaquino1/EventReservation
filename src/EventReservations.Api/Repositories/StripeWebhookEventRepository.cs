using EventReservations.Data;
using EventReservations.Models;
using Microsoft.EntityFrameworkCore;

namespace EventReservations.Repositories
{
    public class StripeWebhookEventRepository(ApplicationDbContext context)
    : IStripeWebhookEventRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<bool> ExistsAsync(string stripeEventId)
            => await _context.StripeWebhookEvents
                .AnyAsync(e => e.StripeEventId == stripeEventId);

        public async Task AddAsync(StripeWebhookEvent stripeEvent)
        {
            _context.StripeWebhookEvents.Add(stripeEvent);
            await _context.SaveChangesAsync();
        }
    }

}
