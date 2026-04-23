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

            return available;
        }

        public void AssignCrewToFlight(int flightId, List<int> employeeIds)
        {
            foreach (var empId in employeeIds)
            {
                try
                {
                    AssignCrewMember(flightId, empId);
                }
                catch
                {
                    // intent: ignore if already assigned to avoid crashing the bulk operation
                }
            }
        }

        public void UpdateCrewForFlight(int flightId, List<int> newEmployeeIds)
        {
            List<Employee> currentCrew = GetFlightCrew(flightId);
            List<int> currentCrewIds = new List<int>();

            foreach (Employee crewMember in currentCrew)
            {
                currentCrewIds.Add(crewMember.Id);
            }

            foreach (int existingId in currentCrewIds)
            {
                if (!currentCrewIds.Contains(existingId))
                {
                    RemoveCrewMember(flightId, existingId);
                }
            }

            foreach (int newId in currentCrewIds)
            {
                if (!currentCrewIds.Contains(newId))
                {
                    AssignCrewMember(flightId, newId);
                }
            }
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

        public List<EmployeeScheduleItem> GetFormattedEmployeeSchedule(int employeeId)
        {
            List<EmployeeScheduleItem> scheduleItems = new List<EmployeeScheduleItem>();

            if (employeeId <= 0)
            {
                return scheduleItems;
            }

            var flights = GetEmployeeSchedule(employeeId);
            flights.Sort(new FlightDateComparer());

            foreach (var flight in flights)
            {
                Route? route = routeRepo.GetRouteById(flight.Route.Id);
                Gate? gate = flight.Gate?.Id > 0 ? gateService.GetById(flight.Gate.Id) : null;
                Runway? runway = runwayService.GetByIdSafe(flight.Runway?.Id ?? 0);

                scheduleItems.Add(new EmployeeScheduleItem
                {
                    Id = flight.Id.ToString(),
                    FlightNumber = flight.FlightNumber,
                    FlightType = routeService.NormalizeFlightType(route?.RouteType),
                    Date = flight.Date.ToString("dd MMM yyyy"),
                    GateName = gate?.Name ?? "-",
                    RunwayName = runway?.Name ?? "-",
                    FlightTime = routeService.GetRelevantTime(route)
                });
            }

            return scheduleItems;
        }

        private class FlightDateComparer : IComparer<Flight>
        {
            public int Compare(Flight? firstFlight, Flight? secondFlight)
            {
                if (firstFlight == null || secondFlight == null)
                {
                    return 0;
                }

                return firstFlight.Date.CompareTo(secondFlight.Date);
            }
        }

        public List<Employee> GetAvailableCrewGroupedByRole(Flight flight)
        {
            List<Employee> availableEmployees = GetAvailableEmployeesForFlight(flight);
            List<Employee> validEmployees = new List<Employee>();

            foreach (Employee employee in availableEmployees)
            {
                if (employee.Role != EmployeeRole.Other)
                {
                    validEmployees.Add(employee);
                }
            }

            validEmployees.Sort(new EmployeeRoleAndNameComparer());

            return validEmployees;
        }

        private class EmployeeRoleAndNameComparer : IComparer<Employee>
        {
            public int Compare(Employee? firstEmployee, Employee? secondEmployee)
            {
                if (firstEmployee == null || secondEmployee == null)
                {
                    return 0;
                }

                int roleComparisonResult = firstEmployee.Role.CompareTo(secondEmployee.Role);

                if (roleComparisonResult != 0)
                {
                    return roleComparisonResult;
                }

                return string.Compare(firstEmployee.Name, secondEmployee.Name, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}