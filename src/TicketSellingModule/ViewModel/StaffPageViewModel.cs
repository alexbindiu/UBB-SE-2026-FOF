using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

namespace TicketSellingModule.ViewModel
{
    public partial class StaffPageViewModel : ObservableObject
    {
        private readonly EmployeeService employeeService;
        private readonly EmployeeFlightService flightEmployeeService;
        private readonly RouteService routeService;
        private readonly GateService gateService;
        private readonly RunwayService runwayService;

        private int currentEmployeeId;

        public ObservableCollection<EmployeeScheduleItem> ScheduledFlights { get; } = new();

        [ObservableProperty] private string employeeIdText = "-";
        [ObservableProperty] private string roleText = "-";
        [ObservableProperty] private string flightsCountText = "0";
        [ObservableProperty] private Visibility emptyStateVisibility = Visibility.Collapsed;

        public StaffPageViewModel(
            EmployeeService employeeService,
            EmployeeFlightService flightEmployeeService,
            RouteService routeService,
            GateService gateService,
            RunwayService runwayService)
        {
            employeeService = employeeService;
            flightEmployeeService = flightEmployeeService;
            routeService = routeService;
            gateService = gateService;
            runwayService = runwayService;
        }

        public void Initialize(int employeeId)
        {
            int resolvedId = ResolveEmployeeId(employeeId);
            LoadEmployeeSchedule(resolvedId);
        }

        [RelayCommand]
        private void Refresh() => LoadEmployeeSchedule(currentEmployeeId);

        private int ResolveEmployeeId(int employeeId)
        {
            if (employeeId > 0)
            {
                return employeeId;
            }

            return employeeService.GetAll().FirstOrDefault()?.Id ?? 0;
        }

        private void LoadEmployeeSchedule(int employeeId)
        {
            ScheduledFlights.Clear();

            if (employeeId <= 0)
            {
                ResetEmployeeInfo();
                return;
            }

            currentEmployeeId = employeeId;

            var employee = employeeService.GetById(employeeId);
            if (employee == null)
            {
                ResetEmployeeInfo();
                return;
            }

            EmployeeIdText = employee.Id.ToString();
            RoleText = employee.Role;

            foreach (var flight in flightEmployeeService.GetEmployeeSchedule(employeeId).OrderBy(f => f.Date))
            {
                var route = routeService.GetById(flight.Route.Id);
                var gate = flight.Gate?.Id > 0 ? gateService.GetById(flight.Gate.Id) : null;
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
            if (runwayId <= 0)
            {
                return null;
            }

            try
            {
                return runwayService.GetById(runwayId);
            }
            catch
            {
                return null;
            }
        }

        private static string NormalizeFlightType(string? routeType)
        {
            if (string.IsNullOrWhiteSpace(routeType))
            {
                return "-";
            }

            string value = routeType.Trim().ToUpperInvariant();
            if (value.StartsWith("ARR") || value.StartsWith("ARRIVAL"))
            {
                return "ARR";
            }

            if (value.StartsWith("DEP") || value.StartsWith("DEPARTURE"))
            {
                return "DEP";
            }

            return value;
        }

        private static string GetRelevantTime(Route? route)
        {
            if (route == null)
            {
                return "-";
            }

            return NormalizeFlightType(route.RouteType) == "ARR"
                ? route.ArrivalTime.ToString("HH:mm")
                : route.DepartureTime.ToString("HH:mm");
        }
    }
}
