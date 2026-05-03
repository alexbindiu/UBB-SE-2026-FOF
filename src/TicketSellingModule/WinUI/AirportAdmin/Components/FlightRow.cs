namespace TicketSellingModule.WinUI.AirportAdmin.Components
{
    public class FlightDisplayRow
    {
        public FlightDisplayRow(FlightSummary summary)
        {
            Id = summary.Id;
            FlightNumber = summary.FlightNumber;
            DateText = summary.DateText;
            DestinationText = summary.DestinationText;
            RunwayText = summary.RunwayText;
            GateText = summary.GateText;
            CrewText = summary.CrewText;
        }

        public int Id { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string DateText { get; set; } = string.Empty;
        public string DestinationText { get; set; } = string.Empty;
        public string RunwayText { get; set; } = string.Empty;
        public string GateText { get; set; } = string.Empty;
        public string CrewText { get; set; } = string.Empty;
    }
}
