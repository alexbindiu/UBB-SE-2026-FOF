using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
            flightRouteService = flightRouteService;
            flightEmployeeService = flightEmployeeService;
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

            var available = flightEmployeeService.GetAvailableEmployeesForFlight(flight);
            var currentIds = flightEmployeeService.GetFlightCrew(flight.Id).Select(c => c.Id).ToList();

            AvailableCrew.Clear();

            var roleOrder = new Dictionary<string, int>
            {
                ["Pilot"] = 0,
                ["Co-Pilot"] = 1,
                ["Flight Attendant"] = 2,
                ["Flight Dispatcher"] = 3,
                ["Other"] = 4
            };

            static string NormalizeRole(string? role)
            {
                if (string.IsNullOrWhiteSpace(role))
                {
                    return "Other";
                }

                return role.Trim();
            }

            var grouped = available
                .GroupBy(e => NormalizeRole(e.Role))
                .OrderBy(g => roleOrder.TryGetValue(g.Key, out var idx) ? idx : int.MaxValue)
                .ThenBy(g => g.Key);

            foreach (var group in grouped)
            {
                bool firstInGroup = true;

                foreach (var emp in group.OrderBy(e => e.Name))
                {
                    AvailableCrew.Add(new CrewSelectionWrapper
                    {
                        Employee = emp,
                        IsSelected = currentIds.Contains(emp.Id),
                        ShowRoleHeader = firstInGroup,
                        RoleHeader = group.Key,
                        RoleHeaderVisibility = firstInGroup ? Visibility.Visible : Visibility.Collapsed
                    });

                    firstInGroup = false;
                }
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

            if (selectedIds.Count < 4)
            {
                DialogError = "You must select at least 4 employees.";
                return;
            }

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
                       (GetDestinationText(f).ToLowerInvariant().Contains(text)) ||
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
                    DestinationText = GetDestinationText(flight),
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