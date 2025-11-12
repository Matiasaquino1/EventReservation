using EventReservations.Data;
using EventReservations.Models;
using Microsoft.EntityFrameworkCore;
using EventReservations.Dto;


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
                reservation.ReservationDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return reservation;
        }
        public async Task<IEnumerable<Reservation>> GetReservationsByUserAsync(int userId)
        {
            return await _context.Reservations.Where(r => r.UserId == userId).ToListAsync();
        }
        public async Task<(IEnumerable<Reservation> Data, int TotalRecords)> GetAdminReservationsAsync(
            string? status,
            int? eventId,
            int page,
            int pageSize,
            string sort)
        {
            var query = _context.Reservations
                .Include(r => r.EventId)
                .Include(r => r.UserId)
                .AsQueryable();

            // Filtros
            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);

            if (eventId.HasValue)
                query = query.Where(r => r.EventId == eventId.Value);

            // Total antes de paginar
            var totalRecords = await query.CountAsync();

            // Ordenamiento
            query = sort.ToLower() == "desc"
                ? query.OrderByDescending(r => r.CreatedAt)
                : query.OrderBy(r => r.CreatedAt);

            // Paginación
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalRecords);
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

        public async Task<IEnumerable<Reservation>> GetAllAsync(string? status = null, int? eventId = null)
        {
            var q = _context.Reservations.AsQueryable();
            if (!string.IsNullOrEmpty(status)) q = q.Where(r => r.Status == status);
            if (eventId.HasValue) q = q.Where(r => r.EventId == eventId.Value);
            return await q.ToListAsync();
        }

    }
}
