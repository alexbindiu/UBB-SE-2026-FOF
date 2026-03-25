using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketSellingModule.Domain;
using TicketSellingModule.Repo;
using TicketSellingModule.Service;

namespace TicketSellingModule.ViewModel
{
    internal class EmployeeViewModel
    {
        private readonly EmployeeService _employeeService;
        private readonly FlightEmployeeService _flightEmployeeService;

        public ObservableCollection<Employee> EmployeesList { get; set; }
        public ObservableCollection<Flight> EmployeeFlightsList { get; set; }

        public EmployeeViewModel()
        {
            var connectionFactory = new DbConnectionFactory();

            _employeeService = new EmployeeService(new EmployeeRepo(connectionFactory));
            _flightEmployeeService = new FlightEmployeeService(new FlightEmployeeRepo(connectionFactory), new EmployeeRepo(connectionFactory), new FlightRepo(connectionFactory));

            EmployeesList = new ObservableCollection<Employee>();
            EmployeeFlightsList = new ObservableCollection<Flight>();
        }

        public List<Employee> GetAllEmployees()
        {
            var employees = _employeeService.GetAll();

            EmployeesList.Clear();
            foreach (var employee in employees)
            {
                EmployeesList.Add(employee);
            }

            return employees;
        }

        public Employee? GetEmployeeInfo(int employeeId)
        {
            return _employeeService.GetById(employeeId);
        }

        public List<Flight> GetFlightEmployee(int employeeId)
        {
            return _flightEmployeeService.GetEmployeeSchedule(employeeId);
        }
    }
}
