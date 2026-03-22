using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSellingModule.Domain
{
    public class Route
    {
        public int Id { get; set; }
        public string RouteType { get; set; }
        public int RecurrenceInterval { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public TimeOnly DepartureTime { get; set; }
        public TimeOnly ArrivalTime { get; set; }
        public int Capacity { get; set; }
        public Company Company { get; set; }

        public Airport Airport { get; set; }
    }
}
