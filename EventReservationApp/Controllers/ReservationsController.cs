using AutoMapper;
using EventReservations.Dto;  // Para ReservationDto
using EventReservations.Models;  // Para Reservation
using EventReservations.Services;  // Para IReservationService
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;  // Para Stripe integration
using System.Threading.Tasks;

namespace EventReservationApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly IPaymentService _paymentService;
        private readonly IMapper _mapper;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(IReservationService reservationService,
                                      IPaymentService paymentService,
                                      IMapper mapper,
                                      ILogger<ReservationsController> logger)
        {
            _reservationService = reservationService;
            _paymentService = paymentService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Crea una nueva reserva para un usuario autenticado.
        /// </summary>
        /// <param name="reservationDto">Datos de la reserva (incluye UserId, EventId, etc.).</param>
        /// <returns>Reserva creada.</returns>
        /// <response code="201">Reserva creada exitosamente.</response>
        /// <response code="400">Datos inválidos.</response>
        /// <response code="401">No autorizado (requiere rol User).</response>
        /// <response code="500">Error interno al crear la reserva.</response>
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<ReservationDto>> CreateReservation([FromBody] ReservationDto reservationDto)
        {
            if (reservationDto == null)
                return BadRequest("Datos de reserva inválidos.");

            var reservation = _mapper.Map<Reservation>(reservationDto);
            reservation.Status = "Pending";
            reservation.ReservationDate = DateTime.UtcNow;

            var created = await _reservationService.CreateReservationAsync(reservation);
            if (created == null)
                return StatusCode(500, "No se pudo crear la reserva.");

            var createdDto = _mapper.Map<ReservationDto>(created);

            return CreatedAtAction(nameof(GetUserReservations), new { userId = createdDto.UserId }, createdDto);
        }

        /// <summary>
        /// Obtiene todas las reservas con filtros opcionales (solo para Admin).
        /// </summary>
        /// <param name="status">Filtrar por estado de reserva (opcional).</param>
        /// <param name="eventId">Filtrar por ID de evento (opcional).</param>
        /// <returns>Lista de reservas.</returns>
        /// <response code="200">Reservas obtenidas.</response>
        /// <response code="401">No autorizado (requiere rol Admin).</response>
        /// <response code="500">Error interno.</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReservations([FromQuery] string? status, [FromQuery] int? eventId)
        {
            try
            {
                var reservations = await _reservationService.GetAllReservationsAsync(status, eventId);
                var dtos = _mapper.Map<IEnumerable<ReservationDto>>(reservations); 
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetAllReservations");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Crea una reserva y procesa el pago automáticamente con Stripe.
        /// </summary>
        /// <param name="reservationDto">Datos de la reserva (nota: amount y paymentMethodId podrían agregarse al DTO para flexibilidad).</param>
        /// <returns>Reserva y resultado del pago.</returns>
        /// <response code="200">Reserva y pago procesados.</response>
        /// <response code="400">Datos inválidos o error en pago.</response>
        /// <response code="401">No autorizado (requiere rol User).</response>
        /// <response code="500">Error interno.</response>
        [HttpPost("create-with-payment")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult> CreateReservationWithPayment([FromBody] ReservationDto reservationDto)
        {
            if (reservationDto == null)
                return BadRequest("Datos de reserva inválidos.");

            var reservation = _mapper.Map<Reservation>(reservationDto);
            reservation.Status = "Pending";
            reservation.ReservationDate = DateTime.UtcNow;

            var createdReservation = await _reservationService.CreateReservationAsync(reservation);
            if (createdReservation == null)
                return StatusCode(500, "Error al crear la reserva.");

            try
            {
                // Sugerencia: Recibir amount y paymentMethodId del DTO en lugar de hardcodear
                var payment = await _paymentService.ProcessPaymentAsync(
                    createdReservation.ReservationId,
                    amount: 50.00m,  // Cambiar a reservationDto.Amount si lo agregas
                    currency: "usd",
                    paymentMethodId: "pm_card_visa"  // Cambiar a reservationDto.PaymentMethodId si lo agregas
                );

                createdReservation.Status = payment.Status == "succeeded" ? "Confirmed" : "PaymentFailed";
                await _reservationService.UpdateReservationAsync(createdReservation);

                var reservationResponse = _mapper.Map<ReservationDto>(createdReservation);

                return Ok(new
                {
                    message = "Reserva y pago procesados correctamente.",
                    reservation = reservationResponse,
                    payment
                });
            }
            catch (StripeException ex)
            {
                return StatusCode(400, new { error = ex.StripeError.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error procesando el pago: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtiene todas las reservas de un usuario específico.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Lista de reservas del usuario.</returns>
        /// <response code="200">Reservas obtenidas.</response>
        /// <response code="401">No autorizado.</response>
        [HttpGet("users/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetUserReservations(int userId)
        {
            var reservations = await _reservationService.GetReservationsByUserAsync(userId);
            var dtos = _mapper.Map<IEnumerable<ReservationDto>>(reservations);
            return Ok(dtos);
        }

        /// <summary>
        /// Obtiene una reserva por ID.
        /// </summary>
        /// <param name="id">ID de la reserva.</param>
        /// <returns>Detalles de la reserva.</returns>
        /// <response code="200">Reserva encontrada.</response>
        /// <response code="404">Reserva no encontrada.</response>
        /// <response code="401">No autorizado.</response>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ReservationDto>> GetReservation(int id)
        {
            var reservation = await _reservationService.GetReservationAsync(id);
            if (reservation == null) return NotFound();

            var dto = _mapper.Map<ReservationDto>(reservation);
            return Ok(dto);
        }

        /// <summary>
        /// Cancela una reserva existente.
        /// </summary>
        /// <param name="id">ID de la reserva a cancelar.</param>
        /// <returns>Reserva cancelada.</returns>
        /// <response code="200">Reserva cancelada.</response>
        /// <response code="404">Reserva no encontrada.</response>
        /// <response code="401">No autorizado.</response>
        [HttpPut("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var updated = await _reservationService.CancelReservationAsync(id);
            if (updated == null) return NotFound();

            var dto = _mapper.Map<ReservationDto>(updated);
            return Ok(dto);
        }
    }
}