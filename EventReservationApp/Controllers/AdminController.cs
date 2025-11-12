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
        [HttpGet("reservations")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetAdminReservations(
            [FromQuery] string? status = null,
            [FromQuery] int? eventId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sort = "desc")
        {
            var result = await _reservationService.GetAdminReservationsAsync(status, eventId, page, pageSize, sort);
            var adminDtos = _mapper.Map<IEnumerable<AdminReservationDto>>(result.Data);

            return Ok(new
            {
                page,
                pageSize,
                totalRecords = result.TotalRecords,
                totalPages = (int)Math.Ceiling(result.TotalRecords / (double)pageSize),
                data = adminDtos
            });
        }


        /// <summary>
        /// Fuerza la confirmación de un evento específico.
        /// </summary>
        [HttpPost("events/{id}/force-confirm")]
        [ProducesResponseType(typeof(EventDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> ForceConfirmEvent([FromRoute] int id)
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
    }
}