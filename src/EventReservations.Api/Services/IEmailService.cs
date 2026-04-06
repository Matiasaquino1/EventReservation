using Resend;
using QRCoder;
using EventReservations.Dto;

namespace EventReservations.Services
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(ConfirmationEmailDataDto data);
    }

    public class EmailService : IEmailService
    {
        private readonly IResend _resend;

        public EmailService(IResend resend)
        {
            _resend = resend;
        }

        public async Task SendConfirmationEmailAsync(ConfirmationEmailDataDto data)
        {
            // 1. Generamos los bytes del QR
            var qrBytes = GenerateQrBytes(data.ReservationId);

            var message = new EmailMessage
            {
                From = "onboarding@resend.dev",
                To = data.ToEmail,
                Subject = $"¡Reserva Confirmada: {data.EventTitle}!",
                HtmlBody = BuildHtmlBody(data, "cid:qrcode_reserva"), 
                Attachments = new List<EmailAttachment>
                {
                    new EmailAttachment
                    {
                        Filename = "codigo_qr.png",
                        Content = qrBytes, 
                        ContentId = "qrcode_reserva", 
                        ContentType = "image/png"
                    }
                }
            };

            await _resend.EmailSendAsync(message);
        }

        private static byte[] GenerateQrBytes(int reservationId)
        {
            var qrContent = $"https://tuapp.com/validate/{reservationId}";
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);

            return qrCode.GetGraphic(8); 
        }

        private static string BuildHtmlBody(Dto.ConfirmationEmailDataDto data, string qrBase64)
        {
            return $@"
            <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto; 
                        border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;'>
                
                <!-- Header -->
                <div style='background-color: #1976d2; padding: 24px; text-align: center;'>
                    <h1 style='color: #ffffff; margin: 0; font-size: 1.5rem;'>
                        ¡Reserva Confirmada!
                    </h1>
                </div>

                <!-- Cuerpo -->
                <div style='padding: 28px;'>
                    <h2 style='color: #1976d2; margin-top: 0;'>¡Hola, {data.UserName}!</h2>
                    <p style='color: #333; font-size: 1rem;'>
                        Tu reserva para <strong>{data.EventTitle}</strong> fue confirmada con éxito.
                    </p>

                    <!-- Detalle de la reserva -->
                    <table style='width: 100%; border-collapse: collapse; margin: 20px 0; font-size: 0.95rem;'>
                        <tr style='background-color: #f5f5f5;'>
                            <td style='padding: 10px 14px; color: #555;'>📅 Fecha del evento</td>
                            <td style='padding: 10px 14px; font-weight: bold;'>
                                {((DateTime)data.EventDate).ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-AR"))}
                            </td>
                        </tr>
                        <tr>
                            <td style='padding: 10px 14px; color: #555;'>🎟️ Entradas</td>
                            <td style='padding: 10px 14px; font-weight: bold;'>{data.NumberOfTickets}</td>
                        </tr>
                        <tr style='background-color: #f5f5f5;'>
                            <td style='padding: 10px 14px; color: #555;'>💳 Total abonado</td>
                            <td style='padding: 10px 14px; font-weight: bold;'>${data.Amount:N2}</td>
                        </tr>
                        <tr>
                            <td style='padding: 10px 14px; color: #555;'>🔖 N° de reserva</td>
                            <td style='padding: 10px 14px; font-weight: bold;'>#{data.ReservationId:D6}</td>
                        </tr>
                    </table>

                    <!-- QR -->
                    <div style='text-align: center; margin: 28px 0;'>
                        <p style='color: #555; margin-bottom: 12px;'>
                            Presentá este código en el ingreso al evento.
                            <br>
                            <strong>Es único e intransferible.</strong>
                        </p>
                        <img src='cid:qrcode_reserva' 
                            alt='Código QR de entrada'
                            style='width: 200px; height: 200px; border: 4px solid #1976d2; 
                                   border-radius: 8px; padding: 8px;' />
                        <p style='font-size: 0.8rem; color: #999; margin-top: 8px;'>
                            Reserva #{data.ReservationId:D6}
                        </p>
                    </div>
                </div>

                <!-- Footer -->
                <div style='background-color: #f9f9f9; padding: 16px; 
                            text-align: center; border-top: 1px solid #e0e0e0;'>
                    <p style='font-size: 0.8rem; color: #aaa; margin: 0;'>
                        Gracias por usar nuestra plataforma. Este es un correo automático.
                    </p>
                </div>
            </div>";
        }
    }
}

