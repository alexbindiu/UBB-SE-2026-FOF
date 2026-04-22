namespace TicketSellingModule.Data.Services
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
            linkRepo = linkRepo;
            employeeRepo = employeeRepo;
            flightRepo = flightRepo;
            routeRepo = routeRepo;
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
                try { AssignCrewMember(flightId, empId); }
                catch { /* already assigned, ignore */ }
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
    }
}