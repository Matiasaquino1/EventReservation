using EventReservations.Models;
using EventReservations.Repositories;
using Stripe;
using Stripe.V2;
using System.Threading.Tasks;

namespace EventReservations.Services
{
    public interface IPaymentService
    {
        Task<PaymentIntent> CreatePaymentIntentAsync(int reservationId, int userId);
        Task ProcessWebhookAsync(Stripe.Event stripeEvent);

        Task<Payment?> GetPaymentByReservationIdAsync(int reservationId);
        Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId);
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();

    }

    public class PaymentService(
        IPaymentRepository paymentRepository,
        IReservationRepository reservationRepository,
        IConfiguration configuration) : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository = paymentRepository;
        private readonly IReservationRepository _reservationRepository = reservationRepository;
        private readonly IConfiguration _configuration = configuration;

        // ===============================
        // CREATE PAYMENT INTENT
        // ===============================
        public async Task<PaymentIntent> CreatePaymentIntentAsync(int reservationId, int userId)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation == null)
                throw new InvalidOperationException("Reserva no encontrada.");

            if (reservation.UserId != userId)
                throw new UnauthorizedAccessException();

            if (reservation.Status != ReservationStatuses.Pending)
                throw new InvalidOperationException("La reserva no puede pagarse.");

            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(reservation.Amount * 100),
                Currency = "usd",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                },
                Metadata = new Dictionary<string, string>
                {
                    ["reservationId"] = reservationId.ToString(),
                    ["userId"] = userId.ToString()
                }
            };

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);

            // el intent asociado
            reservation.PaymentIntentId = intent.Id;

            await _paymentRepository.AddAsync(new Payment
            {
                ReservationId = reservationId,
                Amount = reservation.Amount,
                Status = PaymentStatuses.Pending,
                StripePaymentIntentId = intent.Id,
                PaymentDate = DateTime.UtcNow
            });

            return intent;
        }

        // ===============================
        // STRIPE WEBHOOK
        // ===============================
        public async Task ProcessWebhookAsync(Stripe.Event stripeEvent)
        {
            if (stripeEvent?.Data?.Object is not PaymentIntent intent)
                throw new InvalidOperationException("Evento inválido de Stripe.");

            var payment = await _paymentRepository.GetByStripeIntentIdAsync(intent.Id);
            if (payment == null)
                throw new KeyNotFoundException($"Pago no encontrado ({intent.Id}).");

            payment.Status = MapStripeStatus(intent.Status);
            await _paymentRepository.UpdateAsync(payment);

            // No confirma la reserva acá, por si falla
            if (payment.Status == PaymentStatuses.Succeeded)
            {
                var reservation = await _reservationRepository.GetByIdAsync(payment.ReservationId);
                if (reservation != null)
                {
                    reservation.Status = ReservationStatuses.Confirmed;
                    await _reservationRepository.UpdateAsync(reservation);
                }
            }
        }

        // ===============================
        // QUERIES
        // ===============================
        public async Task<Payment?> GetPaymentByReservationIdAsync(int reservationId)
            => await _paymentRepository.GetByReservationIdAsync(reservationId);

        public async Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId)
            => await _paymentRepository.GetPaymentsByUserIdAsync(userId);

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
            => await _paymentRepository.GetAllAsync();

        // ===============================
        // PRIVATE MAPPER
        // ===============================
        private static PaymentStatuses MapStripeStatus(string stripeStatus)
        {
            return stripeStatus switch
            {
                "succeeded" => PaymentStatuses.Succeeded,
                "processing" => PaymentStatuses.Pending,
                "requires_payment_method" => PaymentStatuses.Failed,
                "canceled" => PaymentStatuses.Canceled,
                _ => PaymentStatuses.Pending
            };
        }
    }
}

