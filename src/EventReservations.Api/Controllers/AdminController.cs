using AutoMapper;
using EventReservations.Data;
using EventReservations.Dto;
using EventReservations.Models;
using EventReservations.Profiles;
using EventReservations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventReservations.Controllers
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
        private readonly IUserService _userService;
        private readonly IReservationService _reservationService;
        private readonly IEventService _eventService;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminController> _logger;
        private readonly ApplicationDbContext _context;


        public AdminController(IReservationService reservationService, IEventService eventService, IMapper mapper, ILogger<AdminController> logger, ApplicationDbContext dbContext, IUserService userService)
        {
            _reservationService = reservationService;
            _eventService = eventService;
            _mapper = mapper;
            _logger = logger;
            _context = dbContext;
            _userService = userService;
        }

        /// <summary>
        /// Obtiene una lista de reservas con filtros opcionales para administración.
        /// </summary>
        [HttpGet("ventas")]
        public async Task<ActionResult<PagedResponseDto<AdminReservationDto>>> GetAdminReservations(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string sort = "desc", [FromQuery] string status = null, [FromQuery] int? eventId = null)
        {
            var paged = await _reservationService.GetPagedReservationsAsync(page, pageSize, sort, status, eventId);
            var dtos = _mapper.Map<IEnumerable<AdminReservationDto>>(paged.Data);
            return Ok(new PagedResponseDto<AdminReservationDto> { Data = dtos, Page = paged.Page, PageSize = paged.PageSize, TotalCount = paged.TotalCount });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var (users, totalCount) = await _userService.GetUsersPagedAsync(page, limit);
            var userDtos = _mapper.Map<List<UserAdminDto>>(users);

            return Ok(new
            {
                Users = userDtos,
                Total = totalCount
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

        /// <summary>
        /// Admins pueden promover a otros usuarios
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("promote/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PromoteUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "Usuario no encontrado." });

            if (user.Role == "Admin")
                return BadRequest(new { message = "El usuario ya es Admin." });

            user.Role = "Admin";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario promovido a Admin correctamente." });
        }

        // PUT api/admin/events/{id}/stock
        [HttpPut("events/{id}/stock")]
        [ProducesResponseType(typeof(EventDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AdjustEventStock([FromRoute] int id, [FromBody] AdjustStockDto dto)
        {
            if (dto == null) return BadRequest(new { error = "Request body required." });
            if (dto.NewTotalTickets < 0) return BadRequest(new { error = "Total tickets must be >= 0." });

            var ev = await _eventService.GetEventByIdAsync(id);
            if (ev == null) return NotFound(new { error = "Event not found." });

            // Ajuste: si se reduce total, TicketsAvailable ajusta restando la diferencia
            var oldTotal = ev.TotalTickets;
            var newTotal = dto.NewTotalTickets;
            var diff = newTotal - oldTotal;

            ev.TotalTickets = newTotal;
            ev.TicketsAvailable = Math.Max(0, ev.TicketsAvailable + diff);

            // Si tickets agotadas
            if (ev.TicketsAvailable == 0) ev.Status = "SoldOut";
            else if (ev.Status == "SoldOut") ev.Status = "Active";

            var updated = await _eventService.UpdateEventAsync(ev);
            var dtoOut = _mapper.Map<EventDto>(updated);
            return Ok(dtoOut);
        }

        [HttpGet("dashboard-stats")]
        public async Task<ActionResult<AdminDashboardDto>> GetDashboardStats()
        {
            var stats = new AdminDashboardDto
            {
                TotalUsers = await _context.Users.CountAsync(),

                TotalReservations = await _context.Reservations
                    .Where(r => r.Status != ReservationStatuses.Cancelled)
                    .CountAsync(),

                TotalRevenue = await _context.Reservations
                    .Where(r => r.Status == ReservationStatuses.Confirmed)
                    .SumAsync(r => r.Amount),

                TopEvents = await _context.Reservations
                    .GroupBy(r => r.Event.Title)
                    .Select(g => new EventStatDto
                    {
                        EventName = g.Key,
                        TicketCount = g.Sum(r => r.NumberOfTickets)
                    })
                    .OrderByDescending(x => x.TicketCount)
                    .Take(5)
                    .ToListAsync()
            };

            return Ok(stats);
        }
    }
}