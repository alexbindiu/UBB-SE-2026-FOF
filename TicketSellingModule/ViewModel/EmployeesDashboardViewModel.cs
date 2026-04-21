using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketSellingModule.Domain;
using TicketSellingModule.Service;

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

        public EmployeesDashboardViewModel(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [RelayCommand]
        public void LoadData()
        {
            // Assuming your service has a method like GetAllEmployeesAsync()
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
        private async Task DeleteEmployeeAsync(Employee employee)
        {
            
        }

        [RelayCommand]
        private void AddEmployee(string targetRole)
        {
            // Logic to open an "Add Employee" dialog or navigate to an Add page.
            // You can pass the targetRole so the dialog knows which type of employee to create.
        }

        [RelayCommand]
        private void EditEmployee(Employee employee)
        {
            // Logic to open an "Edit Employee" dialog or page, passing the selected employee.
        }

    }
}
