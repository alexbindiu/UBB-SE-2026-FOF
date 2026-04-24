using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using TicketSellingModule.Data;
using TicketSellingModule.Data.Services.Interfaces;

namespace TicketSellingModule.ViewModel
{
    public partial class StaffPageViewModel : ObservableObject
    {
        private readonly IEmployeeService employeeService;
        private readonly IEmployeeFlightService flightEmployeeService;

        private int currentEmployeeId;

        public ObservableCollection<EmployeeScheduleItem> ScheduledFlights { get; } = new();

        [ObservableProperty] private string employeeIdText = "-";
        [ObservableProperty] private string roleText = "-";
        [ObservableProperty] private string flightsCountText = "0";
        [ObservableProperty] private Visibility emptyStateVisibility = Visibility.Collapsed;

        public StaffPageViewModel(
            IEmployeeService employeeService,
            IEmployeeFlightService flightEmployeeService)
        {
            this.employeeService = employeeService;
            this.flightEmployeeService = flightEmployeeService;
        }

        public void Initialize(int employeeId)
        {
            LoadEmployeeSchedule(employeeId);
        }

        [RelayCommand]
        private void Refresh() => LoadEmployeeSchedule(currentEmployeeId);

        private void LoadEmployeeSchedule(int employeeId)
        {
            ScheduledFlights.Clear();

            if (employeeId <= 0)
            {
                ResetEmployeeInfo();
                return;
            }

            currentEmployeeId = employeeId;

            var employee = employeeService.GetEmployeeById(employeeId);
            if (employee == null)
            {
                ResetEmployeeInfo();
                return;
            }

            EmployeeIdText = employee.Id.ToString();
            RoleText = employee.Role.ToString();
            var scheduleItems = flightEmployeeService.GetFormattedEmployeeSchedule(employeeId);
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
