using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace TicketSellingModule.ViewModel
{
    public partial class EmployeesDashboardViewModel : ObservableObject
    {
        private readonly EmployeeService _employeeService;
        public ObservableCollection<Employee> PilotEmployees { get; } = new();
        public ObservableCollection<Employee> FlightAttendantEmployees { get; } = new();
        public ObservableCollection<Employee> CoPilotEmployees { get; } = new();
        public ObservableCollection<Employee> FlightDispatcherEmployees { get; } = new();

        [ObservableProperty]
        private Employee? _selectedEmployee;


        [ObservableProperty]
        private Visibility _dialogVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private string _dialogTitle = string.Empty;

        [ObservableProperty]
        private Employee _editingEmployee = new Employee();

        [ObservableProperty]
        private string _dialogErrorMessage = string.Empty;

        [ObservableProperty]
        private DateTimeOffset? _editingBirthday;

        [ObservableProperty]
        private DateTimeOffset? _editingHiringDate;

        [ObservableProperty]
        private string _editingSalaryText = string.Empty;

        [ObservableProperty]
        private Visibility _confirmDeleteDialogVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private Employee? _employeeToDelete;

        [ObservableProperty]
        private string _deleteErrorMessage = string.Empty;

        public EmployeesDashboardViewModel(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [RelayCommand]
        public void LoadData()
        {
            ClearAllCollections();

            foreach (var employee in _employeeService.GetPilots())
                PilotEmployees.Add(employee);

            foreach (var employee in _employeeService.GetFlightAttendants())
                FlightAttendantEmployees.Add(employee);

            foreach (var employee in _employeeService.GetCoPilots())
                CoPilotEmployees.Add(employee);

            foreach (var employee in _employeeService.GetFlightDispatchers())
                FlightDispatcherEmployees.Add(employee);
        }

        private void ClearAllCollections()
        {
            PilotEmployees.Clear();
            FlightAttendantEmployees.Clear();
            CoPilotEmployees.Clear();
            FlightDispatcherEmployees.Clear();
        }

        

        [RelayCommand]
        private void DeleteEmployee(object parameter)
        {
            if (parameter is not Employee employee || employee.Id == 0)
            {
                DeleteErrorMessage = "Please select an employee to delete.";
                ConfirmDeleteDialogVisibility = Visibility.Visible;
                return;
            }

            EmployeeToDelete = employee;
            DeleteErrorMessage = string.Empty;
            ConfirmDeleteDialogVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void ConfirmDelete()
        {
            if (EmployeeToDelete == null || EmployeeToDelete.Id == 0)
            {
                DeleteErrorMessage = "Invalid employee selected.";
                return;
            }

            try
            {
                _employeeService.DeleteWithAssignments(EmployeeToDelete.Id);

                ConfirmDeleteDialogVisibility = Visibility.Collapsed;
                DeleteErrorMessage = string.Empty;
                EmployeeToDelete = null;

                LoadData();
            }
            catch (Exception ex)
            {
                DeleteErrorMessage = $"Could not delete employee: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CancelDelete()
        {
            ConfirmDeleteDialogVisibility = Visibility.Collapsed;
            DeleteErrorMessage = string.Empty;
            EmployeeToDelete = null;
        }

        [RelayCommand]
        private void AddEmployee(string targetRole)
        {
            EditingEmployee = new Employee { Role = targetRole };
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
            if (employee == null) return;

            EditingEmployee = new Employee
            {
                Id = employee.Id,
                Name = employee.Name,
                Role = employee.Role,
                Salary = employee.Salary
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
                _employeeService.SaveEmployee(EditingEmployee, EditingBirthday, EditingHiringDate, EditingSalaryText);

                LoadData();
                DialogVisibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                DialogErrorMessage = ex.Message;
            }

        }
    }
}
