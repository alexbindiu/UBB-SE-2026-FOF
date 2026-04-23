using System;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using TicketSellingModule.Data.Domain;
using TicketSellingModule.Data.Services;

using Windows.Foundation.Metadata;

namespace TicketSellingModule.ViewModel
{
    public partial class EmployeesDashboardViewModel : ObservableObject
    {
        private readonly EmployeeService employeeService;
        public ObservableCollection<Employee> PilotEmployees { get; } = new();
        public ObservableCollection<Employee> FlightAttendantEmployees { get; } = new();
        public ObservableCollection<Employee> CoPilotEmployees { get; } = new();
        public ObservableCollection<Employee> FlightDispatcherEmployees { get; } = new();
        public ObservableCollection<Employee> OtherEmployees { get; } = new();

        [ObservableProperty]
        private Employee? selectedEmployee;

        [ObservableProperty]
        private Visibility dialogVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private string dialogTitle = string.Empty;

        [ObservableProperty]
        private Employee editingEmployee = new Employee();
        [ObservableProperty]
        private string dialogErrorMessage = string.Empty;
        [ObservableProperty]
        private DateTimeOffset? editingBirthday;
        [ObservableProperty]
        private DateTimeOffset? editingHiringDate;
        [ObservableProperty]
        private string editingSalaryText = string.Empty;

        [ObservableProperty]
        private Visibility confirmDeleteDialogVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private Employee? employeeToDelete;

        [ObservableProperty]
        private string deleteErrorMessage = string.Empty;
        public bool IsConfirmationVisible => EmployeeToDelete != null;
        public bool IsErrorOnlyVisible => EmployeeToDelete == null && !string.IsNullOrEmpty(DeleteErrorMessage);
        public EmployeesDashboardViewModel(EmployeeService employeeService)
        {
            this.employeeService = employeeService;
        }

        [RelayCommand]
        public void LoadData()
        {
            var allEmployees = employeeService.GetAll();

            ClearAllCollections();

            foreach (var employee in employeeService.GetPilots())
            {
                PilotEmployees.Add(employee);
            }

            foreach (var employee in employeeService.GetFlightAttendants())
            {
                FlightAttendantEmployees.Add(employee);
            }

            foreach (var employee in employeeService.GetCoPilots())
            {
                CoPilotEmployees.Add(employee);
            }

            foreach (var employee in employeeService.GetFlightDispatchers())
            {
                FlightDispatcherEmployees.Add(employee);
            }
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
                EmployeeToDelete = null;
                DeleteErrorMessage = "Please select an employee to delete.";
                ConfirmDeleteDialogVisibility = Visibility.Visible;

                // Refresh flags
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
            if (EmployeeToDelete == null || EmployeeToDelete.Id == 0)
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

            OnPropertyChanged(nameof(IsConfirmationVisible));
            OnPropertyChanged(nameof(IsErrorOnlyVisible));
        }

        [RelayCommand]
        private void AddEmployee(string targetRole)
        {
            EmployeeRole parsedRole = ParseRoleFromUserInterface(targetRole);

            EditingEmployee = new Employee { Role = parsedRole };
            EditingBirthday = null;
            EditingHiringDate = null;
            EditingSalaryText = string.Empty;

            DialogTitle = $"Add New {targetRole}";
            DialogErrorMessage = string.Empty;
            DialogVisibility = Visibility.Visible;
        }

        private EmployeeRole ParseRoleFromUserInterface(string requestedRoleText)
        {
            if (string.IsNullOrWhiteSpace(requestedRoleText))
            {
                return EmployeeRole.Other;
            }

            string normalizedText = requestedRoleText.Replace(" ", string.Empty).Replace("-", string.Empty);

            if (Enum.TryParse(normalizedText, true, out EmployeeRole result))
            {
                return result;
            }

            return EmployeeRole.Other;
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
                if (EditingBirthday.HasValue)
                {
                    EditingEmployee.Birthday = DateOnly.FromDateTime(EditingBirthday.Value.DateTime);
                }

                if (EditingHiringDate.HasValue)
                {
                    EditingEmployee.HiringDate = DateOnly.FromDateTime(EditingHiringDate.Value.DateTime);
                }

                employeeService.SaveEmployee(EditingEmployee, EditingBirthday, EditingHiringDate, EditingSalaryText);

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
