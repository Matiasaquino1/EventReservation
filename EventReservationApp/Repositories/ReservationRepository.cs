using EventReservations.Data;
using EventReservations.Models;
using Microsoft.EntityFrameworkCore;
using EventReservations.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


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

        /// <summary>
        /// Implementa paginacion: Muestra una lista de las reservas paginada con filtros y orden.
        /// Se requiere rol Admin
        /// </summary>
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

        public async Task<IEnumerable<Reservation>> GetReservationsByUserAndEventAsync(int userId, int eventId)
        {
            return await _context.Reservations
                .Where(r => r.UserId == userId && r.EventId == eventId)
                .ToListAsync();  
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

        /// <summary>
        /// Implementa GetPagedReservationsAsync: Construye una consulta paginada con filtros y orden directamente en el repository.
        /// </summary>
        public async Task<PagedResponseDto<Reservation>> GetPagedReservationsAsync(int page, int pageSize, string sort, string status, int? eventId)
        {
            // Validar y ajustar parámetros
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;
            // Paso 2: Obtener consulta base
            var query = _context.Reservations.AsQueryable();
            // Paso 3: Aplicar filtros
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }
            if (eventId.HasValue)
            {
                query = query.Where(r => r.EventId == eventId.Value);
            }
            // Aplicar orden
            query = string.Equals(sort, "asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(r => r.ReservationDate)
                : query.OrderByDescending(r => r.ReservationDate);
            // Calcula total
            var totalCount = await query.CountAsync();
            // Aplica paginación
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            // Retorna respuesta paginada
            return new PagedResponseDto<Reservation>
            {
                Data = data,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}
