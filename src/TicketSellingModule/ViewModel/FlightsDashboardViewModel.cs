using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TicketSellingModule.Service;

using Microsoft.UI.Xaml;

using TicketSellingModule.WinUI.AirportAdmin.Components;

namespace TicketSellingModule.ViewModel
{
    public partial class FlightsDashboardViewModel : ObservableObject
    {
        private readonly FlightRouteService flightRouteService;
        private readonly EmployeeFlightService flightEmployeeService;

        private List<Flight> allFlights = new();

        [ObservableProperty] private string searchText = string.Empty;
        [ObservableProperty] private FlightRow? selectedFlight;

        [ObservableProperty] private Visibility crewDialogVisibility = Visibility.Collapsed;
        [ObservableProperty] private string dialogError = string.Empty;

        public ObservableCollection<CrewSelectionWrapper> AvailableCrew { get; } = new();
        public ObservableCollection<FlightRow> FilteredFlights { get; } = new();

        public FlightsDashboardViewModel(
            FlightRouteService flightRouteService,
            EmployeeFlightService flightEmployeeService)
        {
            this.flightRouteService = flightRouteService;
            this.flightEmployeeService = flightEmployeeService;
        }

        [RelayCommand]
        public void LoadFlights()
        {
            allFlights = flightRouteService.GetAllFlightsWithDetails();
            ApplyFilter();
        }

        partial void OnSearchTextChanged(string value) => ApplyFilter();

        [RelayCommand]
        private void OpenCrewManagement()
        {
            if (SelectedFlight == null)
            {
                return;
            }

            var flight = flightRouteService.GetFlightById(SelectedFlight.Id);
            if (flight == null)
            {
                return;
            }

            List<Employee> currentCrewMembers = flightEmployeeService.GetFlightCrew(flight.Id);
            List<int> currentCrewIdentifiers = new List<int>();

            foreach (Employee crewMember in currentCrewMembers)
            {
                currentCrewIdentifiers.Add(crewMember.Id);
            }

            List<Employee> availableEmployees = flightEmployeeService.GetAvailableCrewGroupedByRole(flight);

            AvailableCrew.Clear();

            EmployeeRole? previousRole = null;

            foreach (Employee candidateEmployee in availableEmployees)
            {
                EmployeeRole currentRole = candidateEmployee.Role;

                bool isFirstInGroup = currentRole != previousRole;

                AvailableCrew.Add(new CrewSelectionWrapper
                {
                    Employee = candidateEmployee,
                    IsSelected = currentCrewIdentifiers.Contains(candidateEmployee.Id),
                    RoleHeader = currentRole.ToString(),
                    RoleHeaderVisibility = isFirstInGroup ? Visibility.Visible : Visibility.Collapsed
                });

                previousRole = currentRole;
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

            var selectedIds = AvailableCrew.Where(x => x.IsSelected).Select(x => x.Employee.Id).ToList();

            flightEmployeeService.UpdateCrewForFlight(SelectedFlight.Id, selectedIds);
            CrewDialogVisibility = Visibility.Collapsed;
            LoadFlights();
        }

        [RelayCommand]
        private void CloseDialog() => CrewDialogVisibility = Visibility.Collapsed;

        private void ApplyFilter()
        {
            string text = SearchText?.Trim().ToLowerInvariant() ?? string.Empty;

            var filtered = string.IsNullOrWhiteSpace(text)
                ? allFlights
                : allFlights.Where(f =>
                       (f.FlightNumber?.ToLowerInvariant().Contains(text) ?? false) ||
                       f.Date.ToString("dd.MM.yyyy HH:mm").ToLowerInvariant().Contains(text) ||
                       (flightRouteService.GetDestinationText(f).ToLowerInvariant().Contains(text)) ||
                       (f.Runway?.Name?.ToLowerInvariant().Contains(text) ?? false) ||
                       (f.Gate?.Name?.ToLowerInvariant().Contains(text) ?? false))
                    .ToList();

            FilteredFlights.Clear();
            foreach (var flight in filtered)
            {
                var crew = flightEmployeeService.GetFlightCrew(flight.Id);
                FilteredFlights.Add(new FlightRow
                {
                    Id = flight.Id,
                    FlightNumber = flight.FlightNumber ?? string.Empty,
                    DateText = flight.Date.ToString("dd.MM.yyyy HH:mm"),
                    DestinationText = flightRouteService.GetDestinationText(flight),
                    RunwayText = flight.Runway?.Name ?? "-",
                    GateText = flight.Gate?.Name ?? "-",
                    CrewText = crew.Count > 0 ? string.Join(", ", crew.Select(c => c.Name)) : "Unassigned"
                });
            }
        }

        private static string GetDestinationText(Flight flight)
        {
            if (flight.Route?.Airport == null)
            {
                return "-";
            }

            return $"{flight.Route.Airport.AirportCode} - {flight.Route.Airport.AirportName}";
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