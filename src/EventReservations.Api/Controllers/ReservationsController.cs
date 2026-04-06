using AutoMapper;
using EventReservations.Dto;
using EventReservations.Models;
using EventReservations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        /// <response code="401">No autorizado (requiere rol User o Admin).</response>
        /// <response code="500">Error interno al crear la reserva.</response>
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(typeof(ReservationDto), 201)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<ActionResult<ReservationDto>> CreateReservation([FromBody] CreatedReservationDto reservationDto)
        {
            try
            {
                if (reservationDto == null)
                    return BadRequest(new { error = "Datos de reserva inválidos." });

                var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null || !int.TryParse(claim.Value, out var userId))
                    return Unauthorized();

                var reservation = _mapper.Map<Reservation>(reservationDto);
                reservation.UserId = userId;
                reservation.Status = ReservationStatuses.Pending;
                reservation.ReservationDate = DateTime.UtcNow;

                var created = await _reservationService.CreateReservationAsync(reservation);
                if (created == null)
                    return StatusCode(500, new { error = "No se pudo crear la reserva." });

                var createdDto = _mapper.Map<ReservationDto>(created);
                _logger.LogInformation("Reserva creada: {ReservationId} para usuario {UserId}", createdDto.ReservationId, createdDto.UserId);
                return CreatedAtAction(nameof(GetUserReservations), new { userId = createdDto.UserId }, createdDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Regla de negocio inválida al crear reserva");
                return BadRequest(new { error = ex.Message });
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


        [HttpGet("my")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<ReservationDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetMyReservations()
        {
            try
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null || !int.TryParse(claim.Value, out var userId))
                    return Unauthorized();

                var reservations = await _reservationService.GetReservationsByUserAsync(userId);
                var dtos = _mapper.Map<IEnumerable<ReservationDto>>(reservations);

                _logger.LogInformation("Reservas obtenidas para usuario {UserId}: {Count}", userId, reservations.Count());
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reservas del usuario autenticado");
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

                var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null || !int.TryParse(claim.Value, out var userId))
                    return Unauthorized();

                if (reservation.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid();

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
        [HttpPatch("{id}/cancel")]
        [Authorize]
        [ProducesResponseType(typeof(ReservationDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> CancelReservation([FromRoute] int id)
        {
            try
            {
                var reservation = await _reservationService.GetReservationAsync(id);
                if (reservation == null)
                {
                    _logger.LogWarning("Reserva no encontrada para cancelar: {Id}", id);
                    return NotFound(new { error = "Reserva no encontrada." });
                }

                var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null || !int.TryParse(claim.Value, out var userId))
                    return Unauthorized();

                if (reservation.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                var updated = await _reservationService.CancelReservationAsync(id);
                if (updated == null)
                {
                    _logger.LogWarning("Reserva no encontrada para cancelar: {Id}", id);
                    return NotFound(new { error = "Reserva no encontrada." });
                }

                if (reservation.Status == ReservationStatuses.Cancelled)
                    return BadRequest("La reserva ya se encuentra cancelada.");

                var dto = _mapper.Map<ReservationDto>(updated);
                _logger.LogInformation("Reserva cancelada: {Id}", id);
                return Ok(dto);            
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _logger.LogError(ex, "Error cancelando reserva {Id}", id);
                return StatusCode(500, new { error = "Error interno del servidor." });
            }
        }

        [HttpPatch("{id}/confirm")]
        [Authorize]
        public async Task<IActionResult> ConfirmReservation(int id, [FromBody] ConfirmPaymentDto dto)
        {
            try
            {
                await _reservationService.ConfirmPaymentAndDecrementTicketsAsync(id, dto.PaymentIntentId);
                return Ok(new { message = "Reserva confirmada y stock descontado." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "El cupo fue tomado por otro usuario. Intentá de nuevo." });
            }
        }

        [HttpPatch("{id}/force-confirm")]
        public async Task<IActionResult> ForceConfirm(int id)
        {
            try
            {
                var result = await _reservationService.ConfirmReservationAsync(id);

                if (!result)
                {
                    return NotFound(new { message = $"No se encontró la reserva #{id}" });
                }

                Console.WriteLine("Llegué al final del endpoint");
                return Ok(new { message = "Reserva confirmada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar la reserva {Id}", id);
                return StatusCode(500, "Error interno al procesar la confirmación");
            }
        }


        [HttpGet("{id}/payment-intent")]
        [Authorize]
        public async Task<IActionResult> GetPaymentIntent(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var clientSecret = await _paymentService
                .GetExistingOrNewClientSecretAsync(id, userId);

            return Ok(new { clientSecret });
        }

        [HttpPatch("{id}/hide")]
        [Authorize]
        public async Task<IActionResult> HideReservation(int id)
        {
            try
            {
                await _reservationService.HideReservationAsync(id);
                return Ok(new { message = "Reserva ocultada." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
