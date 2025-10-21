using EventReservations.Dto;
using EventReservations.Models;
using EventReservations.Repositories;
using System.Threading.Tasks;

namespace EventReservations.Services
{
    public interface IReservationService
    {
        Task<Reservation> CreateReservationAsync(Reservation reservation);
        Task<Reservation> CancelReservationAsync(int id);
        Task<IEnumerable<Reservation>> GetReservationsByUserAsync(int userId);
        Task<IEnumerable<Reservation>> GetAdminReservationsAsync(string status, int? eventId);
        Task<Reservation> UpdateReservationAsync(Reservation createdReservation);
        // Agrega más métodos como GetReservationAsync, UpdateAsync, etc.
    }

    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;  // Inyecta el repositorio
        public async Task<Reservation> CancelReservationAsync(int id)
        {
            // Lógica de negocio: e.g., verificar si ya está pagada
            return await _reservationRepository.CancelReservationAsync(id);
        }
        public async Task<IEnumerable<Reservation>> GetReservationsByUserAsync(int userId)
        {
            return await _reservationRepository.GetReservationsByUserAsync(userId);
        }
        public async Task<IEnumerable<Reservation>> GetAdminReservationsAsync(string status, int? eventId)
        {
            return await _reservationRepository.GetAdminReservationsAsync(status, eventId);
        }

        public ReservationService(IReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }

        public async Task<Reservation> CreateReservationAsync(Reservation reservation)
        {
            // Agrega lógica de negocio, como verificar disponibilidad de entradas
            return await _reservationRepository.AddAsync(reservation);  // Usa el repositorio
        }

        public async Task<Reservation> UpdateReservationAsync(Reservation createdReservation)
        {
            
            return await _reservationRepository.UpdateAsync(createdReservation);

        }

        // Agrega más métodos si los necesitas, e.g.:
        // public async Task<Reservation> GetReservationAsync(int id) { return await _reservationRepository.GetByIdAsync(id); }
    }
}