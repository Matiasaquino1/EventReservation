using AutoMapper;
using EventReservations.Dto;
using EventReservations.Models;
using EventReservations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventReservations.Profiles;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; 
using System.ComponentModel.DataAnnotations; // Para validaciones (si agrego)

namespace EventReservationApp.Controllers
{
    /// <summary>
    /// Controlador para operaciones administrativas, como gestión de reservas y eventos.
    /// Accesible solo para usuarios con rol "Admin".
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]  
    public class AdminController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly IEventService _eventService;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminController> _logger; 

        public AdminController(IReservationService reservationService, IEventService eventService, IMapper mapper, ILogger<AdminController> logger)
        {
            _reservationService = reservationService;
            _eventService = eventService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene una lista de reservas con filtros opcionales para administración.
        /// </summary>
        /// <param name="status">Filtro opcional por estado de la reserva (e.g., "Confirmed").</param>
        /// <param name="eventId">Filtro opcional por ID del evento.</param>
        /// <returns>Lista de reservas en formato DTO.</returns>
        /// <response code="200">Reservas obtenidas correctamente.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet("reservations")]
        [ProducesResponseType(typeof(IEnumerable<AdminReservationDto>), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<ActionResult<IEnumerable<AdminReservationDto>>> GetAdminReservations(
            [FromQuery] string? status = null,  
            [FromQuery] int? eventId = null)
        {
            try
            {
                var reservations = await _reservationService.GetAdminReservationsAsync(status, eventId);
                var adminDtos = _mapper.Map<IEnumerable<AdminReservationDto>>(reservations);
                _logger.LogInformation("Reservas obtenidas para admin: {Count} registros", reservations.Count());
                return Ok(adminDtos); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reservas para admin");
                return StatusCode(500, new { error = "Error interno del servidor." });
            }
        }

        /// <summary>
        /// Fuerza la confirmación de un evento específico.
        /// </summary>
        /// <param name="id">ID del evento a confirmar.</param>
        /// <returns>Evento confirmado.</returns>
        /// <response code="200">Evento confirmado correctamente.</response>
        /// <response code="404">Evento no encontrado.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPost("events/{id}/force-confirm")]
        [ProducesResponseType(typeof(EventDto), 200)] 
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> ForceConfirmEvent([FromRoute] int id)
        {
            try
            {
                var eventModel = await _eventService.ForceConfirmEventAsync(id);
                if (eventModel == null)
                {
                    _logger.LogWarning("Evento no encontrado para force-confirm: {Id}", id);
                    return NotFound(new { error = "Evento no encontrado." });
                }

                _logger.LogInformation("Evento forzado a confirmar: {Id}", id);
                return Ok(eventModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forzando confirmación de evento {Id}", id);
                return StatusCode(500, new { error = "Error interno del servidor." });
            }
        }
    }
}