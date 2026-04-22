namespace TicketSellingModule.Data.Domain
{
    public class Flight
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string FlightNumber { get; set; }
        public Route Route { get; set; }
        public Runway Runway { get; set; }
        public Gate Gate { get; set; }
    }
}