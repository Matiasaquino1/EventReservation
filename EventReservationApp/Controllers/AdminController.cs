using AutoMapper;
using EventReservations.Dto;
using EventReservations.Models;
using EventReservations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventReservations.Profiles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventReservationApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]  // Solo para admins
    public class AdminController : ControllerBase
    {
        private readonly IReservationService _reservationService; 
        private readonly IEventService _eventService;
        private readonly IMapper _mapper;

        public AdminController(IReservationService reservationService, IEventService eventService, IMapper mapper)
        {
            _reservationService = reservationService;
            _eventService = eventService;
            _mapper = mapper;
        }

        // GET /api/admin/reservations (con filtros)
        [HttpGet("reservations")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetAdminReservations(
            [FromQuery] string? status = null,  // Ejemplo de filtro
            [FromQuery] int? eventId = null)
        {
            var reservations = await _reservationService.GetAdminReservationsAsync(status, eventId);
            var AdminDtos = _mapper.Map<IEnumerable<AdminReservationDto>>(reservations);// Asume este método
            return Ok(reservations);
        }

        // POST /api/admin/events/{id}/force-confirm
        [HttpPost("events/{id}/force-confirm")]
        public async Task<IActionResult> ForceConfirmEvent(int id)
        {
            var eventModel = await _eventService.ForceConfirmEventAsync(id); // Asume este método para lógica personalizada
            eventModel.CreatedAt = DateTime.UtcNow;
            if (eventModel == null) return NotFound();
            return Ok(eventModel);
        }

    }
}
