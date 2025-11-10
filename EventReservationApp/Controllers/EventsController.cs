using AutoMapper;
using EventReservations.Dto;
using EventReservations.Models;  // Para los modelos
using EventReservations.Services;  // Para IEventService
using EventReservations.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;  // Para listas
using System.Threading.Tasks;

namespace EventReservationApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IMapper _mapper;
        private readonly IReservationService _reservationService;
        private readonly IPaymentService _paymentService;

        public EventsController(IEventService eventService, IMapper mapper, IReservationService reservationService, IPaymentService paymentService)
        {
            _eventService = eventService;
            _mapper = mapper;
            _reservationService = reservationService;
            _paymentService = paymentService;
        }
        /// <summary>
        /// Obtiene una lista de eventos con filtros opcionales.
        /// </summary>
        /// <param name="date">Filtrar por fecha (opcional).</param>
        /// <param name="location">Filtrar por ubicación (opcional).</param>
        /// <param name="availability">Filtrar por disponibilidad de entradas (opcional).</param>
        /// <returns>Lista de eventos.</returns>
        // GET /api/events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents([FromQuery] DateTime? date = null, [FromQuery] string location = null, [FromQuery] int? availability = null)
        {
            var events = await _eventService.GetEventsWithFiltersAsync(date, location, availability);
            var eventDtos = _mapper.Map<IEnumerable<EventDto>>(events);  // Mapea a DTOs
            return Ok(eventDtos);
        }

        /// <summary>
        /// Obtiene un evento por ID.
        /// </summary>
        /// <param name="id">ID del evento.</param>
        /// <returns>Detalles del evento.</returns>
        /// <response code="200">Evento encontrado.</response>
        /// <response code="404">Evento no encontrado.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<EventDto>> GetEvent(int id)
        {
            var eventModel = await _eventService.GetEventAsync(id);
            if (eventModel == null) return NotFound();
            var eventDto = _mapper.Map<EventDto>(eventModel);  // Mapea a DTO
            return Ok(eventDto);
        }

        /// <summary>
        /// Crea un nuevo evento (requiere rol Organizer o Admin).
        /// </summary>
        /// <param name="createDto">Datos del evento a crear.</param>
        /// <returns>Evento creado.</returns>
        /// <response code="201">Evento creado exitosamente.</response>
        /// <response code="401">No autorizado.</response>
        // POST /api/events (rol Organizer/Admin)
        [HttpPost]
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventDto createDto)
        {
            var eventModel = _mapper.Map<Event>(createDto);  // Mapea DTO a modelo
            var createdEvent = await _eventService.CreateEventAsync(eventModel);
            var eventDto = _mapper.Map<EventDto>(createdEvent);  // Mapea modelo a DTO
            return CreatedAtAction(nameof(GetEvent), new { id = eventDto.EventId }, eventDto);
        }


        public IEventService Get_eventService()
        {
            return _eventService;
        }

        /// <summary>
        /// Actualiza un evento existente (requiere rol Organizer o Admin).
        /// </summary>
        /// <param name="id">ID del evento.</param>
        /// <param name="updateDto">Datos actualizados del evento.</param>
        /// <returns>Evento actualizado.</returns>
        /// <response code="200">Evento actualizado.</response>
        /// <response code="404">Evento no encontrado.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<ActionResult<EventDto>> UpdateEvent(int id, [FromBody] UpdateEventDto updateDto)
        {
            updateDto.Id = id;  // Asegura el ID
            var eventModel = _mapper.Map<Event>(updateDto);  // Mapea DTO a modelo
            var updatedEvent = await _eventService.UpdateEventAsync(eventModel);
            if (updatedEvent == null) return NotFound();
            var eventDto = _mapper.Map<EventDto>(updatedEvent);  // Mapea a DTO
            return Ok(eventDto);
        }

        /// <summary>
        /// Elimina un evento (requiere rol Admin).
        /// </summary>
        /// <param name="id">ID del evento.</param>
        /// <response code="204">Evento eliminado.</response>
        /// <response code="401">No autorizado.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            await _eventService.DeleteEventAsync(id);  // Asume este método en IEventService
            return NoContent();
        }


        // GET: api/admin/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var events = await _eventService.GetAllAsync();
            var reservations = await _reservationService.GetAllReservationsAsync(null, null);
            var payments = await _paymentService.GetAllPaymentsAsync();

            var totalEvents = events.Count();
            var totalReservations = reservations.Count();
            var totalPayments = payments.Count();
            var totalRevenue = payments.Sum(p => p.Amount);

            return Ok(new
            {
                totalEvents,
                totalReservations,
                totalPayments,
                totalRevenue
            });
        }
    }
}
