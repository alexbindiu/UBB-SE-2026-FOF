using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using TicketSellingModule.WinUI.AirportAdmin.Components;

namespace TicketSellingModule.ViewModel
{
    public partial class FlightsDashboardViewModel : ObservableObject
    {
        private readonly FlightRouteService _flightRouteService;
        private readonly EmployeeFlightService _flightEmployeeService;

        private List<Flight> _allFlights = new();

        [ObservableProperty] private string _searchText = string.Empty;
        [ObservableProperty] private FlightRow? _selectedFlight;

        [ObservableProperty] private Visibility _crewDialogVisibility = Visibility.Collapsed;
        [ObservableProperty] private string _dialogError = string.Empty;

        public ObservableCollection<CrewSelectionWrapper> AvailableCrew { get; } = new();
        public ObservableCollection<FlightRow> FilteredFlights { get; } = new();

        public FlightsDashboardViewModel(
            FlightRouteService flightRouteService,
            EmployeeFlightService flightEmployeeService)
        {
            _flightRouteService = flightRouteService;
            _flightEmployeeService = flightEmployeeService;
        }

        [RelayCommand]
        public void LoadFlights()
        {
            _allFlights = _flightRouteService.GetAllFlightsWithDetails();
            ApplyFilter();
        }

        partial void OnSearchTextChanged(string value) => ApplyFilter();

        [RelayCommand]
        private void OpenCrewManagement()
        {
            if (SelectedFlight == null) return;

            var flight = _flightRouteService.GetFlightById(SelectedFlight.Id);
            if (flight == null) return;

            var currentIds = _flightEmployeeService.GetFlightCrew(flight.Id).Select(c => c.Id).ToList();
            var availableEmployees = _flightEmployeeService.GetAvailableCrewGroupedByRole(flight);

            AvailableCrew.Clear();

            string previousRole = null;
            foreach (var emp in availableEmployees)
            {
                string currentRole = emp.Role?.Trim() ?? "Other";
                bool isFirstInGroup = currentRole != previousRole;

                AvailableCrew.Add(new CrewSelectionWrapper
                {
                    Employee = emp,
                    IsSelected = currentIds.Contains(emp.Id),
                    ShowRoleHeader = isFirstInGroup,
                    RoleHeader = currentRole,
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
            if (SelectedFlight == null) return;

            var selectedIds = AvailableCrew.Where(x => x.IsSelected).Select(x => x.Employee.Id).ToList();

            

            _flightEmployeeService.UpdateCrewForFlight(SelectedFlight.Id, selectedIds);
            CrewDialogVisibility = Visibility.Collapsed;
            LoadFlights();
        }

        [RelayCommand]
        private void CloseDialog() => CrewDialogVisibility = Visibility.Collapsed;

        private void ApplyFilter()
        {
            string text = SearchText?.Trim().ToLowerInvariant() ?? string.Empty;

            var filtered = string.IsNullOrWhiteSpace(text)
                ? _allFlights
                : _allFlights.Where(f =>
                       (f.FlightNumber?.ToLowerInvariant().Contains(text) ?? false) ||
                       f.Date.ToString("dd.MM.yyyy HH:mm").ToLowerInvariant().Contains(text) ||
                       (_flightRouteService.GetDestinationText(f).ToLowerInvariant().Contains(text)) ||
                       (f.Runway?.Name?.ToLowerInvariant().Contains(text) ?? false) ||
                       (f.Gate?.Name?.ToLowerInvariant().Contains(text) ?? false))
                    .ToList();

            FilteredFlights.Clear();
            foreach (var flight in filtered)
            {
                var crew = _flightEmployeeService.GetFlightCrew(flight.Id);
                FilteredFlights.Add(new FlightRow
                {
                    Id = flight.Id,
                    FlightNumber = flight.FlightNumber ?? string.Empty,
                    DateText = flight.Date.ToString("dd.MM.yyyy HH:mm"),
                    DestinationText = _flightRouteService.GetDestinationText(flight),
                    RunwayText = flight.Runway?.Name ?? "-",
                    GateText = flight.Gate?.Name ?? "-",
                    CrewText = crew.Count > 0 ? string.Join(", ", crew.Select(c => c.Name)) : "Unassigned"
                });
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