using EventReservations.Dto;
using EventReservations.Models;
using EventReservations.Repositories;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace EventReservations.Services
{
    public interface IReservationService
    {
        Task<Reservation> CreateReservationAsync(Reservation reservation, bool checkAvailability = true);
        Task<Reservation> CancelReservationAsync(int id);
        Task<IEnumerable<Reservation>> GetReservationsByUserAsync(int userId);
        Task<(IEnumerable<Reservation> Data, int TotalRecords)> GetAdminReservationsAsync(string? status, int? eventId, int page,  int pageSize, string sort);
        Task<Reservation> UpdateReservationAsync(Reservation createdReservation);
        Task<bool> IsDuplicateReservationAsync(int userId, int eventId);
        Task<Reservation> DeleteReservation(int id);
        Task<Reservation> GetReservationAsync(int id);
        Task<IEnumerable<Reservation>> GetAllReservationsAsync(string? status = null, int? eventId = null);
        Task<IEnumerable<Reservation>> GetReservationsByUserAndEventAsync(int userId, int eventId);
        Task ConfirmPaymentAndDecrementTicketsAsync(int reservationId);
    }

    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;  // Inyecta el repositorio
        private readonly IEventRepository _eventRepository;  // Para verificar y actualizar eventos
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(IReservationRepository reservationRepository, IEventRepository eventRepository, ILogger<ReservationService> logger)
        {
            _reservationRepository = reservationRepository;
            _eventRepository = eventRepository;
            _logger = logger;
        }

        public async Task<Reservation> CancelReservationAsync(int id)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation != null && reservation.Status != "Cancelled")
            {
                reservation.Status = "Cancelled";
                await _reservationRepository.UpdateAsync(reservation);

                // Reabrir cupos
                var eventModel = await _eventRepository.GetByIdAsync(reservation.EventId);
                if (eventModel != null)
                {
                    eventModel.TicketsAvailable++;
                    if (eventModel.Status == "Full") eventModel.Status = "Active";  
                    await _eventRepository.UpdateAsync(eventModel);
                    _logger.LogInformation("Reserva {ReservationId} cancelada, cupos reabiertos en evento {EventId}", id, reservation.EventId);
                }
            }
            return reservation;
        }

        public async Task<IEnumerable<Reservation>> GetReservationsByUserAsync(int userId)
        {
            return await _reservationRepository.GetReservationsByUserAsync(userId);
        }

        public async Task<(IEnumerable<Reservation> Data, int TotalRecords)> GetAdminReservationsAsync(
            string? status,
            int? eventId,
            int page,
            int pageSize,
            string sort)
        {
            return await _reservationRepository.GetAdminReservationsAsync(status, eventId, page, pageSize, sort);
        }

        public ReservationService(IReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }

        public async Task<Reservation> CreateReservationAsync(Reservation reservation, bool checkAvailability = true)
        {
            if (await IsDuplicateReservationAsync(reservation.UserId, reservation.EventId))
            {
                _logger.LogWarning("Intento de reserva duplicada: Usuario {UserId} para Evento {EventId}", reservation.UserId, reservation.EventId);
                throw new InvalidOperationException("Ya tienes una reserva para este evento.");
            }
            if (checkAvailability)
            {
                var eventModel = await _eventRepository.GetByIdAsync(reservation.EventId);
                if (eventModel == null || eventModel.TicketsAvailable <= 0)
                {
                    _logger.LogWarning("Evento {EventId} sin entradas disponibles", reservation.EventId);
                    throw new InvalidOperationException("No hay entradas disponibles para este evento.");
                }
            }
            // Crear reserva (sin decrementar aún)
            var created = await _reservationRepository.AddAsync(reservation);
            _logger.LogInformation("Reserva creada con ID {ReservationId} para usuario {UserId}", created.ReservationId, created.UserId);
            return created;
        }

        public async Task<bool> IsDuplicateReservationAsync(int userId, int eventId)
        {
            var existing = await _reservationRepository.GetReservationsByUserAndEventAsync(userId, eventId);
            return existing.Any();
        }

        public async Task UpdateReservationAsync(Reservation reservation)
        {
            await _reservationRepository.UpdateAsync(reservation);
            _logger.LogInformation("Reserva {ReservationId} actualizada a status {Status}", reservation.ReservationId, reservation.Status);
        }

        public async Task ConfirmPaymentAndDecrementTicketsAsync(int reservationId)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation != null && reservation.Status == "Pending")
            {
                var eventModel = await _eventRepository.GetByIdAsync(reservation.EventId);
                if (eventModel != null && eventModel.TicketsAvailable > 0)
                {
                    eventModel.TicketsAvailable--;
                    if (eventModel.TicketsAvailable == 0) eventModel.Status = "Full";
                    await _eventRepository.UpdateAsync(eventModel);
                    reservation.Status = "Confirmed";
                    await UpdateReservationAsync(reservation);
                    _logger.LogInformation("Pago confirmado y entradas decrementadas para reserva {ReservationId}", reservationId);
                }
            }
        }

        public async Task<Reservation> GetReservationAsync(int id) 
        { 
            return await _reservationRepository.GetByIdAsync(id); 
        }

        public Task<Reservation> DeleteReservation(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Reservation>> GetAllReservationsAsync(string? status = null, int? eventId = null)
        {
            return await _reservationRepository.GetAllAsync(status, eventId);
        }

        Task<Reservation> IReservationService.UpdateReservationAsync(Reservation createdReservation)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Reservation>> GetReservationsByUserAndEventAsync(int userId, int eventId)
        {
            throw new NotImplementedException();
        }
    }
}