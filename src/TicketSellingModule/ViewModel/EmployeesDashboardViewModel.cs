using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using TicketSellingModule.Data.Services.Interfaces;

namespace TicketSellingModule.ViewModel
{
    public partial class EmployeesDashboardViewModel : ObservableObject
    {
        private readonly IEmployeeService employeeService;

        [ObservableProperty] private ObservableCollection<Employee> pilotEmployees = new();
        [ObservableProperty] private ObservableCollection<Employee> flightAttendantEmployees = new();
        [ObservableProperty] private ObservableCollection<Employee> coPilotEmployees = new();
        [ObservableProperty] private ObservableCollection<Employee> flightDispatcherEmployees = new();

        [ObservableProperty] private Employee? selectedEmployee;
        [ObservableProperty] private Visibility dialogVisibility = Visibility.Collapsed;
        [ObservableProperty] private string dialogTitle = string.Empty;
        [ObservableProperty] private Employee editingEmployee = new();
        [ObservableProperty] private string dialogErrorMessage = string.Empty;
        [ObservableProperty] private DateTimeOffset? editingBirthday;
        [ObservableProperty] private DateTimeOffset? editingHiringDate;
        [ObservableProperty] private string editingSalaryText = string.Empty;
        [ObservableProperty] private Visibility confirmDeleteDialogVisibility = Visibility.Collapsed;
        [ObservableProperty] private Employee? employeeToDelete;
        [ObservableProperty] private string deleteErrorMessage = string.Empty;

        public bool IsConfirmationVisible => EmployeeToDelete != null;
        public bool IsErrorOnlyVisible => EmployeeToDelete == null && !string.IsNullOrEmpty(DeleteErrorMessage);

        public EmployeesDashboardViewModel(IEmployeeService employeeService)
        {
            this.employeeService = employeeService;
        }

        [RelayCommand]
        public void LoadData()
        {
            PilotEmployees = new ObservableCollection<Employee>(employeeService.GetPilots());
            FlightAttendantEmployees = new ObservableCollection<Employee>(employeeService.GetFlightAttendants());
            CoPilotEmployees = new ObservableCollection<Employee>(employeeService.GetCoPilots());
            FlightDispatcherEmployees = new ObservableCollection<Employee>(employeeService.GetFlightDispatchers());
        }

        [RelayCommand]
        private void DeleteEmployee(object parameter)
        {
            if (parameter is not Employee employee)
            {
                EmployeeToDelete = null;
                DeleteErrorMessage = "Please select an employee to delete.";
                ConfirmDeleteDialogVisibility = Visibility.Visible;
                OnPropertyChanged(nameof(IsConfirmationVisible));
                OnPropertyChanged(nameof(IsErrorOnlyVisible));
                return;
            }

            EmployeeToDelete = employee;
            DeleteErrorMessage = string.Empty;
            ConfirmDeleteDialogVisibility = Visibility.Visible;
            OnPropertyChanged(nameof(IsConfirmationVisible));
            OnPropertyChanged(nameof(IsErrorOnlyVisible));
        }

        [RelayCommand]
        private void ConfirmDelete()
        {
            if (EmployeeToDelete == null)
            {
                DeleteErrorMessage = "Invalid employee selected.";
                return;
            }

            try
            {
                employeeService.DeleteWithAssignments(EmployeeToDelete.Id);
                ConfirmDeleteDialogVisibility = Visibility.Collapsed;
                DeleteErrorMessage = string.Empty;
                EmployeeToDelete = null;
                LoadData();
            }
            catch (Exception exception)
            {
                DeleteErrorMessage = $"Could not delete employee: {exception.Message}";
            }
        }

        [RelayCommand]
        private void CancelDelete()
        {
            ConfirmDeleteDialogVisibility = Visibility.Collapsed;
            DeleteErrorMessage = string.Empty;
            EmployeeToDelete = null;
            OnPropertyChanged(nameof(IsConfirmationVisible));
            OnPropertyChanged(nameof(IsErrorOnlyVisible));
        }

        [RelayCommand]
        private void AddEmployee(string targetRole)
        {
            EditingEmployee = new Employee { Role = employeeService.ParseRole(targetRole) };
            EditingBirthday = null;
            EditingHiringDate = null;
            EditingSalaryText = string.Empty;

            DialogTitle = $"Add New {targetRole}";
            DialogErrorMessage = string.Empty;
            DialogVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void EditEmployee(Employee employee)
        {
            if (employee == null)
            {
                return;
            }

            EditingEmployee = new Employee
            {
                Id = employee.Id,
                Name = employee.Name,
                Role = employee.Role,
                Salary = employee.Salary,
                Birthday = employee.Birthday,
                HiringDate = employee.HiringDate
            };

            EditingSalaryText = employee.Salary.ToString();
            EditingBirthday = new DateTimeOffset(employee.Birthday.ToDateTime(TimeOnly.MinValue));
            EditingHiringDate = new DateTimeOffset(employee.HiringDate.ToDateTime(TimeOnly.MinValue));

            DialogTitle = $"Edit {employee.Role}";
            DialogErrorMessage = string.Empty;
            DialogVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void CloseDialog()
        {
            DialogVisibility = Visibility.Collapsed;
        }

        [RelayCommand]
        private void SaveEmployee()
        {
            try
            {
                employeeService.SaveEmployee(EditingEmployee, EditingBirthday, EditingHiringDate, EditingSalaryText);
                LoadData();
                DialogVisibility = Visibility.Collapsed;
            }
            catch (Exception exception)
            {
                DialogErrorMessage = exception.Message;
            }
        }
    }
}