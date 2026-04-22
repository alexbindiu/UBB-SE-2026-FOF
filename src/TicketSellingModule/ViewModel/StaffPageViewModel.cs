using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

namespace TicketSellingModule.ViewModel
{
    public partial class StaffPageViewModel : ObservableObject
    {
        private readonly EmployeeService _employeeService;
        private readonly EmployeeFlightService _flightEmployeeService;

        private int _currentEmployeeId;

        public ObservableCollection<EmployeeScheduleItem> ScheduledFlights { get; } = new();

        [ObservableProperty] private string _employeeIdText = "-";
        [ObservableProperty] private string _roleText = "-";
        [ObservableProperty] private string _flightsCountText = "0";
        [ObservableProperty] private Visibility _emptyStateVisibility = Visibility.Collapsed;

        public StaffPageViewModel(
            EmployeeService employeeService,
            EmployeeFlightService flightEmployeeService)
        {
            _employeeService = employeeService;
            _flightEmployeeService = flightEmployeeService;
        }

        public void Initialize(int employeeId)
        {
            //int resolvedId = ResolveEmployeeId(employeeId);
            LoadEmployeeSchedule(employeeId);
        }

        [RelayCommand]
        private void Refresh() => LoadEmployeeSchedule(_currentEmployeeId);

        

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

            var scheduleItems = _flightEmployeeService.GetFormattedEmployeeSchedule(employeeId);
            foreach (var item in scheduleItems)
            {
                ScheduledFlights.Add(item);
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
    }
}
