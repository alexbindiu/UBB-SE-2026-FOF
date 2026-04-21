using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TicketSellingModule.Domain;
using TicketSellingModule.Service;

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

        [ObservableProperty] private Runway? _selectedRunway;
        [ObservableProperty] private Gate? _selectedGate;
        [ObservableProperty] private Airport? _selectedAirport;
        [ObservableProperty] private Flight? _selectedFlight;

        [ObservableProperty] private string? _runwayName;
        [ObservableProperty] private int _runwayHandleTime;
        [ObservableProperty] private string? _gateName;
        [ObservableProperty] private string? _airportCode;
        [ObservableProperty] private string? _airportName;
        [ObservableProperty] private string? _airportCity;

        public AirportAdminViewModel(
            AirportService airportService,
            RunwayService runwayService,
            GateService gateService,
            EmployeeService employeeService,
            FlightRouteService flightRouteService,
            FlightEmployeeService flightEmployeeService)
        {
            _airportService = airportService;
            _runwayService = runwayService;
            _gateService = gateService;
            _employeeService = employeeService;
            _flightRouteService = flightRouteService;
            _flightEmployeeService = flightEmployeeService;
        }

        public void Initialize()
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
            AirportsList.Clear();
            foreach (var item in _airportService.GetAll()) AirportsList.Add(item);
        }

        [RelayCommand]
        public void RefreshFlights()
        {
            FlightsList.Clear();
            // Business logic moved to FlightRouteService.GetAllFlightsWithDetails()
            foreach (var f in _flightRouteService.GetAllFlightsWithDetails()) FlightsList.Add(f);
        }

        [RelayCommand]
        public void RefreshRunways()
        {
            RunwaysList.Clear();
            foreach (var item in _runwayService.GetAll()) RunwaysList.Add(item);
        }

        [RelayCommand]
        public void RefreshGates()
        {
            GatesList.Clear();
            foreach (var item in _gateService.GetAll()) GatesList.Add(item);
        }

        [RelayCommand]
        public void RefreshEmployees()
        {
            EmployeesList.Clear();
            foreach (var item in _employeeService.GetAll()) EmployeesList.Add(item);
        }

        public List<Employee> GetAllEmployees()
        {
            if (EmployeesList.Count == 0) RefreshEmployees();
            return EmployeesList.ToList();
        }

        public List<Flight> GetAllFlights()
        {
            if (FlightsList.Count == 0) RefreshFlights();
            return FlightsList.ToList();
        }

        // --- Runway ---

        [RelayCommand]
        public void AddRunway()
        {
            if (string.IsNullOrWhiteSpace(RunwayName)) return;
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
            if (SelectedRunway == null) return;
            UpdateRunway(SelectedRunway.Id, SelectedRunway.Name, SelectedRunway.HandleTime);
        }

        public void UpdateRunway(int runwayId, string runwayName, int handleTime)
        {
            _runwayService.Update(runwayId, runwayName, handleTime);
            var updated = _runwayService.GetById(runwayId);
            for (int i = 0; i < RunwaysList.Count; i++)
            {
                if (RunwaysList[i].Id == runwayId) { RunwaysList[i] = updated; return; }
            }
            RunwaysList.Add(updated);
        }

        [RelayCommand]
        public void DeleteRunway()
        {
            if (SelectedRunway == null) return;
            DeleteRunway(SelectedRunway.Id);
            SelectedRunway = null;
        }

        public void DeleteRunway(int runwayId)
        {
            _runwayService.Delete(runwayId);
            var existing = RunwaysList.FirstOrDefault(r => r.Id == runwayId);
            if (existing != null) RunwaysList.Remove(existing);
        }

        // --- Gate ---

        [RelayCommand]
        public void AddGate()
        {
            if (string.IsNullOrWhiteSpace(GateName)) return;
            int id = AddGate(GateName);
            SelectedGate = GatesList.FirstOrDefault(g => g.Id == id);
        }

        public int AddGate(string gateName)
        {
            int id = _gateService.Add(gateName);
            var gate = _gateService.GetById(id);
            GatesList.Add(gate);
            return id;
        }

        [RelayCommand]
        public void UpdateGate()
        {
            if (SelectedGate == null) return;
            UpdateGate(SelectedGate.Id, SelectedGate.Name);
        }

        public void UpdateGate(int gateId, string gateName)
        {
            _gateService.Update(gateId, gateName);
            var updated = _gateService.GetById(gateId);
            for (int i = 0; i < GatesList.Count; i++)
            {
                if (GatesList[i].Id == gateId) { GatesList[i] = updated; return; }
            }
            GatesList.Add(updated);
        }

        [RelayCommand]
        public void DeleteGate()
        {
            if (SelectedGate == null) return;
            DeleteGate(SelectedGate.Id);
            SelectedGate = null;
        }

        public void DeleteGate(int gateId)
        {
            _gateService.Delete(gateId);
            var existing = GatesList.FirstOrDefault(g => g.Id == gateId);
            if (existing != null) GatesList.Remove(existing);
        }

        // --- Airport ---

        [RelayCommand]
        public void AddAirport()
        {
            if (string.IsNullOrWhiteSpace(AirportCode) ||
                string.IsNullOrWhiteSpace(AirportName) ||
                string.IsNullOrWhiteSpace(AirportCity)) return;

            int id = AddAirport(AirportCode, AirportName, AirportCity);
            SelectedAirport = AirportsList.FirstOrDefault(a => a.Id == id);
        }

        public int AddAirport(string airportCode, string name, string city)
        {
            int id = _airportService.Add(airportCode, name, city);
            var airport = _airportService.GetById(id);
            if (airport != null) AirportsList.Add(airport);
            return id;
        }

        [RelayCommand]
        public void UpdateAirport()
        {
            if (SelectedAirport == null) return;
            UpdateAirport(SelectedAirport.Id, SelectedAirport.City, SelectedAirport.AirportName, SelectedAirport.AirportCode);
        }

        public void UpdateAirport(int airportId, string? newCity = null, string? newName = null, string? newCode = null)
        {
            _airportService.Update(airportId, newCity, newName, newCode);
            var updated = _airportService.GetById(airportId);
            if (updated == null) return;
            for (int i = 0; i < AirportsList.Count; i++)
            {
                if (AirportsList[i].Id == airportId) { AirportsList[i] = updated; return; }
            }
            AirportsList.Add(updated);
        }

        [RelayCommand]
        public void DeleteAirport()
        {
            if (SelectedAirport == null) return;
            DeleteAirport(SelectedAirport.Id);
            SelectedAirport = null;
        }

        public void DeleteAirport(int airportId)
        {
            _airportService.Delete(airportId);
            var existing = AirportsList.FirstOrDefault(a => a.Id == airportId);
            if (existing != null) AirportsList.Remove(existing);
        }

        // --- Employee ---

        public int AddEmployee(string name, string position, DateOnly birthday, int salary, DateOnly hiringDate)
        {
            int id = _employeeService.Add(name, position, birthday, salary, hiringDate);
            var employee = _employeeService.GetById(id);
            if (employee != null) EmployeesList.Add(employee);
            return id;
        }

        public void UpdateEmployee(int employeeId, string? name = null, string? position = null, int? salary = null)
        {
            _employeeService.Update(employeeId, name, position, salary);
            var updated = _employeeService.GetById(employeeId);
            if (updated == null) return;
            for (int i = 0; i < EmployeesList.Count; i++)
            {
                if (EmployeesList[i].Id == employeeId) { EmployeesList[i] = updated; return; }
            }
            EmployeesList.Add(updated);
        }

        public void DeleteEmployee(int employeeId)
        {
            _employeeService.Delete(employeeId);
            var existing = EmployeesList.FirstOrDefault(e => e.Id == employeeId);
            if (existing != null) EmployeesList.Remove(existing);
        }

        // --- Flight/Crew ---

        public Flight? GetFlightById(int flightId) =>
            _flightRouteService.GetFlightById(flightId);

        public List<Employee> GetFlightCrew(int flightId) =>
            _flightEmployeeService.GetFlightCrew(flightId);

        // Business logic moved to FlightEmployeeService.GetAvailableEmployeesForFlight()
        public List<Employee> GetAvailableEmployeesForFlight(Flight targetFlight) =>
            _flightEmployeeService.GetAvailableEmployeesForFlight(targetFlight);

        // Business logic moved to FlightEmployeeService.UpdateCrewForFlight()
        public void AssignCrewToFlight(int flightId, List<int> employeeIds) =>
            _flightEmployeeService.AssignCrewToFlight(flightId, employeeIds);

        public void UpdateCrewForFlight(int flightId, List<int> newEmployeeIds) =>
            _flightEmployeeService.UpdateCrewForFlight(flightId, newEmployeeIds);
    }
}
