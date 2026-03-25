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
    internal class AirportAdminViewModel
    {
        private readonly AirportService _airportService;
        private readonly RunwayService _runwayService;
        private readonly GateService _gateService;
        private readonly EmployeeService _employeeService;
        private readonly FlightRouteService _flightRouteService;

        public ObservableCollection<Runway> RunwaysList { get; set; }
        public ObservableCollection<Gate> GatesList { get; set; }
        public ObservableCollection<Airport> AirportsList { get; set; }
        public ObservableCollection<Employee> EmployeesList { get; set; }
        public ObservableCollection<Flight> FlightsList { get; set; }

        public AirportAdminViewModel()
        {
            // Create the dependency needed by your repositories
            var connectionFactory = new DbConnectionFactory(); 

            // Pass the connectionFactory to each repository
            _airportService = new AirportService(new AirportRepo(connectionFactory));
            _runwayService = new RunwayService(new RunwayRepo(connectionFactory));
            _gateService = new GateService(new GateRepo(connectionFactory));
            _employeeService = new EmployeeService(new EmployeeRepo(connectionFactory));
            _flightRouteService = new FlightRouteService(new FlightRepo(connectionFactory), new RouteRepo(connectionFactory), new CompanyRepo(connectionFactory), new AirportRepo(connectionFactory));

            RunwaysList = new ObservableCollection<Runway>();
            GatesList = new ObservableCollection<Gate>();
            AirportsList = new ObservableCollection<Airport>();
            EmployeesList = new ObservableCollection<Employee>();
            FlightsList = new ObservableCollection<Flight>();
        }

        public List<Runway> GetAllRunways()
        {
            return _runwayService.GetAll();
        }

        public List<Gate> GetAllGates()
        {
            return _gateService.GetAll();
        }

        public List<Airport> GetAllAirports()
        {
            return _airportService.GetAll();
        }

        public List<Employee> GetAllEmployees()
        {
            return _employeeService.GetAll();
        }

        public List<Flight> GetAllFlights()
        {
            return _flightRouteService.GetAllFlights();
        }

        public int AddRunway(string runwayName, int handleTime)
        {
            return _runwayService.Add(runwayName, handleTime);
        }

        public void UpdateRunway(int runwayId, string runwayName, int handleTime)
        {
            _runwayService.Update(runwayId, runwayName, handleTime);
        }

        public void DeleteRunway(int runwayId)
        {
            _runwayService.Delete(runwayId);
        }

        public int AddGate(string gateName)
        {
            return _gateService.Add(gateName);
        }

        public void UpdateGate(int gateId, string gateName)
        {
            _gateService.Update(gateId, gateName);
        }

        public void DeleteGate(int gateId) 
        {
            _gateService.Delete(gateId);
        }

        public int AddAirport(string airportCode, string name, string city)
        {
            return _airportService.Add(airportCode, name, city);
        }

        public void UpdateAirport(int airportId, string? newCity = null, string? newName = null, string? newCode = null)
        {
            _airportService.Update(airportId, newCity, newName, newCode);
        }
        public void DeleteAirport(int airportId)
        {
            _airportService.Delete(airportId);
        }

        public int AddEmployee(string name, string position, DateOnly birthday, int salary, DateOnly hiringDate)
        {
            return _employeeService.Add(name, position, birthday, salary, hiringDate);
        }

        public void UpdateEmployee(int employeeId, string? name = null, string? position = null, int? salary = null)
        {
            _employeeService.Update(employeeId, name, position, salary);
        }

        public void DeleteEmployee(int employeeId)
        {
            _employeeService.Delete(employeeId);
        }

        public Flight? GetFlightById(int flightId)
        {
            return _flightRouteService.GetFlightById(flightId);
        }
    }
}
