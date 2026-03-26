using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TicketSellingModule.Domain;
using TicketSellingModule.Repo;
using TicketSellingModule.Service;

namespace TicketSellingModule.ViewModel
{
    public class EmployeeViewModel
    {
        private readonly EmployeeService _employeeService;
        private readonly FlightEmployeeService _flightEmployeeService;
        private readonly RouteRepo _routeRepo;
        private readonly GateRepo _gateRepo;
        private readonly RunwayRepo _runwayRepo;

        public ObservableCollection<Employee> EmployeesList { get; set; }
        public ObservableCollection<Flight> EmployeeFlightsList { get; set; }

        public EmployeeViewModel()
        {
            var connectionFactory = new DbConnectionFactory();

            _employeeService = new EmployeeService(new EmployeeRepo(connectionFactory));
            _flightEmployeeService = new FlightEmployeeService(
                new FlightEmployeeRepo(connectionFactory),
                new EmployeeRepo(connectionFactory),
                new FlightRepo(connectionFactory));

            _routeRepo = new RouteRepo(connectionFactory);
            _gateRepo = new GateRepo(connectionFactory);
            _runwayRepo = new RunwayRepo(connectionFactory);

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
            var flights = _flightEmployeeService
                .GetEmployeeSchedule(employeeId)
                .OrderBy(f => f.Date)
                .ToList();

            EmployeeFlightsList.Clear();
            foreach (var flight in flights)
            {
                EmployeeFlightsList.Add(flight);
            }

            return flights;
        }

        public Route? GetRouteInfo(int routeId)
        {
            if (routeId <= 0) return null;
            return _routeRepo.GetRouteById(routeId);
        }

        public Gate? GetGateInfo(int gateId)
        {
            if (gateId <= 0) return null;
            return _gateRepo.GetGateById(gateId);
        }

        public Runway? GetRunwayInfo(int runwayId)
        {
            if (runwayId <= 0) return null;
            return _runwayRepo.GetRunwayById(runwayId);
        }
    }
}