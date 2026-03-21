using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSellingModule.Domain
{
    public class Employee
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        private List<Flight> Flights = new List<Flight>();
        public List<Flight> GetParticipatingFlights()
        {
            return this.Flights;
        }
        public void RemoveFromFlight(Flight flight)
        {
            Flights.Remove(flight);
        }
        public void AddToFlight(Flight flight)
        {
            Flights.Add(flight);
        }
    }
}
