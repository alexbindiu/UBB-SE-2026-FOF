using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSellingModule.Domain
{
    public class Flight
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string FlightNumber{ get; set; }
        public List<Employee> Employees { get; set; } = new List<Employee>();
        public Gate Gate { get; set; }
        public Runway Runway{ get; set; }
        public Route Route{ get; set; }
    }
}
