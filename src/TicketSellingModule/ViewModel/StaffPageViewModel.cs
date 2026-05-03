using System.ComponentModel;
using System.Runtime.CompilerServices;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Primitives;
using Microsoft.UI.Xaml;

using TicketSellingModule.Data;
using TicketSellingModule.Data.Services.Interfaces;

namespace TicketSellingModule.ViewModel
{
    public partial class StaffPageViewModel(
        IEmployeeService employeeService,
        IEmployeeFlightService employeeFlightService) : INotifyPropertyChanged
    {
        private const string PlaceholderValue = "-";
        private const string DefaultCount = "0";

        public event PropertyChangedEventHandler PropertyChanged;

        private int currentEmployeeId;

        private ObservableCollection<EmployeeScheduleItem> scheduledFlights;

        public ObservableCollection<EmployeeScheduleItem> ScheduledFlights
        {
            get => scheduledFlights;
            set
            {
                if (scheduledFlights != value)
                {
                    scheduledFlights = value;
                    OnPropertyChanged();
                }
            }
        }

        private string employeeIdText = PlaceholderValue;

        public string EmployeeIdText
        {
            get => employeeIdText;
            set
            {
                if (employeeIdText != value)
                {
                    employeeIdText = value;
                    OnPropertyChanged();
                }
            }
        }

        private string roleText = PlaceholderValue;

        public string RoleText
        {
            get => roleText;
            set
            {
                if (roleText != value)
                {
                    roleText = value;
                    OnPropertyChanged();
                }
            }
        }

        private string flightsCountText = DefaultCount;

        public string FlightsCountText
        {
            get => flightsCountText;
            set
            {
                if (flightsCountText != value)
                {
                    flightsCountText = value;
                    OnPropertyChanged();
                }
            }
        }

        private Visibility emptyStateVisibility = Visibility.Collapsed;

        public Visibility EmptyStateVisibility
        {
            get => emptyStateVisibility;
            set
            {
                if (emptyStateVisibility != value)
                {
                    emptyStateVisibility = value;
                    OnPropertyChanged();
                }
            }
        }

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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
