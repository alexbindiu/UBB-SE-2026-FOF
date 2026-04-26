using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using TicketSellingModule.Data;
using TicketSellingModule.Data.Services.Interfaces;

namespace TicketSellingModule.ViewModel
{
    public partial class StaffPageViewModel(
        IEmployeeService employeeService,
        IEmployeeFlightService employeeFlightService) : ObservableObject
    {
        private const string PlaceholderValue = "-";
        private const string DefaultZeroCount = "0";

        private int currentEmployeeId;

        [ObservableProperty] private ObservableCollection<EmployeeScheduleItem> scheduledFlights;

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
            currentEmployeeId = employeeId;

            Employee? employee = employeeService.GetEmployeeById(employeeId);

            EmployeeIdText = employee.Id.ToString();
            RoleText = employee.Role.ToString();

            List<EmployeeScheduleItem> scheduleItems = employeeFlightService.GetFormattedEmployeeSchedule(employeeId);
            ScheduledFlights = new ObservableCollection<EmployeeScheduleItem>(scheduleItems);

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
