using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using TicketSellingModule.Data;

namespace TicketSellingModule.ViewModel
{
    public partial class StaffPageViewModel(
        IEmployeeService employeeService,
        IEmployeeFlightService employeeFlightService) : ObservableObject
    {
        private const string PlaceholderValue = "-";
        private const string DefaultZeroCount = "0";

        private int currentEmployeeId;

        public ObservableCollection<EmployeeScheduleItem> ScheduledFlights { get; } = new();

        [ObservableProperty] private string employeeIdText = PlaceholderValue;
        [ObservableProperty] private string roleText = PlaceholderValue;
        [ObservableProperty] private string flightsCountText = DefaultZeroCount;
        [ObservableProperty] private Visibility emptyStateVisibility = Visibility.Collapsed;

        public void Initialize(int employeeId)
        {
            LoadEmployeeSchedule(employeeId);
        }

        [RelayCommand]
        private void Refresh()
        {
            LoadEmployeeSchedule(currentEmployeeId);
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

            Employee? employee = employeeService.GetEmployeeById(employeeId);
            if (employee == null)
            {
                ResetEmployeeInfo();
                return;
            }

            EmployeeIdText = employee.Id.ToString();
            RoleText = employee.Role.ToString();

            List<EmployeeScheduleItem> scheduleItems = employeeFlightService.GetFormattedEmployeeSchedule(employeeId);
            foreach (EmployeeScheduleItem item in scheduleItems)
            {
                ScheduledFlights.Add(item);
            }

            FlightsCountText = ScheduledFlights.Count.ToString();

            if (this.ScheduledFlights.Count == 0)
            {
                this.EmptyStateVisibility = Visibility.Visible;
            }
            else
            {
                this.EmptyStateVisibility = Visibility.Collapsed;
            }
        }

        private void ResetEmployeeInfo()
        {
            EmployeeIdText = PlaceholderValue;
            RoleText = PlaceholderValue;
            FlightsCountText = DefaultZeroCount;
            EmptyStateVisibility = Visibility.Visible;
        }
    }
}
