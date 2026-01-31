namespace EventReservations.Models
{
    public enum PaymentStatuses
    {
        Pending,        // creado localmente, esperando confirmación real
        Processing,     // Stripe lo está procesando
        Succeeded,      // pago confirmado por webhook
        Failed,         // falló
        Canceled        // cancelado manualmente o expirado
    }
}
