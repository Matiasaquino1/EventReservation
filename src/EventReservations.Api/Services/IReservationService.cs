using EventReservations.Data;
using EventReservations.Dto;
using EventReservations.Models;
using EventReservations.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
        /// <summary>
        /// Obtiene una lista paginada de reservas con filtros opcionales para uso administrativo.
        /// Permite filtrar por estado y evento, ordenar por fecha, y paginar resultados.
        /// </summary>
        /// <param name="page">El número de página a recuperar (empezando en 1). Si es menor a 1, se asume 1.</param>
        /// <param name="pageSize">El número de elementos por página (máximo recomendado: 100 para performance).</param>
        /// <param name="sort">Orden de los resultados: "asc" para ascendente o "desc" para descendente (por defecto "desc" por fecha de reserva).</param>
        /// <param name="status">Filtro opcional por estado de la reserva (e.g., "Pending", "Confirmed"). Si es null o vacío, no filtra.</param>
        /// <param name="eventId">Filtro opcional por ID de evento. Si es null, no filtra.</param>
        /// <returns>Un objeto PagedResponseDto con la lista de reservas, página actual, tamaño de página y total de elementos.</returns>
        Task<PagedResponseDto<Reservation>> GetPagedReservationsAsync(int page, int pageSize, string sort, string status, int? eventId);
    }

    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IReservationRepository _reservationRepository;
        private readonly IEventRepository _eventRepository;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            ApplicationDbContext context,
            IReservationRepository reservationRepository,
            IEventRepository eventRepository,
            ILogger<ReservationService> logger)
        {
            _context = context;
            _reservationRepository = reservationRepository;
            _eventRepository = eventRepository;
            _logger = logger;
        }

        public async Task<Reservation> CancelReservationAsync(int id)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);

            if (reservation != null && reservation.Status != ReservationStatuses.Cancelled)
            {
                reservation.Status = ReservationStatuses.Cancelled;
                await _reservationRepository.UpdateAsync(reservation);

                var eventModel = await _eventRepository.GetByIdAsync(reservation.EventId);
                if (eventModel != null)
                {
                    eventModel.TicketsAvailable += reservation.NumberOfTickets;

                    if (eventModel.TicketsAvailable > 0 && eventModel.Status == "SoldOut")
                        eventModel.Status = "Active";

                    await _eventRepository.UpdateAsync(eventModel);
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
                throw new InvalidOperationException("Ya tienes una reserva para este evento.");

            var eventModel = await _eventRepository.GetByIdAsync(reservation.EventId);

            if (eventModel == null)
                throw new InvalidOperationException("Evento no encontrado.");

            if (checkAvailability)
            {
                if (eventModel.TicketsAvailable < reservation.NumberOfTickets)
                    throw new InvalidOperationException("No hay suficientes entradas disponibles.");
            }

            // Crear reserva
            var created = await _reservationRepository.AddAsync(reservation);
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
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var reservation = await _reservationRepository.GetByIdAsync(reservationId);
                if (reservation == null || reservation.Status != "Pending")
                    throw new InvalidOperationException("Reserva inválida.");

                var eventModel = await _eventRepository.GetByIdAsync(reservation.EventId);
                if (eventModel == null)
                    throw new InvalidOperationException("Evento no encontrado.");

                if (eventModel.TicketsAvailable < reservation.NumberOfTickets)
                    throw new InvalidOperationException("Entradas insuficientes.");

                // 1️⃣ Descontar tickets
                eventModel.TicketsAvailable -= reservation.NumberOfTickets;
                if (eventModel.TicketsAvailable == 0)
                    eventModel.Status = "SoldOut";

                // 2️⃣ Confirmar reserva
                reservation.Status = "Confirmed";

                // 3️⃣ Guardar todo junto
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Pago confirmado y tickets descontados. Reserva {ReservationId}",
                    reservationId
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
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

        // Reemplaza la implementación de GetPagedReservationsAsync con una llamada al repository
        public async Task<PagedResponseDto<Reservation>> GetPagedReservationsAsync(int page, int pageSize, string sort, string status, int? eventId)
        {
            return await _reservationRepository.GetPagedReservationsAsync(page, pageSize, sort, status, eventId);
        }

    }
}