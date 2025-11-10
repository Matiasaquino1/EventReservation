using EventReservations.Models;
using EventReservations.Repositories;
using System.Threading.Tasks;
using Stripe;

namespace EventReservations.Services
{
    public interface IPaymentService
    {
        Task<Payment> ProcessPaymentAsync(int reservationId, decimal amount, string currency, string paymentMethodId);
        Task<Payment> GetPaymentAsync(int id);
        Task ProcessPaymentAsync(int reservationId, decimal amount);
        Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency);
        Task ProcessWebhookAsync(Stripe.Event stripeEvent);
        Task<Payment?> GetPaymentByReservationIdAsync(int reservationId);
        Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId);
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
    }

    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IConfiguration _configuration;
        private readonly IReservationRepository _reservationRepository; 

        public PaymentService(IPaymentRepository paymentRepository, IConfiguration configuration, IReservationRepository reservationRepository)
        {
            _paymentRepository = paymentRepository;
            _configuration = configuration;
            _reservationRepository = reservationRepository;
        }
        public async Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount)
        {
            var options = new PaymentIntentCreateOptions { Amount = (long)(amount * 100), Currency = "usd" };  // Ejemplo
            var service = new PaymentIntentService();
            return await service.CreateAsync(options);  // Usa tu clave de Stripe
        }

        public async Task ProcessWebhookAsync(Stripe.Event stripeEvent)
        {
            if (stripeEvent == null)
                throw new ArgumentNullException(nameof(stripeEvent), "El evento recibido desde Stripe es nulo.");

            try
            {
                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        {
                            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                            if (paymentIntent == null)
                                throw new InvalidOperationException("No se pudo obtener el PaymentIntent del evento.");

                            var payment = await _paymentRepository.GetByStripeIntentIdAsync(paymentIntent.Id);
                            if (payment == null)
                                throw new KeyNotFoundException($"No se encontró un pago con PaymentIntentId: {paymentIntent.Id}");

                            // Actualizar estado del pago
                            payment.Status = "Succeeded";
                            await _paymentRepository.UpdateAsync(payment);

                            // Actualizar estado de la reserva asociada
                            var reservation = await _reservationRepository.GetByIdAsync(payment.ReservationId);
                            if (reservation != null)
                            {
                                reservation.Status = "Confirmed";
                                await _reservationRepository.UpdateAsync(reservation);
                            }

                            Console.WriteLine($"✅ Pago exitoso procesado: {paymentIntent.Id}");
                            break;
                        }

                    case "payment_intent.payment_failed":
                        {
                            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                            var payment = await _paymentRepository.GetByStripeIntentIdAsync(paymentIntent.Id);

                            if (payment != null)
                            {
                                payment.Status = "Failed";
                                await _paymentRepository.UpdateAsync(payment);
                            }

                            Console.WriteLine($"❌ Pago fallido: {paymentIntent?.Id}");
                            break;
                        }

                    case "payment_intent.canceled":
                        {
                            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                            var payment = await _paymentRepository.GetByStripeIntentIdAsync(paymentIntent.Id);

                            if (payment != null)
                            {
                                payment.Status = "Canceled";
                                await _paymentRepository.UpdateAsync(payment);
                            }

                            Console.WriteLine($"⚠️ Pago cancelado: {paymentIntent?.Id}");
                            break;
                        }

                    default:
                        Console.WriteLine($"ℹ️ Evento no manejado: {stripeEvent.Type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❗Error procesando webhook de Stripe: {ex.Message}");
                throw; // O manejar con un logger según tu configuración
            }
        }



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

        public async Task<Payment?> GetPaymentByReservationIdAsync(int reservationId)
        {
            return await _paymentRepository.GetByReservationIdAsync(reservationId);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId)
        {
            return await _paymentRepository.GetPaymentsByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            return await _paymentRepository.GetAllAsync();
        }

    }
}
