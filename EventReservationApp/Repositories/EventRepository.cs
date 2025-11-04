using EventReservations.Models;
using Microsoft.EntityFrameworkCore;
using EventReservations.Data;

namespace EventReservations.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _context;
        public EventRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Event>> GetEventsWithFiltersAsync(DateTime? date, string location, int? availability)
        {
            var query = _context.Events.AsQueryable();
            if (date.HasValue) query = query.Where(e => e.CreatedAt.Date == date.Value.Date);
            if (!string.IsNullOrEmpty(location)) query = query.Where(e => e.Location.Contains(location));
            if (availability.HasValue) query = query.Where(e => e.TicketsAvailable >= availability.Value);
            return await query.ToListAsync();
        }
        public async Task<Event> ForceConfirmEventAsync(int id)
        {
            var eventModel = await _context.Events.FindAsync(id);
            if (eventModel != null)
            {
                // Lógica simple: e.g., marcar como confirmado (agrega lógica personalizada)
                eventModel.Status = "Confirmed";  // Asume un campo Status en Event
                _context.Events.Update(eventModel);
                await _context.SaveChangesAsync();
            }
            return eventModel;
        }
        public async Task<Event> GetByIdAsync(int id)
        {
            return await _context.Events.FindAsync(id);
        }
        public async Task<Event> AddAsync(Event eventModel)
        {
            _context.Events.Add(eventModel);
            await _context.SaveChangesAsync();
            return eventModel;
        }

        public Task<Event> UpdateAsync(Event eventModel)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Event>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

    }
}