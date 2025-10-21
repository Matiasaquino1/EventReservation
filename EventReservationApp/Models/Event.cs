namespace EventReservations.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateTime { get; set; }  // Fecha y hora del evento
        public string Location { get; set; }
        public string Status { get; set; } 
        public decimal Price { get; set; }
        public int TicketsAvailable { get; set; }  // Número de entradas disponibles
    }
}
