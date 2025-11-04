using AutoMapper;
using EventReservations.Dto;
using EventReservations.Models;  // Para los modelos
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

        // POST /api/reservations
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

        // GET: api/reservations  (solo admin)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReservations([FromQuery] string? status, [FromQuery] int? eventId)
        {
            try
            {
                var reservations = await _reservationService.GetAllReservationsAsync(status, eventId);
                var dtos = reservations.Select(r => _mapper.Map<ReservationDto>(r));
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetAllReservations");
                return StatusCode(500, new { error = ex.Message });
            }
        }


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
                var payment = await _paymentService.ProcessPaymentAsync(
                    createdReservation.ReservationId,
                    amount: 50.00m,
                    currency: "usd",
                    paymentMethodId: "pm_card_visa"
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



        // GET /api/users/{userId}/reservations
        [HttpGet("users/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetUserReservations(int userId)
        {
            var reservations = await _reservationService.GetReservationsByUserAsync(userId);
            var dtos = _mapper.Map<IEnumerable<ReservationDto>>(reservations);
            return Ok(dtos);
        }

        //GET /api/reservations/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ReservationDto>> GetReservation(int id)
        {
            var reservation = await _reservationService.GetReservationAsync(id);
            if (reservation == null) return NotFound();

            var dto = _mapper.Map<ReservationDto>(reservation);
            return Ok(dto);
        }

        // PUT /api/reservations/{id}/cancel
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
