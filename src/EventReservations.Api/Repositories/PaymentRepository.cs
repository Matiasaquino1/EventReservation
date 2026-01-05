using EventReservations.Models;
using EventReservations.Data;
using Microsoft.EntityFrameworkCore;

namespace EventReservations.Repositories
{
    public class PaymentRepository(ApplicationDbContext context) : IPaymentRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<Payment> GetByIdAsync(int id)
        {
            return await _context.Payments.FindAsync(id);
        }
        public async Task<Payment> AddAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            return payment;
        }
        public async Task<Payment> UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            return payment;
        }
        public async Task DeleteAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
            }
        }

        public Task<Payment> UpdatePaymentStatusAsync(string id, string v)
        {
            throw new NotImplementedException();
        }

        public async Task<Payment?> GetByStripeIntentIdAsync(string stripePaymentIntentId)
        {
            if (string.IsNullOrEmpty(stripePaymentIntentId))
                throw new ArgumentException("El ID de Stripe no puede ser nulo o vacío.", nameof(stripePaymentIntentId));

            return await _context.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == stripePaymentIntentId);
        }

        public async Task<Payment?> GetByReservationIdAsync(int reservationId)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.ReservationId == reservationId);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId)
        {
            // Payments.ReservationId relaciona a Reservation
            return await _context.Payments
                .Where(p => _context.Reservations.Any(r => r.ReservationId == p.ReservationId && r.UserId == userId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments.ToListAsync();
        }

    }
}
    
