using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TicketSellingModule.Domain;
using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI
{
    public sealed partial class StaffPage : Page
    {
        public EmployeeViewModel ViewModel { get; }
        public ObservableCollection<EmployeeScheduleItem> ScheduledFlights { get; }

        private int _currentEmployeeId;

        public StaffPage()
        {
            ViewModel = new EmployeeViewModel();
            ScheduledFlights = new ObservableCollection<EmployeeScheduleItem>();

            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            int employeeId = ResolveEmployeeId(e.Parameter);
            LoadEmployeeSchedule(employeeId);
        }

        private int ResolveEmployeeId(object parameter)
        {
            if (parameter is int employeeId && employeeId > 0)
            {
                return employeeId;
            }

            var firstEmployee = ViewModel.GetAllEmployees().FirstOrDefault();
            return firstEmployee?.Id ?? 0;
        }

        private void LoadEmployeeSchedule(int employeeId)
        {
            ScheduledFlights.Clear();

            if (employeeId <= 0)
            {
                //EmployeeNameText.Text = "EMPLOYEE DASHBOARD";
                //AvatarText.Text = "E";
                EmployeeIdText.Text = "-";
                RoleText.Text = "-";
                FlightsCountText.Text = "0";
                EmptyStateText.Visibility = Visibility.Visible;
                return;
            }

            _currentEmployeeId = employeeId;

            var employee = ViewModel.GetEmployeeInfo(employeeId);
            if (employee == null)
            {
                //EmployeeNameText.Text = "EMPLOYEE DASHBOARD";
                //AvatarText.Text = "E";
                EmployeeIdText.Text = "-";
                RoleText.Text = "-";
                FlightsCountText.Text = "0";
                EmptyStateText.Visibility = Visibility.Visible;
                return;
            }

            //EmployeeNameText.Text = employee.Name.ToUpperInvariant();
            //AvatarText.Text = GetInitials(employee.Name);
            EmployeeIdText.Text = employee.Id.ToString();
            RoleText.Text = employee.Role;

            var flights = ViewModel.GetFlightEmployee(employeeId);

            foreach (var flight in flights.OrderBy(f => f.Date))
            {
                var route = ViewModel.GetRouteInfo(flight.RouteId);
                var gate = ViewModel.GetGateInfo(flight.GateId);
                var runway = ViewModel.GetRunwayInfo(flight.RunwayId);

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

            FlightsCountText.Text = ScheduledFlights.Count.ToString();
            EmptyStateText.Visibility = ScheduledFlights.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadEmployeeSchedule(_currentEmployeeId);
        }

        private static string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return "E";
            }

            var parts = fullName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length >= 2)
            {
                return $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[1][0])}";
            }

            if (parts[0].Length >= 2)
            {
                return parts[0].Substring(0, 2).ToUpper();
            }

            return parts[0].Substring(0, 1).ToUpper();
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

    public class EmployeeScheduleItem
    {
        public string Id { get; set; }
        public string FlightNumber { get; set; }
        public string FlightType { get; set; }
        public string Date { get; set; }
        public string GateName { get; set; }
        public string RunwayName { get; set; }
        public string FlightTime { get; set; }
    }
}