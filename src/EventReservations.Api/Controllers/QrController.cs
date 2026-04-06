using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace EventReservations.Controllers
{
    [ApiController]
    [Route("qr")]
    public class QrController : ControllerBase
    {
        [HttpGet("{reservationId}/qr")]
        [AllowAnonymous] 
        public IActionResult GetQr(int reservationId)
        {
            var qrContent = $"https://tuapp.com/validate/{reservationId}";

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);

            var pngBytes = qrCode.GetGraphic(8);

            return File(pngBytes, "image/png");
        }
    }
}