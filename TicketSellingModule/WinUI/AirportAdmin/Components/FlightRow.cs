using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSellingModule.WinUI.AirportAdmin.Components
{
    public class FlightRow
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; } = "";
        public string DateText { get; set; } = "";
        public string DestinationText { get; set; } = "";
        public string RunwayText { get; set; } = "";
        public string GateText { get; set; } = "";
    }
}
