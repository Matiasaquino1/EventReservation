using EventReservations.Models;
using Stripe;


namespace EventReservations.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment> GetByIdAsync(int id);
        Task<Payment> AddAsync(Payment payment);
        Task<Payment> UpdateAsync(Payment payment);
        Task<Payment> UpdatePaymentStatusAsync(String id, string v);
        Task DeleteAsync(int id);
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<Payment?> GetByStripeIntentIdAsync(string stripePaymentIntentId);
        Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId);
        Task<Payment?> GetByReservationIdAsync(int reservationId);
    }
}
