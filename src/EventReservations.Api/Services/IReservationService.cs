using EventReservations.Data;
using EventReservations.Dto;
using EventReservations.Models;
using EventReservations.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Threading.Tasks;

namespace EventReservations.Services
{
    public interface IReservationService
    {
        Task<Reservation> CreateReservationAsync(Reservation reservation, bool checkAvailability = true);
        Task<Reservation> CancelReservationAsync(int id);
        Task HideReservationAsync(int id);
        Task<IEnumerable<Reservation>> GetReservationsByUserAsync(int userId);
        Task<IEnumerable<Reservation>> GetReservationsByEventIdAsync(int eventId);
        Task<(IEnumerable<Reservation> Data, int TotalRecords)> GetAdminReservationsAsync(string? status, int? eventId, int page,  int pageSize, string sort);
        Task<Reservation> UpdateReservationAsync(Reservation reservation);
        Task<bool> IsDuplicateReservationAsync(int userId, int eventId);
        Task<bool> ConfirmReservationAsync(int reservationId);
        Task<Reservation> DeleteReservation(int id);
        Task<Reservation> GetReservationAsync(int id);
        Task<IEnumerable<Reservation>> GetAllReservationsAsync(string? status = null, int? eventId = null);
        Task<IEnumerable<Reservation>> GetReservationsByUserAndEventAsync(int userId, int eventId);
        Task ConfirmPaymentAndDecrementTicketsAsync(int reservationId, string StripePaymentIntentId);
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

        public async Task<Reservation> CreateReservationAsync(
            Reservation reservation,
            bool checkAvailability = true)
        {
            if (reservation.NumberOfTickets <= 0)
                throw new InvalidOperationException("La cantidad de entradas debe ser mayor a cero.");

            if (await IsDuplicateReservationAsync(reservation.UserId, reservation.EventId))
                throw new InvalidOperationException("Ya tienes una reserva para este evento.");

            var eventModel = await _eventRepository.GetByIdAsync(reservation.EventId)
                ?? throw new InvalidOperationException("Evento no encontrado.");

            if (checkAvailability && eventModel.TicketsAvailable < reservation.NumberOfTickets)
                throw new InvalidOperationException("No hay suficientes entradas disponibles.");
            
            if (eventModel.Status == "Cancelled")
                throw new InvalidOperationException("No se pueden crear reservas para eventos cancelados.");

            // El monto se calcula en backend para evitar manipulación del cliente.
            reservation.Amount = eventModel.Price * reservation.NumberOfTickets;


            reservation.Status = ReservationStatuses.Pending;
            reservation.CreatedAt = DateTime.UtcNow;

            return await _reservationRepository.AddAsync(reservation);
        }


        public async Task<Reservation> CancelReservationAsync(int id)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);

            if (reservation.Status != ReservationStatuses.Pending && reservation.Status != ReservationStatuses.Confirmed)
            {
                throw new InvalidOperationException("Solo reservas pendientes o confirmadas pueden cancelarse.");
            }

            if (!string.IsNullOrEmpty(reservation.PaymentIntentId))
            {
                var service = new PaymentIntentService();

                try
                {
                    await service.CancelAsync(reservation.PaymentIntentId);
                }
                catch (StripeException)
                {
                }
            }

            reservation.Status = ReservationStatuses.Cancelled;

            await _reservationRepository.UpdateAsync(reservation);

            return reservation;
        }

        public async Task HideReservationAsync(int id)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id)
                ?? throw new InvalidOperationException("Reserva no encontrada.");

            if (reservation.Status != ReservationStatuses.Cancelled)
                throw new InvalidOperationException("Solo se pueden ocultar reservas canceladas.");

            reservation.IsVisibleForUser = false;
            await _reservationRepository.UpdateAsync(reservation);
        }

        public async Task<IEnumerable<Reservation>> GetReservationsByUserAsync(int userId)
        {
            return await _reservationRepository.GetReservationsByUserAsync(userId);
        }

        public async Task<IEnumerable<Reservation>> GetReservationsByEventIdAsync(int eventId)
        {
            try
            {
                var reservations = await _reservationRepository.GetByEventIdWithUserAsync(eventId);
                _logger.LogInformation("Se obtuvieron {Count} reservas para el evento {EventId}", reservations.Count(), eventId);
                return reservations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservas por evento {EventId}", eventId);
                throw;
            }
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

        public async Task<bool> IsDuplicateReservationAsync(int userId, int eventId)
        {
            var existing = await _reservationRepository.GetReservationsByUserAndEventAsync(userId, eventId);
            return existing.Any(r =>
                r.Status == ReservationStatuses.Pending ||
                r.Status == ReservationStatuses.Confirmed);
        }

        public async Task<Reservation> UpdateReservationAsync(Reservation reservation)
        {
            var updated = await _reservationRepository.UpdateAsync(reservation);
            _logger.LogInformation("Reserva {ReservationId} actualizada a status {Status}", reservation.ReservationId, reservation.Status);
            return updated;
        }

        public async Task ConfirmPaymentAndDecrementTicketsAsync(int reservationId, string stripePaymentIntentId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var reservation = await _context.Reservations
                    .Include(r => r.Event) 
                    .FirstOrDefaultAsync(r => r.ReservationId == reservationId)
                    ?? throw new InvalidOperationException("Reserva no encontrada.");

                // Idempotencia
                if (reservation.Status == ReservationStatuses.Confirmed)
                    return;

                if (reservation.Status != ReservationStatuses.Pending)
                    throw new InvalidOperationException("La reserva no se puede confirmar: estado inválido.");

                if (reservation.Event == null)
                    throw new InvalidOperationException("Evento no encontrado.");

                if (reservation.Event.TicketsAvailable < reservation.NumberOfTickets)
                    throw new InvalidOperationException("Entradas insuficientes.");

                var payment = await _context.Payments.FirstOrDefaultAsync(p =>
                    p.ReservationId == reservationId &&
                    p.StripePaymentIntentId == stripePaymentIntentId);
                
                if (payment == null)
                {
                    payment = new Payment
                    {
                        ReservationId = reservationId,
                        StripePaymentIntentId = stripePaymentIntentId, 
                        Status = PaymentStatuses.Pending,
                        Amount = reservation.Amount,
                        PaymentDate = DateTime.UtcNow
                    };
                    _context.Payments.Add(payment);
                }

                if (payment.Status == PaymentStatuses.Succeeded)
                    return;

                payment.Status = PaymentStatuses.Succeeded;
                payment.PaymentDate = DateTime.UtcNow;

                reservation.Event.TicketsAvailable -= reservation.NumberOfTickets;
                if (reservation.Event.TicketsAvailable == 0)
                    reservation.Event.Status = "SoldOut";

                reservation.Status = ReservationStatuses.Confirmed;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error crítico en ConfirmPaymentAndDecrementTicketsAsync para la reserva {Id}", reservationId);
                throw; // Relanzo para que el controlador reciba la excepción
            }
        }

        public async Task<bool> ConfirmReservationAsync(int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);

            if (reservation == null) return false;

            // Solo confirma si está pendiente
            if (reservation.Status == ReservationStatuses.Pending)
            {
                reservation.Status = ReservationStatuses.Confirmed;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<Reservation> GetReservationAsync(int id) 
        { 
            return await _reservationRepository.GetByIdAsync(id); 
        }

        public async Task<Reservation> DeleteReservation(int id)
        {
            var existing = await _reservationRepository.GetByIdAsync(id)
                ?? throw new InvalidOperationException("Reserva no encontrada.");

            await _reservationRepository.DeleteAsync(id);
            return existing;
        }

        public async Task<IEnumerable<Reservation>> GetAllReservationsAsync(string? status = null, int? eventId = null)
        {
            return await _reservationRepository.GetAllAsync(status, eventId);
        }

        public async Task<IEnumerable<Reservation>> GetReservationsByUserAndEventAsync(int userId, int eventId)
        {
            return await _reservationRepository.GetReservationsByUserAndEventAsync(userId, eventId);
        }

        // Reemplaza la implementación de GetPagedReservationsAsync con una llamada al repository
        public async Task<PagedResponseDto<Reservation>> GetPagedReservationsAsync(int page, int pageSize, string sort, string status, int? eventId)
        {
            return await _reservationRepository.GetPagedReservationsAsync(page, pageSize, sort, status, eventId);
        }

    }
}
