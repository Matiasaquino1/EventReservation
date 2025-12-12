using EventReservations.Models;
using EventReservations.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventReservations.Services
{
    public interface IEventService
    {
        Task<Event> CreateEventAsync(Event eventModel);
        Task<Event> GetEventAsync(int id);
        Task<Event> UpdateEventAsync(Event eventModel);
        Task DeleteEventAsync(int id);
        Task<IEnumerable<Event>> GetEventsWithFiltersAsync(DateTime? date, string location, int? availability);
        Task<Event> ForceConfirmEventAsync(int id);
        Task<IEnumerable<Event>> GetAllAsync();
        Task<Event?> GetEventByIdAsync(int id);

    }

        public class EventService : IEventService
        {
            private readonly IEventRepository _eventRepository;
            private readonly ILogger<EventService> _logger;

            public EventService(IEventRepository eventRepository, ILogger<EventService> logger)
            {
                _eventRepository = eventRepository;
                _logger = logger;
            }

            public async Task<Event> CreateEventAsync(Event eventModel)
            {
                if (eventModel == null) throw new ArgumentNullException(nameof(eventModel));
                if (string.IsNullOrWhiteSpace(eventModel.Title)) throw new ArgumentException("Title is required.");
                if (eventModel.TotalTickets <= 0) throw new ArgumentException("TotalTickets must be > 0.");

                // Initialize TicketsAvailable from TotalTickets (idempotent)
                eventModel.TicketsAvailable = eventModel.TotalTickets;
                eventModel.Status ??= "Active";
                eventModel.CreatedAt = eventModel.CreatedAt == default ? DateTime.UtcNow : eventModel.CreatedAt;

                var created = await _eventRepository.AddAsync(eventModel);
                _logger.LogInformation("Event created {EventId} ({Title}) with {TotalTickets} tickets", created.EventId, created.Title, created.TotalTickets);
                return created;
            }

            public async Task<Event> GetEventAsync(int id)
            {
                return await _eventRepository.GetByIdAsync(id);
            }

            public async Task<Event> UpdateEventAsync(Event eventModel)
            {
                if (eventModel == null) throw new ArgumentNullException(nameof(eventModel));

                var existing = await _eventRepository.GetByIdAsync(eventModel.EventId);
                if (existing == null) throw new KeyNotFoundException("Event not found.");

                // Apply updates: if TotalTickets changed, adjust TicketsAvailable accordingly
                if (eventModel.TotalTickets != existing.TotalTickets)
                {
                    var diff = eventModel.TotalTickets - existing.TotalTickets;
                    existing.TotalTickets = eventModel.TotalTickets;
                    existing.TicketsAvailable = Math.Max(0, existing.TicketsAvailable + diff);
                }

                // Update scalar properties
                existing.Title = eventModel.Title ?? existing.Title;
                existing.Description = eventModel.Description ?? existing.Description;
                existing.EventDate = eventModel.EventDate ?? existing.EventDate;
                existing.Location = eventModel.Location ?? existing.Location;
                existing.Price = eventModel.Price != default ? eventModel.Price : existing.Price;
                existing.Status = eventModel.Status ?? existing.Status;

                var updated = await _eventRepository.UpdateAsync(existing);
                _logger.LogInformation("Event updated {EventId}", updated.EventId);
                return updated;
            }

            public async Task DeleteEventAsync(int id)
            {
                await _eventRepository.DeleteAsync(id);
                _logger.LogInformation("Event deleted {EventId}", id);
            }

            public async Task<IEnumerable<Event>> GetEventsWithFiltersAsync(DateTime? date, string location, int? availability)
            {
                return await _eventRepository.GetEventsWithFiltersAsync(date, location, availability);
            }

            public async Task<Event> ForceConfirmEventAsync(int id)
            {
                var e = await _eventRepository.ForceConfirmEventAsync(id);
                if (e != null)
                {
                    e.Status = "Confirmed";
                    await _eventRepository.UpdateAsync(e);
                }
                return e;
            }

            public async Task<IEnumerable<Event>> GetAllAsync() => await _eventRepository.GetAllAsync();

            public async Task<Event?> GetEventByIdAsync(int id) => await _eventRepository.GetByIdAsync(id);
        }
}

