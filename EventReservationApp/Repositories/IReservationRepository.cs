using EventReservations.Models;


namespace EventReservations.Repositories
{
    public interface IReservationRepository
    {
        Task<Reservation> CancelReservationAsync(int id);
        Task<Reservation> GetByIdAsync(int id);
        Task<Reservation> AddAsync(Reservation reservation);
        Task<Reservation> UpdateAsync(Reservation reservation);
        Task DeleteAsync(int id);
        Task<IEnumerable<Reservation>> GetAllAsync(string? status = null, int? eventId = null);
        Task<IEnumerable<Reservation>> GetReservationsByUserAsync(int userId);
        Task<(IEnumerable<Reservation> Data, int TotalRecords)> GetAdminReservationsAsync(string? status, int? eventId, int page, int pageSize, string sort);
        Task<IEnumerable<Reservation>> GetReservationsByUserAndEventAsync(int userId, int eventId);
    }
}



