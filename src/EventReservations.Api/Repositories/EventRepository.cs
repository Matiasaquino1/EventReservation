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
                // Lógica simple: marca como confirmado 
                eventModel.Status = "Confirmed";  
                _context.Events.Update(eventModel);
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
            return eventModel;
        }

        public async Task<Event> UpdateAsync(Event eventModel)
        {
            _context.Events.Update(eventModel);  
            return eventModel; 
        }

        public async Task DeleteAsync(int id)  
        {
            var eventModel = await _context.Events.FindAsync(id);  
            if (eventModel != null)
            {
                _context.Events.Remove(eventModel); 
            }
        }

        public async Task<IEnumerable<Event>> GetAllAsync()
        {
            return await _context.Events.ToListAsync();
        }

    }
}