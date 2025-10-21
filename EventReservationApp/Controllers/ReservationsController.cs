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
        private readonly IPaymentService _paymentService;  // Para crear PaymentIntent

        public ReservationsController(IReservationService reservationService, IPaymentService paymentService)
        {
            _reservationService = reservationService;
            _paymentService = paymentService;
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


        // POST /api/events/{eventId}/reservations (crea reserva pendiente + PaymentIntent)
        //[HttpPost("events/{eventId}/reservations")]
        //[Authorize(Roles = "User")]  // Asume que solo usuarios pueden reservar
        //public async Task<ActionResult<Reservation>> CreateReservation(int eventId, [FromBody] Reservation reservation)
        //{
        //    reservation.EventId = eventId;  // Asocia el evento
        //    reservation.Status = "Pending";  // Establece como pendiente
        //    var createdReservation = await _reservationService.CreateReservationAsync(reservation);

        //    // Crea PaymentIntent con Stripe
        //    var paymentIntent = await _paymentService.CreatePaymentIntentAsync(createdReservation.Id, reservation.TotalAmount);  // Asume método en IPaymentService
        //    createdReservation.PaymentIntentId = paymentIntent.Id;  // Guarda el ID si es necesario
        //    return CreatedAtAction(nameof(GetReservation), new { id = createdReservation.Id }, createdReservation);
        //}


        // GET /api/users/{userId}/reservations
        [HttpGet("users/{userId}/reservations")]
        [Authorize]  // Cualquier usuario autenticado, o restringe a userId == usuario actual
        public async Task<ActionResult<IEnumerable<Reservation>>> GetUserReservations(int userId)
        {
            var reservations = await _reservationService.GetReservationsByUserAsync(userId);  // Asume este método en IReservationService
            return Ok(reservations);
        }

        // GET /api/reservations/{id}
        //[HttpGet("{id}")]
        //[Authorize]
        //public async Task<ActionResult<Reservation>> GetReservation(int id)
        //{
        //    var reservation = await _reservationService.GetReservationAsync(id);  // Asume este método
        //    if (reservation == null) return NotFound();
        //    return Ok(reservation);
        //}

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
