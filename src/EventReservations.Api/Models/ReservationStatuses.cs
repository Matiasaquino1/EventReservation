namespace EventReservations.Models
{
    public enum ReservationStatuses
    {
        Pending,        // creada, tickets reservados lógicamente
        AwaitingPayment,// esperando confirmación de Stripe
        Confirmed,      // pago confirmado
        Cancelled,      // cancelada por usuario / timeout
        Expired         // no se pagó a tiempo
    }

}
