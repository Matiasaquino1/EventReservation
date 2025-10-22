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
        public async Task<ActionResult<Reservation>> CreateReservation([FromBody] Reservation reservation)
        {
            if (reservation == null)
                return BadRequest("Datos de reserva inválidos.");

            reservation.Status = "Pending";
            reservation.ReservationDate = DateTime.UtcNow;

            var createdReservation = await _reservationService.CreateReservationAsync(reservation);
            if (createdReservation == null)
                return StatusCode(500, "No se pudo crear la reserva.");

            return CreatedAtAction(nameof(GetUserReservations), new { userId = reservation.UserId }, createdReservation);
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
        public async Task<ActionResult> CreateReservationWithPayment([FromBody] Reservation reservation)
        {
            if (reservation == null)
                return BadRequest("Datos de reserva inválidos.");

            reservation.Status = "Pending";
            reservation.ReservationDate = DateTime.UtcNow;

            // 1️⃣ Crear la reserva en tu base de datos
            var createdReservation = await _reservationService.CreateReservationAsync(reservation);
            if (createdReservation == null)
                return StatusCode(500, "Error al crear la reserva.");

            try
            {
                // 2️⃣ Generar el pago con Stripe (PaymentIntent)
                var payment = await _paymentService.ProcessPaymentAsync(
                    createdReservation.ReservationId,
                    amount: 50.00m, // <-- reemplazá por el monto real de tu evento si lo tenés en BD
                    currency: "usd",
                    paymentMethodId: "pm_card_visa" // o el que venga desde el frontend
                );

                // 3️⃣ Si el pago fue exitoso, actualizar el estado de la reserva
                if (payment.Status == "succeeded")
                {
                    createdReservation.Status = "Confirmed";
                    await _reservationService.UpdateReservationAsync(createdReservation);
                }
                else
                {
                    createdReservation.Status = "PaymentFailed";
                    await _reservationService.UpdateReservationAsync(createdReservation);
                }

                // 4️⃣ Devolver todo junto (reserva + pago)
                return Ok(new
                {
                    message = "Reserva y pago procesados correctamente.",
                    reservation = createdReservation,
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
        [HttpGet("users/{userId}/reservations")]
        [Authorize]  // Cualquier usuario autenticado, o restringe a userId == usuario actual
        public async Task<ActionResult<IEnumerable<Reservation>>> GetUserReservations(int userId)
        {
            var reservations = await _reservationService.GetReservationsByUserAsync(userId);  // Asume este método en IReservationService
            return Ok(reservations);
        }

        //GET /api/reservations/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _reservationService.GetReservationAsync(id);  // Asume este método
            if (reservation == null) return NotFound();
            return Ok(reservation);
        }

        // PUT /api/reservations/{id}/cancel
        [HttpPut("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var updatedReservation = await _reservationService.CancelReservationAsync(id);  // Asume este método en IReservationService
            if (updatedReservation == null) return NotFound();
            return Ok(updatedReservation);
        }

    }
}
