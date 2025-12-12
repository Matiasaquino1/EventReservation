using AutoMapper;
using EventReservations.Dto;
using EventReservations.Models;  // Para los modelos
using EventReservations.Services;  // Para IEventService
using EventReservations.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;  // Para listas
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; 
using System.ComponentModel.DataAnnotations; // Para validaciones (si agrego)

namespace EventReservations.Controllers
{
    /// <summary>
    /// Controlador para gestionar eventos, incluyendo creación, actualización, eliminación y consultas.
    /// Utiliza servicios para manejar lógica de negocio y AutoMapper para mapeos.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IMapper _mapper;
        private readonly IReservationService _reservationService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<EventsController> _logger;

        public EventsController(
            IEventService eventService,
            IMapper mapper,
            IReservationService reservationService,
            IPaymentService paymentService,
            ILogger<EventsController> logger)
        {
            _eventService = eventService;
            _mapper = mapper;
            _reservationService = reservationService;
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene una lista de eventos con filtros opcionales.
        /// </summary>
        /// <param name="date">Filtrar por fecha (opcional).</param>
        /// <param name="location">Filtrar por ubicación (opcional).</param>
        /// <param name="availability">Filtrar por disponibilidad de entradas (opcional).</param>
        /// <returns>Lista de eventos en formato DTO.</returns>
        /// <response code="200">Eventos obtenidos correctamente.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EventDto>), 200)]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents(
            [FromQuery] DateTime? date = null,
            [FromQuery] string? location = null,
            [FromQuery] int? availability = null)
        {
            var events = await _eventService.GetEventsWithFiltersAsync(date, location, availability);
            var eventDtos = _mapper.Map<IEnumerable<EventDto>>(events);

            _logger.LogInformation("Eventos obtenidos: {Count} registros", events.Count());
            return Ok(eventDtos);
        }

        /// <summary>
        /// Obtiene un evento por ID.
        /// </summary>
        /// <param name="id">ID del evento.</param>
        /// <returns>Detalles del evento en formato DTO.</returns>
        /// <response code="200">Evento encontrado.</response>
        /// <response code="404">Evento no encontrado.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EventDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<EventDto>> GetEvent([FromRoute] int id)
        {
            var eventModel = await _eventService.GetEventAsync(id);
            if (eventModel == null)
            {
                _logger.LogWarning("Evento no encontrado: {Id}", id);
                return NotFound(new { error = "Evento no encontrado." });
            }

            var eventDto = _mapper.Map<EventDto>(eventModel);
            return Ok(eventDto);
        }

        /// <summary>
        /// Crea un nuevo evento (requiere rol Organizer o Admin).
        /// </summary>
        /// <param name="createDto">Datos del evento a crear.</param>
        /// <returns>Evento creado en formato DTO.</returns>
        /// <response code="201">Evento creado exitosamente.</response>
        /// <response code="400">Datos inválidos.</response>
        /// <response code="401">No autorizado.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPost]
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventDto createDto)
        {
            var eventModel = _mapper.Map<Event>(createDto);

            eventModel.TicketsAvailable = createDto.TotalTickets;
            eventModel.TotalTickets = createDto.TotalTickets;

            var createdEvent = await _eventService.CreateEventAsync(eventModel);

            var eventDto = _mapper.Map<EventDto>(createdEvent);

            return CreatedAtAction(nameof(GetEvent), new { id = eventDto.EventId }, eventDto);
        }


        /// <summary>
        /// Actualiza un evento existente (requiere rol Organizer o Admin).
        /// </summary>
        /// <param name="id">ID del evento.</param>
        /// <param name="updateDto">Datos actualizados del evento.</param>
        /// <returns>Evento actualizado en formato DTO.</returns>
        /// <response code="200">Evento actualizado.</response>
        /// <response code="404">Evento no encontrado.</response>
        /// <response code="401">No autorizado.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Organizer,Admin")]
        [ProducesResponseType(typeof(EventDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<EventDto>> UpdateEvent([FromRoute] int id, [FromBody] UpdateEventDto updateDto)
        {
            updateDto.Id = id;
            var eventModel = _mapper.Map<Event>(updateDto);
            var updatedEvent = await _eventService.UpdateEventAsync(eventModel);

            if (updatedEvent == null)
            {
                _logger.LogWarning("Evento no encontrado para actualización: {Id}", id);
                return NotFound(new { error = "Evento no encontrado." });
            }

            var eventDto = _mapper.Map<EventDto>(updatedEvent);
            _logger.LogInformation("Evento actualizado: {Id}", id);
            return Ok(eventDto);
        }

        /// <summary>
        /// Elimina un evento (requiere rol Admin).
        /// </summary>
        /// <param name="id">ID del evento.</param>
        /// <response code="204">Evento eliminado.</response>
        /// <response code="401">No autorizado.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> DeleteEvent([FromRoute] int id)
        {
            await _eventService.DeleteEventAsync(id);
            _logger.LogInformation("Evento eliminado: {Id}", id);
            return NoContent();
        }

        /// <summary>
        /// Obtiene un resumen de estadísticas generales (requiere rol Admin).
        /// </summary>
        /// <returns>Objeto con totales de eventos, reservas, pagos y revenue.</returns>
        /// <response code="200">Resumen obtenido correctamente.</response>
        /// <response code="401">No autorizado.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet("summary")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetSummary()
        {
            var events = await _eventService.GetAllAsync();
            var reservations = await _reservationService.GetAllReservationsAsync(null, null);
            var payments = await _paymentService.GetAllPaymentsAsync();

            var summary = new
            {
                totalEvents = events.Count(),
                totalReservations = reservations.Count(),
                totalPayments = payments.Count(),
                totalRevenue = payments.Sum(p => p.Amount)
            };

            _logger.LogInformation("Resumen obtenido correctamente: Eventos={TotalEvents}, Reservas={TotalReservations}",
                summary.totalEvents, summary.totalReservations);

            return Ok(summary);
        }
    }
}