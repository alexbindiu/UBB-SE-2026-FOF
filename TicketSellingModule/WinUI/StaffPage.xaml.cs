using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

namespace TicketSellingModule.WinUI
{
    public sealed partial class StaffPage : Page
    {
        public ObservableCollection<MockFlight> TempFlightsList { get; set; }

        public StaffPage()
        {
            this.InitializeComponent();

            TempFlightsList = new ObservableCollection<MockFlight>
            {
                new MockFlight { FlightNumber = "KLM1093", FlightType = "DEP", GateName = "A14", RunwayName = "RWY09L", FlightTime = "14:15" },
                new MockFlight { FlightNumber = "BAW0512", FlightType = "ARR", GateName = "B22", RunwayName = "RWY09R", FlightTime = "15:30" }
            };

            FlightScheduleList.ItemsSource = TempFlightsList;
        }
    }

    public class MockFlight
    {
        public string FlightNumber { get; set; }
        public string FlightType { get; set; }
        public string GateName { get; set; }
        public string RunwayName { get; set; }
        public string FlightTime { get; set; }
    }
}