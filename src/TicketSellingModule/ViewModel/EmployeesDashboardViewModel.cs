using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace TicketSellingModule.ViewModel
{
    public partial class EmployeesDashboardViewModel : ObservableObject
    {
        private readonly EmployeeService _employeeService;
        private readonly EmployeeFlightService _employeeFlightService;
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

        public EmployeesDashboardViewModel(EmployeeService employeeService, EmployeeFlightService employeeFlightService)
        {
            _employeeService = employeeService;
            _employeeFlightService = employeeFlightService;
        }

        [RelayCommand]
        public void LoadData()
        {
            var allEmployees = _employeeService.GetAll();

            ClearAllCollections();

            foreach (var employee in allEmployees)
            {
                switch (employee.Role)
                {
                    case "Pilot":
                        PilotEmployees.Add(employee);
                        break;
                    case "Flight Attendant":
                        FlightAttendantEmployees.Add(employee);
                        break;
                    case "Co-Pilot":
                        CoPilotEmployees.Add(employee);
                        break;
                    case "Flight Dispatcher":
                        FlightDispatcherEmployees.Add(employee);
                        break;
                }
            }
        }

        private void ClearAllCollections()
        {
            PilotEmployees.Clear();
            FlightAttendantEmployees.Clear();
            CoPilotEmployees.Clear();
            FlightDispatcherEmployees.Clear();
        }

        private void RemoveFromCollections(Employee employee)
        {
            PilotEmployees.Remove(employee);
            FlightAttendantEmployees.Remove(employee);
            CoPilotEmployees.Remove(employee);
            FlightDispatcherEmployees.Remove(employee);
        }

        [RelayCommand]
        private void DeleteEmployee()
        {
            if (SelectedEmployee == null) return;

            try
            {
                _employeeFlightService.CleanUpEmployeeAssignments(SelectedEmployee.Id);

                _employeeService.Delete(SelectedEmployee.Id);

                LoadData();
            }
            catch (Exception ex)
            {
                DialogErrorMessage = $"Could not delete employee: {ex.Message}";
                DialogVisibility = Visibility.Visible;
            }
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

                if (EditingBirthday == null || EditingHiringDate == null)
                {
                    DialogErrorMessage = "Birthday and Hiring Date are required.";
                    return;
                }

                if (!int.TryParse(EditingSalaryText, out int parsedSalary))
                {
                    DialogErrorMessage = "Salary must be a valid number.";
                    return;
                }


                DateOnly finalBirthday = DateOnly.FromDateTime(EditingBirthday.Value.DateTime);
                DateOnly finalHiringDate = DateOnly.FromDateTime(EditingHiringDate.Value.DateTime);
                EditingEmployee.Salary = parsedSalary;

  
                if (EditingEmployee.Id == 0)
                {
                    _employeeService.Add(
                        name: EditingEmployee.Name,
                        role: EditingEmployee.Role,
                        birthday: finalBirthday,
                        salary: EditingEmployee.Salary,
                        hiringDate: finalHiringDate
                    );
                }
                else
                {
                    _employeeService.Update(
                        id: EditingEmployee.Id,
                        name: EditingEmployee.Name,
                        role: EditingEmployee.Role,
                        salary: EditingEmployee.Salary
                    );
                }


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
