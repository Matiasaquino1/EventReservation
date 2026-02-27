using AutoMapper;
using EventReservations.Dto;
using EventReservations.Models;
using EventReservations.Profiles;
using EventReservations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Stripe;
using System.ComponentModel.DataAnnotations; // Para validación
using System.Security.Claims;

namespace EventReservations.Controllers
{
    /// <summary>
    /// Controlador para gestionar pagos y transacciones relacionadas con reservas de eventos.
    /// Utiliza Stripe para procesar pagos y webhooks.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IReservationService _reservationService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            IReservationService reservationService,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _reservationService = reservationService;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
        }


        /// <summary>
        /// Crea un Payment Intent en Stripe para iniciar un pago.
        /// Requiere autenticación.
        /// </summary>
        /// <param name="request">Datos para crear el intent, incluyendo Amount y Currency.</param>
        /// <returns>El clientSecret del Payment Intent.</returns>
        /// <response code="200">Payment Intent creado correctamente.</response>
        /// <response code="400">Datos inválidos.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPost("create-payment-intent")]
        [Authorize]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> CreatePaymentIntent(
            [FromBody] CreatePaymentIntentDto dto)
        {
            if (dto == null || dto.ReservationId <= 0)
                return BadRequest(new { error = "Reserva inválida." });

            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !int.TryParse(claim.Value, out var userId))
                return Unauthorized();

            try
            {
                var intent = await _paymentService.CreatePaymentIntentAsync(
                    dto.ReservationId,
                    userId
                );

                return Ok(new { clientSecret = intent.ClientSecret });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "No se pudo crear el intent de pago");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el historial de pagos. Para usuarios normales, devuelve sus propios pagos; para admins, todos los pagos.
        /// Requiere autenticación.
        /// </summary>
        /// <returns>Lista de pagos en formato DTO.</returns>
        /// <response code="200">Historial obtenido correctamente.</response>
        /// <response code="401">Usuario no autorizado.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet("history")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<PaymentDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetHistory()
        {
            try
            {
                IEnumerable<Payment> payments;

                if (User.IsInRole("Admin"))
                {
                    payments = await _paymentService.GetAllPaymentsAsync();
                }
                else
                {
                    var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (claim == null) return Unauthorized();
                    if (!int.TryParse(claim.Value, out var userId)) return Unauthorized();

                    payments = await _paymentService.GetPaymentsByUserIdAsync(userId);
                }

                // Cambiado: Mapear a PaymentDto (DTO de salida)
                var dtos = payments.Select(p => _mapper.Map<PaymentDto>(p));
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo historial de pagos");
                return StatusCode(500, new { error = "Error interno del servidor." });
            }
        }
    }

}


