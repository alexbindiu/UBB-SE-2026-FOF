using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSellingModule.Domain
{
    internal class Employee
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        private List<Flight> Flights;
        public List<Flight> GetParticipatingFlights()
        {
            return this.Fligths;
        }
        public void RemoveFromFlight(Flight flight)
        {
            Flights.remove(flight);
        }
        public void AddToFlight(Flight flight)
        {
            Flights.add(flight);
        }
    }
}
