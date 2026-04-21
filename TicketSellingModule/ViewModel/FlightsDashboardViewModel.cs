using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using TicketSellingModule.Domain;
using TicketSellingModule.Service;
using TicketSellingModule.WinUI.AirportAdmin.Components;

namespace TicketSellingModule.ViewModel
{
    public partial class FlightsDashboardViewModel : ObservableObject
    {
        private readonly FlightRouteService _flightRouteService;
        private readonly FlightEmployeeService _flightEmployeeService;

        private List<Flight> _allFlights = new();

        [ObservableProperty] private string _searchText = string.Empty;
        [ObservableProperty] private FlightRow? _selectedFlight;

        [ObservableProperty] private Visibility _crewDialogVisibility = Visibility.Collapsed;
        [ObservableProperty] private string _dialogError = string.Empty;

        public ObservableCollection<CrewSelectionWrapper> AvailableCrew { get; } = new();
        public ObservableCollection<FlightRow> FilteredFlights { get; } = new();

        public FlightsDashboardViewModel(
            FlightRouteService flightRouteService,
            FlightEmployeeService flightEmployeeService)
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

            var available = _flightEmployeeService.GetAvailableEmployeesForFlight(flight);
            var currentIds = _flightEmployeeService.GetFlightCrew(flight.Id).Select(c => c.Id).ToList();

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
                if (string.IsNullOrWhiteSpace(role)) return "Other";
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
            if (SelectedFlight == null) return;

            var selectedIds = AvailableCrew.Where(x => x.IsSelected).Select(x => x.Employee.Id).ToList();

            if (selectedIds.Count < 4)
            {
                DialogError = "You must select at least 4 employees.";
                return;
            }

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
                       (GetDestinationText(f).ToLowerInvariant().Contains(text)) ||
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
                return "-";

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