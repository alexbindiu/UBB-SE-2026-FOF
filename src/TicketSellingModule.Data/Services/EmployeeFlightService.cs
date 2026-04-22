namespace TicketSellingModule.Data.Services
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
                    /* already assigned, ignore */
                }
            }
        }

        public void UpdateCrewForFlight(int flightId, List<int> newEmployeeIds)
        {
            var currentCrewIds = GetFlightCrew(flightId).Select(e => e.Id).ToList();
            foreach (var empId in currentCrewIds.Except(newEmployeeIds).ToList())
            {
                RemoveCrewMember(flightId, empId);
            }

            foreach (var empId in newEmployeeIds.Except(currentCrewIds).ToList())
            {
                AssignCrewMember(flightId, empId);
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
            var scheduleItems = new List<EmployeeScheduleItem>();

            if (employeeId <= 0)
            {
                return scheduleItems;
            }

            var flights = GetEmployeeSchedule(employeeId).OrderBy(f => f.Date).ToList();

            foreach (var flight in flights)
            {
                var route = routeRepo.GetRouteById(flight.Route.Id);
                var gate = flight.Gate?.Id > 0 ? gateService.GetById(flight.Gate.Id) : null;
                var runway = runwayService.GetByIdSafe(flight.Runway?.Id ?? 0);

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

        public static string NormalizeEmployeeRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return "Other";
            }

            return role.Trim();
        }

        public bool IsValidEmployeeRole(string? role)
        {
            var validRoles = new[] { "Pilot", "Co-Pilot", "Flight Attendant", "Flight Dispatcher" };
            string normalized = NormalizeEmployeeRole(role);
            return validRoles.Contains(normalized);
        }

        public List<Employee> GetAvailableCrewGroupedByRole(Flight flight)
        {
            var available = GetAvailableEmployeesForFlight(flight);

            var roleOrder = new Dictionary<string, int>
            {
                ["Pilot"] = 0,
                ["Co-Pilot"] = 1,
                ["Flight Attendant"] = 2,
                ["Flight Dispatcher"] = 3
            };

            // Filter out employees with "Other" role and invalid roles
            var validEmployees = available.Where(e => IsValidEmployeeRole(e.Role)).ToList();

            var grouped = validEmployees
                .GroupBy(e => NormalizeEmployeeRole(e.Role))
                .OrderBy(g => roleOrder.TryGetValue(g.Key, out var idx) ? idx : int.MaxValue)
                .ThenBy(g => g.Key)
                .SelectMany(g => g.OrderBy(e => e.Name))
                .ToList();

            return grouped;
        }
    }
}