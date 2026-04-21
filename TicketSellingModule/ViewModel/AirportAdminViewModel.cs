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

       
        public ObservableCollection<Flight> FlightsList { get; } = new();


        [ObservableProperty]
        private Flight? _selectedFlight;

        public AirportAdminViewModel()
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

            RefreshFlights();
        }

        

        [RelayCommand]
        public void RefreshFlights()
        {
            FlightsList.Clear();
            // Business logic moved to FlightRouteService.GetAllFlightsWithDetails()
            foreach (var f in _flightRouteService.GetAllFlightsWithDetails()) FlightsList.Add(f);
        }

        

        

        public List<Flight> GetAllFlights()
        {
            if (FlightsList.Count == 0) RefreshFlights();
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
