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

        public ObservableCollection<Runway> RunwaysList { get; } = new();
        public ObservableCollection<Gate> GatesList { get; } = new();
        public ObservableCollection<Airport> AirportsList { get; } = new();
        public ObservableCollection<Employee> EmployeesList { get; } = new();
        public ObservableCollection<Flight> FlightsList { get; } = new();

        [ObservableProperty]
        private Runway? _selectedRunway;

        [ObservableProperty]
        private Gate? _selectedGate;

        [ObservableProperty]
        private Airport? _selectedAirport;

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
            RefreshAirports();
            RefreshRunways();
            RefreshGates();
            RefreshEmployees();
            RefreshFlights();
        }

        [RelayCommand]
        public void RefreshAirports()
        {
            var data = _airportService.GetAll();
            AirportsList.Clear(); 
            foreach (var item in data) AirportsList.Add(item);
        }

        [RelayCommand]
        public void RefreshFlights()
        {
            var flights = LoadFlightsWithDetails();
            FlightsList.Clear();
            foreach (var f in flights) FlightsList.Add(f);
        }

        [RelayCommand]
        public void RefreshRunways()
        {
            var data = _runwayService.GetAll();
            RunwaysList.Clear();
            foreach (var item in data) RunwaysList.Add(item);
        }

        [RelayCommand]
        public void RefreshGates()
        {
            var data = _gateService.GetAll();
            GatesList.Clear();
            foreach (var item in data) GatesList.Add(item);
        }

        [RelayCommand]
        public void RefreshEmployees()
        {
            var data = _employeeService.GetAll();
            EmployeesList.Clear();
            foreach (var item in data) EmployeesList.Add(item);
        }

        public List<Employee> GetAllEmployees()
        {
            return _employeeService.GetAll();
        }

        public List<Flight> GetAllFlights()
        {
            return LoadFlightsWithDetails();
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

        [ObservableProperty]
        private string? _runwayName;

        [ObservableProperty]
        private int _runwayHandleTime;

        [ObservableProperty]
        private string? _gateName;

        [ObservableProperty]
        private string? _airportCode;

        [ObservableProperty]
        private string? _airportName;

        [ObservableProperty]
        private string? _airportCity;

        [RelayCommand]
        public void AddRunway()
        {
            if (string.IsNullOrWhiteSpace(RunwayName))
            {
                return;
            }

            int id = AddRunway(RunwayName, RunwayHandleTime);
            SelectedRunway = RunwaysList.FirstOrDefault(r => r.Id == id);
        }

        public int AddRunway(string runwayName, int handleTime)
        {
            int id = _runwayService.Add(runwayName, handleTime);
            var runway = _runwayService.GetById(id);
            RunwaysList.Add(runway);
            return id;
        }

        [RelayCommand]
        public void UpdateRunway()
        {
            if (SelectedRunway == null)
            {
                return;
            }

            UpdateRunway(SelectedRunway.Id, SelectedRunway.Name, SelectedRunway.HandleTime);
        }

        public void UpdateRunway(int runwayId, string runwayName, int handleTime)
        {
            _runwayService.Update(runwayId, runwayName, handleTime);
            var updated = _runwayService.GetById(runwayId);
            for (int i = 0; i < RunwaysList.Count; i++)
            {
                if (RunwaysList[i].Id == runwayId)
                {
                    RunwaysList[i] = updated;
                    return;
                }
            }

            RunwaysList.Add(updated);
        }

        [RelayCommand]
        public void DeleteRunway()
        {
            if (SelectedRunway == null)
            {
                return;
            }

            DeleteRunway(SelectedRunway.Id);
            SelectedRunway = null;
        }

        public void DeleteRunway(int runwayId)
        {
            _runwayService.Delete(runwayId);
            var existing = RunwaysList.FirstOrDefault(r => r.Id == runwayId);
            if (existing != null)
            {
                RunwaysList.Remove(existing);
            }
        }

        [RelayCommand]
        public void AddGate()
        {
            if (string.IsNullOrWhiteSpace(GateName))
            {
                return;
            }

            int id = AddGate(GateName);
            SelectedGate = GatesList.FirstOrDefault(g => g.Id == id);
        }

        public int AddGate(string gateName)
        {
            int id = _gateService.Add(gateName);
            var gate = _gateService.GetById(id);
            if (gate != null)
            {
                GatesList.Add(gate);
            }

            return id;
        }

        [RelayCommand]
        public void UpdateGate()
        {
            if (SelectedGate == null)
            {
                return;
            }

            UpdateGate(SelectedGate.Id, SelectedGate.Name);
        }

        public void UpdateGate(int gateId, string gateName)
        {
            _gateService.Update(gateId, gateName);
            var updated = _gateService.GetById(gateId);
            if (updated == null)
            {
                return;
            }

            for (int i = 0; i < GatesList.Count; i++)
            {
                if (GatesList[i].Id == gateId)
                {
                    GatesList[i] = updated;
                    return;
                }
            }

            GatesList.Add(updated);
        }

        [RelayCommand]
        public void DeleteGate()
        {
            if (SelectedGate == null)
            {
                return;
            }

            DeleteGate(SelectedGate.Id);
            SelectedGate = null;
        }

        public void DeleteGate(int gateId) 
        {
            _gateService.Delete(gateId);
            var existing = GatesList.FirstOrDefault(g => g.Id == gateId);
            if (existing != null)
            {
                GatesList.Remove(existing);
            }
        }

        [RelayCommand]
        public void AddAirport()
        {
            if (string.IsNullOrWhiteSpace(AirportCode) ||
                string.IsNullOrWhiteSpace(AirportName) ||
                string.IsNullOrWhiteSpace(AirportCity))
            {
                return;
            }

            int id = AddAirport(AirportCode, AirportName, AirportCity);
            SelectedAirport = AirportsList.FirstOrDefault(a => a.Id == id);
        }

        public int AddAirport(string airportCode, string name, string city)
        {
            int id = _airportService.Add(airportCode, name, city);
            var airport = _airportService.GetById(id);
            if (airport != null)
            {
                AirportsList.Add(airport);
            }

            return id;
        }

        [RelayCommand]
        public void UpdateAirport()
        {
            if (SelectedAirport == null)
            {
                return;
            }

            UpdateAirport(SelectedAirport.Id, SelectedAirport.City, SelectedAirport.AirportName, SelectedAirport.AirportCode);
        }

        public void UpdateAirport(int airportId, string? newCity = null, string? newName = null, string? newCode = null)
        {
            _airportService.Update(airportId, newCity, newName, newCode);
            var updated = _airportService.GetById(airportId);
            if (updated == null)
            {
                return;
            }

            for (int i = 0; i < AirportsList.Count; i++)
            {
                if (AirportsList[i].Id == airportId)
                {
                    AirportsList[i] = updated;
                    return;
                }
            }

            AirportsList.Add(updated);
        }

        [RelayCommand]
        public void DeleteAirport()
        {
            if (SelectedAirport == null)
            {
                return;
            }

            DeleteAirport(SelectedAirport.Id);
            SelectedAirport = null;
        }

        public void DeleteAirport(int airportId)
        {
            _airportService.Delete(airportId);
            var existing = AirportsList.FirstOrDefault(a => a.Id == airportId);
            if (existing != null)
            {
                AirportsList.Remove(existing);
            }
        }

        public int AddEmployee(string name, string position, DateOnly birthday, int salary, DateOnly hiringDate)
        {
            int id = _employeeService.Add(name, position, birthday, salary, hiringDate);
            var employee = _employeeService.GetById(id);
            if (employee != null)
            {
                EmployeesList.Add(employee);
            }

            return id;
        }

        public void UpdateEmployee(int employeeId, string? name = null, string? position = null, int? salary = null)
        {
            _employeeService.Update(employeeId, name, position, salary);
            var updated = _employeeService.GetById(employeeId);
            if (updated == null)
            {
                return;
            }

            for (int i = 0; i < EmployeesList.Count; i++)
            {
                if (EmployeesList[i].Id == employeeId)
                {
                    EmployeesList[i] = updated;
                    return;
                }
            }

            EmployeesList.Add(updated);
        }

        public void DeleteEmployee(int employeeId)
        {
            _employeeService.Delete(employeeId);
            var existing = EmployeesList.FirstOrDefault(e => e.Id == employeeId);
            if (existing != null)
            {
                EmployeesList.Remove(existing);
            }
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
