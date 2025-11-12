namespace EventReservations.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Fecha y hora del evento
        public DateTime? EventDate { get; set; } = DateTime.UtcNow;
        public string Location { get; set; }
        public string Status { get; set; } 
        public decimal Price { get; set; }
        public int TicketsAvailable { get; set; }  // Número de entradas disponibles

        public Reservation Reservation { get; set; }
        public User User { get; set; }
    }
}
