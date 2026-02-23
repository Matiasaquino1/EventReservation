using EventReservations.Models;
using EventReservations.Repositories;
using Stripe;
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
        IReservationService reservationService,
        IConfiguration configuration) : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository = paymentRepository;
        private readonly IReservationRepository _reservationRepository = reservationRepository;
        private readonly IReservationService _reservationService = reservationService;
        private readonly IConfiguration _configuration = configuration;

        // ===============================
        // CREATE PAYMENT INTENT
        // ===============================
        public async Task<PaymentIntent> CreatePaymentIntentAsync(int reservationId, int userId)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId)
                ?? throw new InvalidOperationException("Reserva no encontrada.");

            if (reservation.UserId != userId)
                throw new UnauthorizedAccessException();

            if (reservation.Status != ReservationStatuses.Pending)
                throw new InvalidOperationException("La reserva no puede pagarse.");

            // Idempotencia
            if (!string.IsNullOrEmpty(reservation.PaymentIntentId))
            {
                var existingService = new PaymentIntentService();
                return await existingService.GetAsync(reservation.PaymentIntentId);
            }

            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(reservation.Amount * 100),
                Currency = "usd",
                AutomaticPaymentMethods = new()
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

            // Persistimos el intent en la reserva
            reservation.PaymentIntentId = intent.Id;
            await _reservationRepository.UpdateAsync(reservation);

            // Persistimos (o reutilizamos) el pago para que webhook/dominio tengan trazabilidad
            var existingPayment = await _paymentRepository.GetByReservationIdAsync(reservationId);
            if (existingPayment is null)
            {
                await _paymentRepository.AddAsync(new Payment
                {
                    ReservationId = reservationId,
                    Amount = reservation.Amount,
                    Status = PaymentStatuses.Pending,
                    StripePaymentIntentId = intent.Id
                });
            }
            else
            {
                existingPayment.StripePaymentIntentId = intent.Id;
                existingPayment.Amount = reservation.Amount;
                if (existingPayment.Status != PaymentStatuses.Succeeded)
                {
                    existingPayment.Status = PaymentStatuses.Pending;
                }

                await _paymentRepository.UpdateAsync(existingPayment);
            }

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

            var mappedStatus = MapStripeStatus(intent.Status);
            payment.Status = mappedStatus;
            payment.PaymentDate = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment);

            // La confirmación de reserva y decremento de tickets se hace en dominio transaccional
            if (mappedStatus == PaymentStatuses.Succeeded)
            {
                await _reservationService.ConfirmPaymentAndDecrementTicketsAsync(
                    payment.ReservationId,
                    intent.Id);
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
        private static PaymentStatuses MapStripeStatus(string status) => status switch
        {
            "succeeded" => PaymentStatuses.Succeeded,
            "processing" => PaymentStatuses.Processing,
            "requires_payment_method" => PaymentStatuses.Failed,
            "canceled" => PaymentStatuses.Canceled,
            _ => PaymentStatuses.Pending
        };
    }
}

