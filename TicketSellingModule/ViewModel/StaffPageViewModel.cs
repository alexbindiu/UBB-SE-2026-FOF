using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using TicketSellingModule.Domain;
using TicketSellingModule.Service;

namespace TicketSellingModule.ViewModel
{
    public partial class StaffPageViewModel : ObservableObject
    {
        private readonly EmployeeService _employeeService;
        private readonly EmployeeFlightService _flightEmployeeService;
        private readonly RouteService _routeService;
        private readonly GateService _gateService;
        private readonly RunwayService _runwayService;

        private int _currentEmployeeId;

        public ObservableCollection<EmployeeScheduleItem> ScheduledFlights { get; } = new();

        [ObservableProperty] private string _employeeIdText = "-";
        [ObservableProperty] private string _roleText = "-";
        [ObservableProperty] private string _flightsCountText = "0";
        [ObservableProperty] private Visibility _emptyStateVisibility = Visibility.Collapsed;

        public StaffPageViewModel(
            EmployeeService employeeService,
            EmployeeFlightService flightEmployeeService,
            RouteService routeService,
            GateService gateService,
            RunwayService runwayService)
        {
            _employeeService = employeeService;
            _flightEmployeeService = flightEmployeeService;
            _routeService = routeService;
            _gateService = gateService;
            _runwayService = runwayService;
        }

        public void Initialize(int employeeId)
        {
            int resolvedId = ResolveEmployeeId(employeeId);
            LoadEmployeeSchedule(resolvedId);
        }

        [RelayCommand]
        private void Refresh() => LoadEmployeeSchedule(_currentEmployeeId);

        private int ResolveEmployeeId(int employeeId)
        {
            if (employeeId > 0) return employeeId;
            return _employeeService.GetAll().FirstOrDefault()?.Id ?? 0;
        }

        private void LoadEmployeeSchedule(int employeeId)
        {
            ScheduledFlights.Clear();

            if (employeeId <= 0)
            {
                ResetEmployeeInfo();
                return;
            }

            _currentEmployeeId = employeeId;

            var employee = _employeeService.GetById(employeeId);
            if (employee == null)
            {
                ResetEmployeeInfo();
                return;
            }

            EmployeeIdText = employee.Id.ToString();
            RoleText = employee.Role;

            foreach (var flight in _flightEmployeeService.GetEmployeeSchedule(employeeId).OrderBy(f => f.Date))
            {
                var route = _routeService.GetById(flight.Route.Id);
                var gate = flight.Gate?.Id > 0 ? _gateService.GetById(flight.Gate.Id) : null;
                var runway = GetRunwaySafe(flight.Runway?.Id ?? 0);

                ScheduledFlights.Add(new EmployeeScheduleItem
                {
                    Id = flight.Id.ToString(),
                    FlightNumber = flight.FlightNumber,
                    FlightType = NormalizeFlightType(route?.RouteType),
                    Date = flight.Date.ToString("dd MMM yyyy"),
                    GateName = gate?.Name ?? "-",
                    RunwayName = runway?.Name ?? "-",
                    FlightTime = GetRelevantTime(route)
                });
            }

            FlightsCountText = ScheduledFlights.Count.ToString();
            EmptyStateVisibility = ScheduledFlights.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ResetEmployeeInfo()
        {
            EmployeeIdText = "-";
            RoleText = "-";
            FlightsCountText = "0";
            EmptyStateVisibility = Visibility.Visible;
        }

        private Runway? GetRunwaySafe(int runwayId)
        {
            if (runwayId <= 0) return null;
            try { return _runwayService.GetById(runwayId); }
            catch { return null; }
        }

        private static string NormalizeFlightType(string? routeType)
        {
            if (string.IsNullOrWhiteSpace(routeType)) return "-";
            string value = routeType.Trim().ToUpperInvariant();
            if (value.StartsWith("ARR") || value.StartsWith("ARRIVAL")) return "ARR";
            if (value.StartsWith("DEP") || value.StartsWith("DEPARTURE")) return "DEP";
            return value;
        }

        private static string GetRelevantTime(Route? route)
        {
            if (route == null) return "-";
            return NormalizeFlightType(route.RouteType) == "ARR"
                ? route.ArrivalTime.ToString("HH:mm")
                : route.DepartureTime.ToString("HH:mm");
        }
    }
}
