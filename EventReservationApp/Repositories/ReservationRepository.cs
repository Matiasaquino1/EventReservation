using EventReservations.Models;
using Microsoft.EntityFrameworkCore;

namespace EventReservations.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly ApplicationDbContext _context;
        public ReservationRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Reservation> CancelReservationAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                reservation.Status = "Cancelled";
                _context.Reservations.Update(reservation);
                await _context.SaveChangesAsync();
            }
            return reservation;
        }
        public async Task<IEnumerable<Reservation>> GetReservationsByUserAsync(int userId)
        {
            return await _context.Reservations.Where(r => r.UserId == userId).ToListAsync();
        }
        public async Task<IEnumerable<Reservation>> GetAdminReservationsAsync(string status, int? eventId)
        {
            var query = _context.Reservations.AsQueryable();
            if (!string.IsNullOrEmpty(status)) query = query.Where(r => r.Status == status);
            if (eventId.HasValue) query = query.Where(r => r.EventId == eventId.Value);
            return await query.ToListAsync();
        }

        public async Task<Reservation> GetByIdAsync(int id)
        {
            return await _context.Reservations.FindAsync(id);
        }
        public async Task<Reservation> AddAsync(Reservation reservation)
        {
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            return reservation;
        }
        public async Task<Reservation> UpdateAsync(Reservation reservation)
        {
            _context.Reservations.Update(reservation);
            await _context.SaveChangesAsync();
            return reservation;
        }
        public async Task DeleteAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<Reservation>> GetAllAsync()
        {
            return await _context.Reservations.ToListAsync();
        }
    }
}
