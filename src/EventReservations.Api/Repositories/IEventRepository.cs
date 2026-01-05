using EventReservations.Models;

namespace EventReservations.Repositories
{
    public interface IEventRepository
    {
        Task<Event> GetByIdAsync(int id);
        Task<Event> AddAsync(Event eventModel);
        Task<IEnumerable<Event>> GetEventsWithFiltersAsync(DateTime? date, string location, int? availability);
        Task<Event> ForceConfirmEventAsync(int id);
        Task<Event> UpdateAsync(Event eventModel);
        Task DeleteAsync(int id);
        Task<IEnumerable<Event>> GetAllAsync();

    }
}
