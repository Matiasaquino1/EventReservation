using AutoMapper;
using EventReservations.Dto;
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
        public async Task<ActionResult<PagedResponseDto<AdminReservationDto>>> GetAdminReservations(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string sort = "desc", [FromQuery] string status = null, [FromQuery] int? eventId = null)
        {
            var paged = await _reservationService.GetPagedReservationsAsync(page, pageSize, sort, status, eventId);
            var dtos = _mapper.Map<IEnumerable<AdminReservationDto>>(paged.Data);
            return Ok(new PagedResponseDto<AdminReservationDto> { Data = dtos, Page = paged.Page, PageSize = paged.PageSize, TotalCount = paged.TotalCount });
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