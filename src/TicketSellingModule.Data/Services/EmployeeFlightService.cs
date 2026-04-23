using System;
using System.Collections.Generic;
using System.Linq;

using TicketSellingModule.Data.Domain;
using TicketSellingModule.Data.Repositories;

namespace TicketSellingModule.Service
{
    public class EmployeeFlightService
    {
        private readonly EmployeeFlightRepo linkRepo;
        private readonly EmployeeRepo employeeRepo;
        private readonly FlightRepo flightRepo;
        private readonly RouteRepo routeRepo;

        public EmployeeFlightService(
            EmployeeFlightRepo linkRepo,
            EmployeeRepo employeeRepo,
            FlightRepo flightRepo,
            RouteRepo routeRepo)
        {
            this.linkRepo = linkRepo ?? throw new ArgumentNullException(nameof(linkRepo));
            this.employeeRepo = employeeRepo ?? throw new ArgumentNullException(nameof(employeeRepo));
            this.flightRepo = flightRepo ?? throw new ArgumentNullException(nameof(flightRepo));
            this.routeRepo = routeRepo ?? throw new ArgumentNullException(nameof(routeRepo));
        }

        public void AssignCrewMember(int flightId, int employeeId)
        {
            if (flightId <= 0 || employeeId <= 0)
            {
                throw new ArgumentException("Invalid flight or employee ID.");
            }

            var emp = employeeRepo.GetEmployeeById(employeeId);
            var flight = flightRepo.GetById(flightId);

            if (emp == null || flight == null)
            {
                throw new InvalidOperationException("Employee or Flight does not exist.");
            }

            var currentCrewIds = linkRepo.GetEmployeesByFlight(flightId);
            if (currentCrewIds.Contains(employeeId))
            {
                throw new InvalidOperationException("Employee already assigned.");
            }

            if (!IsEmployeeAvailable(employeeId, flight.Date, flight.Route.Id, flight.Id))
            {
                throw new InvalidOperationException($"Conflict: Employee is already assigned to another flight during this time.");
            }

            linkRepo.AssignFlightToEmployee(employeeId, flightId);
        }

        public void RemoveCrewMember(int flightId, int employeeId)
        {
            linkRepo.RemoveFlightFromEmployee(employeeId, flightId);
        }

        public void CleanUpFlightAssignments(int flightId)
        {
            if (flightId > 0)
            {
                linkRepo.RemoveAllByFlightId(flightId);
            }
        }

        public void CleanUpEmployeeAssignments(int employeeId)
        {
            if (employeeId > 0)
            {
                linkRepo.RemoveAllByEmployeeId(employeeId);
            }
        }

        public List<Employee> GetFlightCrew(int flightId)
        {
            var crew = new List<Employee>();
            foreach (var id in linkRepo.GetEmployeesByFlight(flightId))
            {
                var emp = employeeRepo.GetEmployeeById(id);
                if (emp != null)
                {
                    crew.Add(emp);
                }
            }
            return crew;
        }

        public List<Flight> GetEmployeeSchedule(int employeeId)
        {
            if (employeeId <= 0)
            {
                return new List<Flight>();
            }

            var flightIds = linkRepo.GetFlightsByEmployee(employeeId);
            var flights = new List<Flight>();

            foreach (var id in flightIds)
            {
                var flight = flightRepo.GetById(id);
                if (flight != null)
                {
                    flights.Add(flight);
                }
            }
            return flights;
        }

        public bool IsEmployeeAvailable(int employeeId, DateTime targetDate, int targetRouteId, int? excludeFlightId = null)
        {
            var targetRoute = routeRepo.GetRouteById(targetRouteId);
            if (targetRoute == null)
            {
                return false;
            }

            var schedule = GetEmployeeSchedule(employeeId);
            var sameDayFlights = schedule.Where(f => f.Date.Date == targetDate.Date && f.Id != excludeFlightId);

            foreach (var flight in sameDayFlights)
            {
                var scheduledRoute = routeRepo.GetRouteById(flight.Route.Id);
                if (scheduledRoute == null)
                {
                    continue;
                }

                bool overlap = targetRoute.DepartureTime < scheduledRoute.ArrivalTime &&
                               targetRoute.ArrivalTime > scheduledRoute.DepartureTime;

                if (overlap)
                {
                    return false;
                }
            }

            return true;
        }

        public void UpdateCrewForFlight(int flightId, List<int> newEmployeeIds)
        {
            var currentCrewIds = linkRepo.GetEmployeesByFlight(flightId);

            foreach (var empId in currentCrewIds.Except(newEmployeeIds).ToList())
            {
                RemoveCrewMember(flightId, empId);
            }

            foreach (var empId in newEmployeeIds.Except(currentCrewIds).ToList())
            {
                AssignCrewMember(flightId, empId);
            }
        }
    }
}