using EventReservations.Models;
using EventReservations.Repositories;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Threading.Tasks;

namespace EventReservations.Services
{
    public interface IPaymentService
    {
        Task<Payment> ProcessPaymentAsync(int reservationId, decimal amount, string currency, string paymentMethodId);
        Task<Payment> GetPaymentAsync(int id);
        Task ProcessPaymentAsync(int reservationId, decimal amount);
        Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount);
        //Task ProcessWebhookAsync(Stripe.Event stripeEvent);
    }

    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IConfiguration _configuration;

        public PaymentService(IPaymentRepository paymentRepository, IConfiguration configuration)
        {
            _paymentRepository = paymentRepository;
            _configuration = configuration;
        }
        public async Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount)
        {
            var options = new PaymentIntentCreateOptions { Amount = (long)(amount * 100), Currency = "usd" };  // Ejemplo
            var service = new PaymentIntentService();
            return await service.CreateAsync(options);  // Usa tu clave de Stripe
        }
        //public async Task ProcessWebhookAsync(Stripe.Event stripeEvent)
        //{
        //    if (stripeEvent.Type == "payment_intent.succeeded")
        //    {
        //        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        //        // Lógica: Actualiza Reservation y Payment en la DB
        //        await _paymentRepository.UpdatePaymentStatusAsync(paymentIntent.Id, "Succeeded");  // Asume método en repository
        //    }
        //    // Agrega más lógica según el evento
        //}


        public async Task<Payment> ProcessPaymentAsync(int reservationId, decimal amount, string currency, string paymentMethodId)
        {
            // 1️⃣ Inicializar Stripe con la clave secreta
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
            

            // 2️⃣ Crear el pago
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Stripe usa centavos
                Currency = currency,
                PaymentMethod = paymentMethodId,
                Confirm = true,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                    AllowRedirects = "never"
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            // 3️⃣ Guardar el resultado en tu base de datos
            var payment = new Payment
            {
                ReservationId = reservationId,
                Amount = amount,
                Status = paymentIntent.Status,
                PaymentDate = DateTime.UtcNow,
                StripePaymentIntentId = paymentIntent.Id
            };

            await _paymentRepository.AddAsync(payment);
            return payment;
        }
        

        public async Task<Payment> GetPaymentAsync(int id)
        {
            return await _paymentRepository.GetByIdAsync(id);
        }

        public Task ProcessPaymentAsync(int reservationId, decimal amount)
        {
            throw new NotImplementedException();
        }
    }
}
