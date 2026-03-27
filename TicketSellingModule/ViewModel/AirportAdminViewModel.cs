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
    public class AirportAdminViewModel
    {
        private readonly AirportService _airportService;
        private readonly RunwayService _runwayService;
        private readonly GateService _gateService;
        private readonly EmployeeService _employeeService;
        private readonly FlightRouteService _flightRouteService;
        private readonly FlightEmployeeService _flightEmployeeService;

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
            _flightRouteService = new FlightRouteService(new FlightRepo(connectionFactory), 
                new RouteRepo(connectionFactory), 
                new CompanyRepo(connectionFactory), 
                new AirportRepo(connectionFactory)
                );
            _flightEmployeeService = new FlightEmployeeService(new FlightEmployeeRepo(connectionFactory), 
                new EmployeeRepo(connectionFactory), 
                new FlightRepo(connectionFactory)
                );


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
            // 1. Ask the Service for the raw Flights (which only have the integer IDs right now)
            List<Flight> rawFlights = _flightRouteService.GetAllFlights();

            // 2. Orchestrate! Loop through the flights and use your other services to fill in the missing objects
            foreach (var flight in rawFlights)
            {
                // Fill the Runway box
                if (flight.RunwayId > 0)
                {
                    flight.Runway = _runwayService.GetById(flight.RunwayId);
                }

                // Fill the Gate box
                if (flight.GateId > 0)
                {
                    flight.Gate = _gateService.GetById(flight.GateId);
                }

                // Fill the Route and Airport boxes
                if (flight.RouteId > 0)
                {
                    flight.Route = _flightRouteService.GetRouteById(flight.RouteId);

                    // The UI needs the Airport to display the destination, so we fetch it here!
                    if (flight.Route != null && flight.Route.AirportId > 0)
                    {
                        flight.Route.Airport = _airportService.GetById(flight.Route.AirportId);
                    }
                }
            }

            // 3. Return the fully assembled flights to the UI
            return rawFlights;
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

        public List<Employee> GetFlightCrew(int flightId)
        {
            return _flightEmployeeService.GetFlightCrew(flightId);
        }

        public List<Employee> GetAvailableEmployeesForFlight(Flight targetFlight)
        {
            var allEmployees = _employeeService.GetAll();
            var available = new List<Employee>();

            // 1. Fetch the full Route details for the target flight so we know its times
            var targetRoute = _flightRouteService.GetRouteById(targetFlight.RouteId);

            if (targetRoute == null) return available; // Safety check

            foreach (var emp in allEmployees)
            {
                var schedule = _flightEmployeeService.GetEmployeeSchedule(emp.Id);

                // 2. Grab only the flights this employee is already working on that EXACT calendar day
                var sameDayFlights = schedule.Where(f => f.Date.Date == targetFlight.Date.Date && f.Id != targetFlight.Id).ToList();

                bool isDoubleBooked = false;

                foreach (var scheduledFlight in sameDayFlights)
                {
                    // 3. Fetch the Route details for the flight they are already working
                    var scheduledRoute = _flightRouteService.GetRouteById(scheduledFlight.RouteId);

                    if (scheduledRoute != null)
                    {
                        // 4. The Mathematical Overlap Check
                        // (Target Starts before Scheduled Ends) AND (Target Ends after Scheduled Starts)
                        bool isTimeOverlap = targetRoute.DepartureTime < scheduledRoute.ArrivalTime &&
                                             targetRoute.ArrivalTime > scheduledRoute.DepartureTime;

                        if (isTimeOverlap)
                        {
                            isDoubleBooked = true;
                            break; // We found a conflict! Stop checking their other flights.
                        }
                    }
                }

                // 5. If they survived the overlap check without conflicts, add them to the UI!
                if (!isDoubleBooked)
                {
                    available.Add(emp);
                }
            }

            return available;
        }

        public void AssignCrewToFlight(int flightId, List<int> employeeIds)
        {
            foreach (var empId in employeeIds)
            {
                try
                {
                    _flightEmployeeService.AssignCrewMember(flightId, empId);
                }
                catch
                {
                    // If they are already assigned, just ignore the error and continue
                }
            }
        }

        public void UpdateCrewForFlight(int flightId, List<int> newEmployeeIds)
        {
            // 1. Get the IDs of the people CURRENTLY assigned to the flight
            var currentCrewIds = _flightEmployeeService.GetFlightCrew(flightId).Select(e => e.Id).ToList();

            // 2. Find who was UNCHECKED (They are in current, but not in new)
            var employeesToRemove = currentCrewIds.Except(newEmployeeIds).ToList();

            // 3. Find who was CHECKED (They are in new, but not in current)
            var employeesToAdd = newEmployeeIds.Except(currentCrewIds).ToList();

            // 4. Apply the changes!
            foreach (var empId in employeesToRemove)
            {
                _flightEmployeeService.RemoveCrewMember(flightId, empId);
            }

            foreach (var empId in employeesToAdd)
            {
                _flightEmployeeService.AssignCrewMember(flightId, empId);
            }
        }
    }
}
