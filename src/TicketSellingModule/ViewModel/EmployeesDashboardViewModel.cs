using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

namespace TicketSellingModule.ViewModel
{
    public partial class EmployeesDashboardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly IEmployeeService employeeService;

        private const string ErrorMessageEmployeeNotSelected = "Please select an employee to delete.";
        private const string ErrorMessageInvalidEmployeeSelected = "Invalid employee selected.";
        private const string AddTitlePrefix = "Add New";
        private const string EditTitlePrefix = "Edit";

        public EmployeesDashboardViewModel(IEmployeeService employeeService)
        {
            this.employeeService = employeeService;
        }

        private ObservableCollection<Employee> pilotEmployees = new();
        public ObservableCollection<Employee> PilotEmployees
        {
            get => pilotEmployees;
            set
            {
                if (pilotEmployees != value)
                {
                    pilotEmployees = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<Employee> flightAttendantEmployees = new();
        public ObservableCollection<Employee> FlightAttendantEmployees
        {
            get => flightAttendantEmployees;
            set
            {
                if (flightAttendantEmployees != value)
                {
                    flightAttendantEmployees = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<Employee> coPilotEmployees = new();
        public ObservableCollection<Employee> CoPilotEmployees
        {
            get => coPilotEmployees;
            set
            {
                if (coPilotEmployees != value)
                {
                    coPilotEmployees = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<Employee> flightDispatcherEmployees = new();
        public ObservableCollection<Employee> FlightDispatcherEmployees
        {
            get => flightDispatcherEmployees;
            set
            {
                if (flightDispatcherEmployees != value)
                {
                    flightDispatcherEmployees = value;
                    OnPropertyChanged();
                }
            }
        }

        private Employee? selectedEmployee;
        public Employee? SelectedEmployee
        {
            get => selectedEmployee;
            set
            {
                if (selectedEmployee != value)
                {
                    selectedEmployee = value;
                    OnPropertyChanged();
                }
            }
        }

        private Visibility dialogVisibility = Visibility.Collapsed;
        public Visibility DialogVisibility
        {
            get => dialogVisibility;
            set
            {
                if (dialogVisibility != value)
                {
                    dialogVisibility = value;
                    OnPropertyChanged();
                }
            }
        }

        private string dialogTitle = string.Empty;
        public string DialogTitle
        {
            get => dialogTitle;
            set
            {
                if (dialogTitle != value)
                {
                    dialogTitle = value;
                    OnPropertyChanged();
                }
            }
        }

        private Employee editingEmployee = new();
        public Employee EditingEmployee
        {
            get => editingEmployee;
            set
            {
                if (editingEmployee != value)
                {
                    editingEmployee = value;
                    OnPropertyChanged();
                }
            }
        }

        private string dialogErrorMessage = string.Empty;
        public string DialogErrorMessage
        {
            get => dialogErrorMessage;
            set
            {
                if (dialogErrorMessage != value)
                {
                    dialogErrorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTimeOffset? editingBirthday;
        public DateTimeOffset? EditingBirthday
        {
            get => editingBirthday;
            set
            {
                if (editingBirthday != value)
                {
                    editingBirthday = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTimeOffset? editingHiringDate;
        public DateTimeOffset? EditingHiringDate
        {
            get => editingHiringDate;
            set
            {
                if (editingHiringDate != value)
                {
                    editingHiringDate = value;
                    OnPropertyChanged();
                }
            }
        }

        private string editingSalaryText = string.Empty;
        public string EditingSalaryText
        {
            get => editingSalaryText;
            set
            {
                if (editingSalaryText != value)
                {
                    editingSalaryText = value;
                    OnPropertyChanged();
                }
            }
        }

        private Visibility confirmDeleteDialogVisibility = Visibility.Collapsed;
        public Visibility ConfirmDeleteDialogVisibility
        {
            get => confirmDeleteDialogVisibility;
            set
            {
                if (confirmDeleteDialogVisibility != value)
                {
                    confirmDeleteDialogVisibility = value;
                    OnPropertyChanged();
                }
            }
        }

        private Employee? employeeToDelete;
        public Employee? EmployeeToDelete
        {
            get => employeeToDelete;
            set
            {
                if (employeeToDelete != value)
                {
                    employeeToDelete = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsConfirmationVisible));
                    OnPropertyChanged(nameof(IsErrorOnlyVisible));
                }
            }
        }

        private string deleteErrorMessage = string.Empty;
        public string DeleteErrorMessage
        {
            get => deleteErrorMessage;
            set
            {
                if (deleteErrorMessage != value)
                {
                    deleteErrorMessage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsErrorOnlyVisible));
                }
            }
        }

        public bool IsConfirmationVisible => EmployeeToDelete != null;
        public bool IsErrorOnlyVisible => EmployeeToDelete == null && !string.IsNullOrEmpty(DeleteErrorMessage);

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
                DeleteErrorMessage = ErrorMessageEmployeeNotSelected;
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
            if (EmployeeToDelete == null)
            {
                DeleteErrorMessage = ErrorMessageInvalidEmployeeSelected;
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
        }

        [RelayCommand]
        private void AddEmployee(string targetRole)
        {
            EditingEmployee = new Employee { Role = employeeService.ParseRole(targetRole) };
            EditingBirthday = null;
            EditingHiringDate = null;
            EditingSalaryText = string.Empty;

            DialogTitle = $"{AddTitlePrefix} {targetRole}";
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

            DialogTitle = $"{EditTitlePrefix} {employee.Role}";
            DialogErrorMessage = string.Empty;
            DialogVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void CloseDialog() => DialogVisibility = Visibility.Collapsed;

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

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}