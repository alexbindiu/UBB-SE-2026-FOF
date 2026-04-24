using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TicketSellingModule.ViewModel
{
    public enum AirportAdminSection
    {
        Flights,
        Employees,
        Airportonfiguration
    }

    public partial class AirportAdminViewModel : ObservableObject
    {
        [ObservableProperty]
        private AirportAdminSection selectedSection = AirportAdminSection.Flights;

        public AirportAdminViewModel()
        {
        }

        public void Initialize()
        {
            SelectedSection = AirportAdminSection.Flights;
        }

        [RelayCommand]
        private void ShowFlights()
        {
            SelectedSection = AirportAdminSection.Flights;
        }

        [RelayCommand]
        private void ShowEmployees()
        {
            SelectedSection = AirportAdminSection.Employees;
        }

        [RelayCommand]
        private void ShowAirport()
        {
            SelectedSection = AirportAdminSection.Airportonfiguration;
        }
    }
}
