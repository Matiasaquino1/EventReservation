using EventReservations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace EventReservations.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QrController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public QrController(
            IReservationService reservationService
            )
        {
            _reservationService = reservationService;
        }

        [HttpGet("qr/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetQrByToken(Guid token)
        {
            try
            {
                var imageBytes = await _reservationService.GenerateQrCodeBytesByTokenAsync(token);
                return File(imageBytes, "image/png");
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}