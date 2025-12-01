using AutoMapper;
using EventReservations.Dto;  // Para ReservationDto
using EventReservations.Models;  // Para Reservation
using EventReservations.Services;  // Para IReservationService
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;  // Para Stripe integration
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; 
using System.Security.Claims; // Para claims de usuario
using System.ComponentModel.DataAnnotations; // Para validaciones (si agrego)

namespace EventReservations.Controllers
{
    /// <summary>
    /// Controlador para gestionar reservas de eventos, incluyendo creación, consulta y cancelación.
    /// Utiliza servicios para manejar lógica de negocio y AutoMapper para mapeos.
    /// </summary>
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
        /// <returns>Reserva creada en formato DTO.</returns>
        /// <response code="201">Reserva creada exitosamente.</response>
        /// <response code="400">Datos inválidos.</response>
        /// <response code="401">No autorizado (requiere rol User).</response>
        /// <response code="500">Error interno al crear la reserva.</response>
        [HttpPost]
        [Authorize(Roles = "User")]
        [ProducesResponseType(typeof(ReservationDto), 201)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<ActionResult<ReservationDto>> CreateReservation([FromBody] ReservationDto reservationDto)
        {
            try
            {
                if (reservationDto == null)
                    return BadRequest(new { error = "Datos de reserva inválidos." });

                var reservation = _mapper.Map<Reservation>(reservationDto);
                reservation.Status = "Pending";
                reservation.ReservationDate = DateTime.UtcNow;

                var created = await _reservationService.CreateReservationAsync(reservation);
                if (created == null)
                    return StatusCode(500, new { error = "No se pudo crear la reserva." });

                var createdDto = _mapper.Map<ReservationDto>(created);
                _logger.LogInformation("Reserva creada: {ReservationId} para usuario {UserId}", createdDto.ReservationId, createdDto.UserId);
                return CreatedAtAction(nameof(GetUserReservations), new { userId = createdDto.UserId }, createdDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando reserva");
                return StatusCode(500, new { error = "Error interno del servidor." });
            }
        }

        /// <summary>
        /// Obtiene todas las reservas con filtros opcionales (solo para Admin).
        /// </summary>
        /// <param name="status">Filtrar por estado de reserva (opcional).</param>
        /// <param name="eventId">Filtrar por ID de evento (opcional).</param>
        /// <returns>Lista de reservas en formato DTO.</returns>
        /// <response code="200">Reservas obtenidas.</response>
        /// <response code="401">No autorizado (requiere rol Admin).</response>
        /// <response code="500">Error interno.</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<ReservationDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetAllReservations([FromQuery] string? status, [FromQuery] int? eventId)
        {
            try
            {
                var reservations = await _reservationService.GetAllReservationsAsync(status, eventId);
                var dtos = _mapper.Map<IEnumerable<ReservationDto>>(reservations);
                _logger.LogInformation("Reservas obtenidas para admin: {Count} registros", reservations.Count());
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo todas las reservas");
                return StatusCode(500, new { error = "Error interno del servidor." });
            }
        }

        /// <summary>
        /// Crea una reserva y procesa el pago automáticamente con Stripe.
        /// </summary>
        /// <param name="createDto">Datos de la reserva incluyendo Amount y PaymentMethodId.</param>
        /// <returns>Reserva y resultado del pago mapeado a DTOs.</returns>
        /// <response code="200">Reserva y pago procesados.</response>
        /// <response code="400">Datos inválidos o error en pago.</response>
        /// <response code="401">No autorizado (requiere rol User).</response>
        /// <response code="500">Error interno.</response>
        [HttpPost("create-with-payment")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<ActionResult> CreateReservationWithPayment([FromBody] CreatePaymentIntentDto createDto)
        {
            try
            {
                if (createDto == null || createDto.Amount <= 0 || string.IsNullOrEmpty(createDto.PaymentMethodId))
                    return BadRequest(new { error = "Datos de reserva inválidos. Amount debe ser > 0 y PaymentMethodId requerido." });

                var reservation = _mapper.Map<Reservation>(createDto);
                reservation.Status = "Pending";
                reservation.ReservationDate = DateTime.UtcNow;

                var createdReservation = await _reservationService.CreateReservationAsync(reservation);
                if (createdReservation == null)
                    return StatusCode(500, new { error = "Error al crear la reserva." });

                var paymentResult = await _paymentService.ProcessPaymentAsync(
                    createdReservation.ReservationId,
                    createDto.Amount,
                    "usd",
                    createDto.PaymentMethodId
                );

                createdReservation.Status = paymentResult.Status == "succeeded" ? "Confirmed" : "PaymentFailed";
                await _reservationService.UpdateReservationAsync(createdReservation);

                var reservationResponse = _mapper.Map<ReservationDto>(createdReservation);
                var paymentResponse = _mapper.Map<PaymentRequestDto>(paymentResult);

                _logger.LogInformation("Reserva y pago procesados: Reserva {ReservationId}, Pago {PaymentId}", createdReservation.ReservationId, paymentResult.PaymentId);
                return Ok(new
                {
                    message = "Reserva y pago procesados correctamente.",
                    reservation = reservationResponse,
                    payment = paymentResponse
                });
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Error en pago con Stripe: {Message}", ex.StripeError.Message);
                return BadRequest(new { error = "Error en el procesamiento del pago." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando reserva con pago");
                return StatusCode(500, new { error = "Error interno del servidor." });
            }
        }

        /// <summary>
        /// Obtiene todas las reservas de un usuario específico (solo el propietario o Admin).
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Lista de reservas del usuario en formato DTO.</returns>
        /// <response code="200">Reservas obtenidas.</response>
        /// <response code="401">No autorizado.</response>
        /// <response code="403">Acceso denegado (no es el propietario).</response>
        /// <response code="500">Error interno.</response>
        [HttpGet("users/{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<ReservationDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetUserReservations([FromRoute] int userId)
        {
            try
            {
                // Verificación de seguridad: Solo el propietario o Admin puede acceder
                var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null || !int.TryParse(claim.Value, out var currentUserId))
                    return Unauthorized();

                if (currentUserId != userId && !User.IsInRole("Admin"))
                    return Forbid(); // 403 Forbidden

                var reservations = await _reservationService.GetReservationsByUserAsync(userId);
                var dtos = _mapper.Map<IEnumerable<ReservationDto>>(reservations);
                _logger.LogInformation("Reservas obtenidas para usuario {UserId}: {Count} registros", userId, reservations.Count());
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reservas para usuario {UserId}", userId);
                return StatusCode(500, new { error = "Error interno del servidor." });
            }
        }

        /// <summary>
        /// Obtiene una reserva por ID.
        /// </summary>
        /// <param name="id">ID de la reserva.</param>
        /// <returns>Detalles de la reserva en formato DTO.</returns>
        /// <response code="200">Reserva encontrada.</response>
        /// <response code="404">Reserva no encontrada.</response>
        /// <response code="401">No autorizado.</response>
        /// <response code="500">Error interno.</response>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ReservationDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<ActionResult<ReservationDto>> GetReservation([FromRoute] int id)
        {
            try
            {
                var reservation = await _reservationService.GetReservationAsync(id);
                if (reservation == null)
                {
                    _logger.LogWarning("Reserva no encontrada: {Id}", id);
                    return NotFound(new { error = "Reserva no encontrada." });
                }

                var dto = _mapper.Map<ReservationDto>(reservation);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reserva {Id}", id);
                return StatusCode(500, new { error = "Error interno del servidor." });
            }
        }

        /// <summary>
        /// Cancela una reserva existente.
        /// </summary>
        /// <param name="id">ID de la reserva a cancelar.</param>
        /// <returns>Reserva cancelada en formato DTO.</returns>
        /// <response code="200">Reserva cancelada.</response>
        /// <response code="404">Reserva no encontrada.</response>
        /// <response code="401">No autorizado.</response>
        /// <response code="500">Error interno.</response>
        [HttpPut("{id}/cancel")]
        [Authorize]
        [ProducesResponseType(typeof(ReservationDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> CancelReservation([FromRoute] int id)
        {
            try
            {
                var updated = await _reservationService.CancelReservationAsync(id);
                if (updated == null)
                {
                    _logger.LogWarning("Reserva no encontrada para cancelar: {Id}", id);
                    return NotFound(new { error = "Reserva no encontrada." });
                }

                var dto = _mapper.Map<ReservationDto>(updated);
                _logger.LogInformation("Reserva cancelada: {Id}", id);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelando reserva {Id}", id);
                return StatusCode(500, new { error = "Error interno del servidor." });
            }
        }
    }
}f