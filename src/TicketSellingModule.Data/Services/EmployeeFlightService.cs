using TicketSellingModule.Data.Services.Interfaces;

namespace TicketSellingModule.Data.Services
{
    public class EmployeeFlightService : IEmployeeFlightService
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
            this.linkRepo = linkRepo;
            this.employeeRepo = employeeRepo;
            this.flightRepo = flightRepo;
            this.routeRepo = routeRepo;
            this.gateService = gateService;
            this.runwayService = runwayService;
            this.routeService = routeService;
        }

        public void AssignCrewMember(int flightId, int employeeId)
        {
            if (flightId <= 0 || employeeId <= 0)
            {
                return;
            }
            var emp = employeeRepo.GetEmployeeById(employeeId);
            if (emp == null)
            {
                throw new InvalidOperationException("The employee does not exist in database.");
            }
            var currentCrew = linkRepo.GetEmployeesByFlight(flightId);
            if (currentCrew.Contains(employeeId))
            {
                throw new InvalidOperationException("Employee already assigned to this flight.");
            }
            linkRepo.AssignFlightToEmployee(employeeId, flightId);
        }

        public void RemoveCrewMember(int flightId, int employeeId)
        {
            linkRepo.RemoveFlightFromEmployee(employeeId, flightId);
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
            var flights = new List<Flight>();
            foreach (var id in linkRepo.GetFlightsByEmployee(employeeId))
            {
                var flight = flightRepo.GetById(id);
                if (flight != null)
                {
                    flights.Add(flight);
                }
            }
            return flights;
        }

        public List<Employee> GetAvailableEmployeesForFlight(Flight targetFlight)
        {
            var available = new List<Employee>();
            var targetRoute = routeRepo.GetRouteById(targetFlight.Route.Id);
            if (targetRoute == null)
            {
                return available;
            }

            foreach (var emp in employeeRepo.GetAllEmployees())
            {
                var sameDayFlights = GetEmployeeSchedule(emp.Id)
                    .Where(f => f.Date.Date == targetFlight.Date.Date && f.Id != targetFlight.Id)
                    .ToList();

                bool isDoubleBooked = false;
                foreach (var scheduledFlight in sameDayFlights)
                {
                    var scheduledRoute = routeRepo.GetRouteById(scheduledFlight.Route.Id);
                    if (scheduledRoute != null)
                    {
                        bool overlap = targetRoute.DepartureTime < scheduledRoute.ArrivalTime &&
                                       targetRoute.ArrivalTime > scheduledRoute.DepartureTime;
                        if (overlap)
                        {
                            isDoubleBooked = true;
                            break;
                        }
                    }
                }

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