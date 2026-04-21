using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using TicketSellingModule.Domain;
using TicketSellingModule.Repo;
using TicketSellingModule.Service;
using CommunityToolkit.Mvvm.Input;

namespace TicketSellingModule.ViewModel
{
    public partial class AirportAdminViewModel : ObservableObject
    {
        private readonly AirportService _airportService;
        private readonly RunwayService _runwayService;
        private readonly GateService _gateService;
        private readonly EmployeeService _employeeService;
        private readonly FlightRouteService _flightRouteService;
        private readonly FlightEmployeeService _flightEmployeeService;

       
        public ObservableCollection<Flight> FlightsList { get; } = new();


        [ObservableProperty]
        private Flight? _selectedFlight;

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


            LoadInitialData();
        }

        private void LoadInitialData()
        {

            RefreshFlights();
        }

        

        [RelayCommand]
        public void RefreshFlights()
        {
            var flights = LoadFlightsWithDetails();
            FlightsList.Clear();
            foreach (var f in flights) FlightsList.Add(f);
        }

        

        

        public List<Flight> GetAllFlights()
        {
            if (FlightsList.Count == 0)
            {
                RefreshFlights();
            }

            return FlightsList.ToList();
        }

        private List<Flight> LoadFlightsWithDetails()
        {
            var flights = _flightRouteService.GetAllFlights();

            foreach (var flight in flights)
            {
                if (flight.RunwayId > 0) flight.Runway = _runwayService.GetById(flight.RunwayId);
                if (flight.GateId > 0) flight.Gate = _gateService.GetById(flight.GateId);
                if (flight.RouteId > 0)
                {
                    flight.Route = _flightRouteService.GetRouteById(flight.RouteId);
                    if (flight.Route?.AirportId > 0)
                        flight.Route.Airport = _airportService.GetById(flight.Route.AirportId);
                }
            }

            return flights;
        }


        

       
        

        public Flight? GetFlightById(int flightId)
        {
            return _flightRouteService.GetFlightById(flightId);
        }

        public List<Employee> GetFlightCrew(int flightId)
        {
            return _flightEmployeeService.GetFlightCrew(flightId);
        }

        public List<Employee> GetAvailableEmployeesForFlight(Flight targetFlight) //move to Service
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
