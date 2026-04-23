using System;
using System.Collections.Generic;
using System.Linq;

using TicketSellingModule.Data;
using TicketSellingModule.Data.Domain;
using TicketSellingModule.Data.Repositories;
using TicketSellingModule.Data.Services; // Ensure this is included for helper services

namespace TicketSellingModule.Service
{
    public class EmployeeFlightService
    {
        private readonly EmployeeFlightRepo linkRepo;
        private readonly EmployeeRepo employeeRepo;
        private readonly FlightRepo flightRepo;
        private readonly RouteRepo routeRepo;
        private readonly GateService gateService;
        private readonly RunwayService runwayService;
        private readonly RouteService routeService;

        public EmployeeFlightService(
            EmployeeFlightRepo linkRepo,
            EmployeeRepo employeeRepo,
            FlightRepo flightRepo,
            RouteRepo routeRepo,
            GateService gateService,
            RunwayService runwayService,
            RouteService routeService)
        {
            this.linkRepo = linkRepo ?? throw new ArgumentNullException(nameof(linkRepo));
            this.employeeRepo = employeeRepo ?? throw new ArgumentNullException(nameof(employeeRepo));
            this.flightRepo = flightRepo ?? throw new ArgumentNullException(nameof(flightRepo));
            this.routeRepo = routeRepo ?? throw new ArgumentNullException(nameof(routeRepo));
            this.gateService = gateService ?? throw new ArgumentNullException(nameof(gateService));
            this.runwayService = runwayService ?? throw new ArgumentNullException(nameof(runwayService));
            this.routeService = routeService ?? throw new ArgumentNullException(nameof(routeService));
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
                throw new InvalidOperationException("Employee already assigned to this flight.");
            }

            if (!IsEmployeeAvailable(employeeId, flight.Date, flight.Route.Id, flight.Id))
            {
                throw new InvalidOperationException($"Conflict: Employee {emp.Name} is already assigned to another flight during this time.");
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
            if (targetRoute == null) return false;

            var schedule = GetEmployeeSchedule(employeeId);
            var sameDayFlights = schedule.Where(f => f.Date.Date == targetDate.Date && f.Id != excludeFlightId);

            foreach (var flight in sameDayFlights)
            {
                var scheduledRoute = routeRepo.GetRouteById(flight.Route.Id);
                if (scheduledRoute == null) continue;

                bool overlap = targetRoute.DepartureTime < scheduledRoute.ArrivalTime &&
                               targetRoute.ArrivalTime > scheduledRoute.DepartureTime;

                if (overlap) return false;
            }

            return true;
        }

        public void AssignCrewToFlight(int flightId, List<int> employeeIds)
        {
            foreach (var empId in employeeIds)
            {
                try { AssignCrewMember(flightId, empId); }
                catch { /* Intent: Ignore if already assigned to avoid crashing bulk ops */ }
            }
        }

        public void UpdateCrewForFlight(int flightId, List<int> newEmployeeIds)
        {
            var currentCrewIds = linkRepo.GetEmployeesByFlight(flightId);

            // Remove members who are NOT in the new list
            foreach (var empId in currentCrewIds.Except(newEmployeeIds).ToList())
            {
                RemoveCrewMember(flightId, empId);
            }

            // Add members who are NOT in the current list
            foreach (var empId in newEmployeeIds.Except(currentCrewIds).ToList())
            {
                AssignCrewMember(flightId, empId);
            }
        }

        public List<EmployeeScheduleItem> GetFormattedEmployeeSchedule(int employeeId)
        {
            List<EmployeeScheduleItem> scheduleItems = new List<EmployeeScheduleItem>();
            if (employeeId <= 0) return scheduleItems;

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
            public int Compare(Flight? x, Flight? y) => x?.Date.CompareTo(y?.Date) ?? 0;
        }

        public List<Employee> GetAvailableCrewGroupedByRole(Flight flight)
        {
            var allEmployees = employeeRepo.GetAllEmployees();
            var validEmployees = allEmployees.Where(e => IsEmployeeAvailable(e.Id, flight.Date, flight.Route.Id, flight.Id)).ToList();

            validEmployees.Sort(new EmployeeRoleAndNameComparer());
            return validEmployees;
        }

        private class EmployeeRoleAndNameComparer : IComparer<Employee>
        {
            public int Compare(Employee? x, Employee? y)
            {
                if (x == null || y == null) return 0;
                int roleComp = string.Compare(x.Role, y.Role, StringComparison.OrdinalIgnoreCase);
                return roleComp != 0 ? roleComp : string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}