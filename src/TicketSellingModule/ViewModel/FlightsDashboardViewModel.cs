using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using TicketSellingModule.Data.Services;
using TicketSellingModule.Data.Services.Interfaces;
using TicketSellingModule.WinUI.AirportAdmin.Components;

namespace TicketSellingModule.ViewModel
{
    public partial class FlightsDashboardViewModel(
       IFlightRouteService flightRouteService,
       IEmployeeFlightService flightEmployeeService) : ObservableObject
    {
        private List<Flight> allFlights = new();

        [ObservableProperty] private string searchText = string.Empty;
        [ObservableProperty] private FlightDisplayRow? selectedFlight;

        [ObservableProperty] private Visibility crewDialogVisibility = Visibility.Collapsed;
        [ObservableProperty] private string dialogError = string.Empty;

        public ObservableCollection<CrewSelectionWrapper> AvailableCrew { get; } = new();
        public ObservableCollection<FlightDisplayRow> FilteredFlights { get; } = new();

        [RelayCommand]
        public void LoadFlights()
        {
            allFlights = flightRouteService.GetAllFlightsWithDetails();
            ApplyFilter();
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilter();
        }

        [RelayCommand]
        private void OpenCrewManagement()
        {
            if (SelectedFlight == null)
            {
                return;
            }

            Flight? flight = flightRouteService.GetFlightById(SelectedFlight.Id);

            List<CrewMemberSelectionData> crewData = flightEmployeeService.GetCrewSelectionData(flight);

            AvailableCrew.Clear();
            foreach (CrewMemberSelectionData item in crewData)
            {
                AvailableCrew.Add(new CrewSelectionWrapper
                {
                    Employee = item.Employee,
                    IsSelected = item.IsSelected,
                    RoleHeader = item.RoleHeader,
                    RoleHeaderVisibility = item.IsFirstInRoleGroup ? Visibility.Visible : Visibility.Collapsed
                });
            }

            DialogError = string.Empty;
            CrewDialogVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void SaveCrew()
        {
            if (SelectedFlight == null)
            {
                return;
            }

            List<int> selectedEmployeeIds = new List<int>();
            foreach (CrewSelectionWrapper selectionContext in this.AvailableCrew)
            {
                if (selectionContext.IsSelected)
                {
                    selectedEmployeeIds.Add(selectionContext.Employee.Id);
                }
            }

            flightEmployeeService.UpdateEmployeesForFlightUsingIds(SelectedFlight.Id, selectedEmployeeIds);
            CrewDialogVisibility = Visibility.Collapsed;
            LoadFlights();
        }

        [RelayCommand]
        private void CloseDialog()
        {
            CrewDialogVisibility = Visibility.Collapsed;
        }

        private void ApplyFilter()
        {
            string query = SearchText?.Trim().ToLowerInvariant() ?? string.Empty;
            List<Flight> matchingFlights = flightRouteService.SearchFlights(allFlights, query);

            FilteredFlights.Clear();
            foreach (Flight flight in matchingFlights)
            {
                string crewText = flightEmployeeService.FormatCrewList(flight.Id);
                FilteredFlights.Add(new FlightDisplayRow(flightRouteService.BuildFlightSummary(flight, crewText)));
            }
        }
    }

    public class CrewSelectionWrapper
    {
        public Employee Employee { get; set; } = new();
        public bool IsSelected { get; set; }
        public bool ShowRoleHeader { get; set; }
        public string RoleHeader { get; set; } = string.Empty;
        public Visibility RoleHeaderVisibility { get; set; } = Visibility.Collapsed;
    }
}