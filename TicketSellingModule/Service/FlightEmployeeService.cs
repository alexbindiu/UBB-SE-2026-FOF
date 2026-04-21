using System.Collections.Generic;
using System.Linq;
using System;

using TicketSellingModule.Domain;
using TicketSellingModule.Repo;

namespace TicketSellingModule.Service
{
    public class FlightEmployeeService
    {
        private readonly FlightEmployeeRepo _linkRepo;
        private readonly EmployeeRepo _employeeRepo;
        private readonly FlightRepo _flightRepo;
        private readonly EmployeeService _employeeService;
        private readonly FlightRouteService _flightRouteService;

        public FlightEmployeeService(
            FlightEmployeeRepo linkRepo,
            EmployeeRepo employeeRepo,
            FlightRepo flightRepo,
            EmployeeService employeeService,
            FlightRouteService flightRouteService)
        {
            _linkRepo = linkRepo;
            _employeeRepo = employeeRepo;
            _flightRepo = flightRepo;
            _employeeService = employeeService;
            _flightRouteService = flightRouteService;
        }

        public void AssignCrewMember(int flightId, int employeeId)
        {
            if (flightId <= 0 || employeeId <= 0) return;
            var emp = _employeeRepo.GetEmployeeById(employeeId);
            if (emp == null)
                throw new InvalidOperationException("The employee does not exist in database.");
            var currentCrew = _linkRepo.GetEmployeesByFlight(flightId);
            if (currentCrew.Contains(employeeId))
                throw new InvalidOperationException("Employee already assigned to this flight.");
            _linkRepo.AssignFlightToEmployee(employeeId, flightId);
        }

        public void RemoveCrewMember(int flightId, int employeeId)
        {
            _linkRepo.RemoveFlightFromEmployee(employeeId, flightId);
        }

        public List<Employee> GetFlightCrew(int flightId)
        {
            var crew = new List<Employee>();
            foreach (var id in _linkRepo.GetEmployeesByFlight(flightId))
            {
                var emp = _employeeRepo.GetEmployeeById(id);
                if (emp != null) crew.Add(emp);
            }
            return crew;
        }

        public List<Flight> GetEmployeeSchedule(int employeeId)
        {
            if (employeeId <= 0) return new List<Flight>();
            var flights = new List<Flight>();
            foreach (var id in _linkRepo.GetFlightsByEmployee(employeeId))
            {
                var flight = _flightRepo.GetById(id);
                if (flight != null) flights.Add(flight);
            }
            return flights;
        }

        public List<Employee> GetAvailableEmployeesForFlight(Flight targetFlight)
        {
            var available = new List<Employee>();
            var targetRoute = _flightRouteService.GetRouteById(targetFlight.RouteId);
            if (targetRoute == null) return available;

            foreach (var emp in _employeeService.GetAll())
            {
                var sameDayFlights = GetEmployeeSchedule(emp.Id)
                    .Where(f => f.Date.Date == targetFlight.Date.Date && f.Id != targetFlight.Id)
                    .ToList();

                bool isDoubleBooked = false;
                foreach (var scheduledFlight in sameDayFlights)
                {
                    var scheduledRoute = _flightRouteService.GetRouteById(scheduledFlight.RouteId);
                    if (scheduledRoute != null)
                    {
                        bool overlap = targetRoute.DepartureTime < scheduledRoute.ArrivalTime &&
                                       targetRoute.ArrivalTime > scheduledRoute.DepartureTime;
                        if (overlap) { isDoubleBooked = true; break; }
                    }
                }

                if (!isDoubleBooked) available.Add(emp);
            }

            return available;
        }

        public void AssignCrewToFlight(int flightId, List<int> employeeIds)
        {
            foreach (var empId in employeeIds)
            {
                try { AssignCrewMember(flightId, empId); }
                catch { /* already assigned, ignore */ }
            }
        }

        public void UpdateCrewForFlight(int flightId, List<int> newEmployeeIds)
        {
            var currentCrewIds = GetFlightCrew(flightId).Select(e => e.Id).ToList();
            foreach (var empId in currentCrewIds.Except(newEmployeeIds).ToList())
                RemoveCrewMember(flightId, empId);
            foreach (var empId in newEmployeeIds.Except(currentCrewIds).ToList())
                AssignCrewMember(flightId, empId);
        }
    }
}