using EventReservations.Models;
using EventReservations.Repositories;

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
    }

    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;  
        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }
        public async Task<IEnumerable<Event>> GetEventsWithFiltersAsync(DateTime? date, string location, int? availability)
        {
            // validaciones
            return await _eventRepository.GetEventsWithFiltersAsync(date, location, availability);
        }
        public async Task<Event> ForceConfirmEventAsync(int id)
        {
            // logica negociocf
            return await _eventRepository.ForceConfirmEventAsync(id);
        }

        public async Task<Event> CreateEventAsync(Event eventModel)
        {
            // Aquí puedes agregar lógica de negocio adicional, como validaciones
            return await _eventRepository.AddAsync(eventModel);  // Usa el repositorio
        }

        public async Task<Event> GetEventAsync(int id)
        {
            return await _eventRepository.GetByIdAsync(id);  // Usa el repositorio
        }

        public async Task<Event> UpdateEventAsync(Event eventModel)
        {
            // Lógica de negocio: e.g., validaciones
            return await _eventRepository.UpdateAsync(eventModel);  // Asume que IEventRepository tiene UpdateAsync
        }
        public async Task DeleteEventAsync(int id)
        {
            await _eventRepository.DeleteAsync(id);  // Asume que IEventRepository tiene DeleteAsync
        }

        public async Task<IEnumerable<Event>> GetAllAsync()
        {
            return await _eventRepository.GetAllAsync();
        }

    }
}
