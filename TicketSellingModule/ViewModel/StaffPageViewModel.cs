using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using TicketSellingModule.Domain;

namespace TicketSellingModule.ViewModel
{
    public class StaffPageViewModel : ViewModelBase
    {
        private readonly EmployeeViewModel _employeeViewModel;
        private int _currentEmployeeId;
        private string _employeeIdText = "-";
        private string _roleText = "-";
        private string _flightsCountText = "0";
        private Visibility _emptyStateVisibility = Visibility.Collapsed;

        public StaffPageViewModel()
        {
            _employeeViewModel = new EmployeeViewModel();
            ScheduledFlights = new ObservableCollection<EmployeeScheduleItem>();
            RefreshCommand = new RelayCommand(Refresh);
        }

        public ObservableCollection<EmployeeScheduleItem> ScheduledFlights { get; }

        public string EmployeeIdText
        {
            get => _employeeIdText;
            private set => SetProperty(ref _employeeIdText, value);
        }

        public string RoleText
        {
            get => _roleText;
            private set => SetProperty(ref _roleText, value);
        }

        public string FlightsCountText
        {
            get => _flightsCountText;
            private set => SetProperty(ref _flightsCountText, value);
        }

        public Visibility EmptyStateVisibility
        {
            get => _emptyStateVisibility;
            private set => SetProperty(ref _emptyStateVisibility, value);
        }

        public ICommand RefreshCommand { get; }

        public void Initialize(int employeeId)
        {
            int resolvedId = ResolveEmployeeId(employeeId);
            LoadEmployeeSchedule(resolvedId);
        }

        private int ResolveEmployeeId(int employeeId)
        {
            if (employeeId > 0)
            {
                return employeeId;
            }

            var firstEmployee = _employeeViewModel.GetAllEmployees().FirstOrDefault();
            return firstEmployee?.Id ?? 0;
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

            var employee = _employeeViewModel.GetEmployeeInfo(employeeId);
            if (employee == null)
            {
                ResetEmployeeInfo();
                return;
            }

            EmployeeIdText = employee.Id.ToString();
            RoleText = employee.Role;

            var flights = _employeeViewModel.GetFlightEmployee(employeeId);
            foreach (var flight in flights.OrderBy(f => f.Date))
            {
                var route = _employeeViewModel.GetRouteInfo(flight.RouteId);
                var gate = _employeeViewModel.GetGateInfo(flight.GateId);
                var runway = _employeeViewModel.GetRunwayInfo(flight.RunwayId);

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

        private void Refresh()
        {
            LoadEmployeeSchedule(_currentEmployeeId);
        }

        private void ResetEmployeeInfo()
        {
            EmployeeIdText = "-";
            RoleText = "-";
            FlightsCountText = "0";
            EmptyStateVisibility = Visibility.Visible;
        }

        private static string NormalizeFlightType(string routeType)
        {
            if (string.IsNullOrWhiteSpace(routeType))
            {
                return "-";
            }

            string value = routeType.Trim().ToUpperInvariant();

            if (value.StartsWith("ARR"))
            {
                return "ARR";
            }

            if (value.StartsWith("DEP"))
            {
                return "DEP";
            }

            if (value.StartsWith("ARRIVAL"))
            {
                return "ARR";
            }

            if (value.StartsWith("DEPARTURE"))
            {
                return "DEP";
            }

            return value;
        }

        private static string GetRelevantTime(Route route)
        {
            if (route == null)
            {
                return "-";
            }

            string flightType = NormalizeFlightType(route.RouteType);

            if (flightType == "ARR")
            {
                return route.ArrivalTime.ToString("HH:mm");
            }

            return route.DepartureTime.ToString("HH:mm");
        }
    }
}
