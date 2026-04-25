namespace TicketSellingModule.Data.Services
{
    public class EmployeeFlightService(
        IEmployeeFlightRepository employeeFlightRepository,
        IEmployeeRepository employeeRepository,
        IFlightRepository flightRepository,
        IRouteRepository routeRepository,
        IGateService gateService,
        IRunwayService runwayService,
        IRouteService routeService) : IEmployeeFlightService
    {
        public void AssignEmployeeToFlightUsingIds(int flightId, int employeeId)
        {
            if (flightId <= 0 || employeeId <= 0)
            {
                throw new ArgumentException("Invalid flight or employee ID.");
            }

            Employee? employee = employeeRepository.GetEmployeeById(employeeId);
            Flight? flight = flightRepository.GetFlightById(flightId);

            if (employee == null || flight == null)
            {
                throw new InvalidOperationException("Employee or Flight does not exist.");
            }

            var currentCrewIds = employeeFlightRepository.GetEmployeesByFlightId(flightId);
            if (currentCrewIds.Contains(employeeId))
            {
                throw new InvalidOperationException("Employee already assigned to this flight.");
            }

            if (!IsEmployeeAvailable(employeeId, flight.Date, flight.Route.Id, flight.Id))
            {
                throw new InvalidOperationException($"Conflict: Employee {employee.Name} is already assigned to another flight during this time.");
            }

            employeeFlightRepository.AssignFlightToEmployeeUsingIds(employeeId, flightId);
        }

        public void RemoveEmployeeFromFlightUsingIds(int flightId, int employeeId)
        {
            employeeFlightRepository.RemoveFlightFromEmployeeUsingIds(employeeId, flightId);
        }

        public void RemoveAllCrewAssignmentsForFlight(int flightId)
        {
            if (flightId > 0)
            {
                employeeFlightRepository.RemoveAllByFlightId(flightId);
            }
        }

        public void RemoveAllFlightsAssignmentsForEmployee(int employeeId)
        {
            if (employeeId > 0)
            {
                employeeFlightRepository.RemoveAllByEmployeeId(employeeId);
            }
        }

        public List<Employee> GetEmployeesAssignedToFlight(int flightId)
        {
            List<Employee> flightCrew = new List<Employee>();
            List<int> crewIdentifiers = employeeFlightRepository.GetEmployeesByFlightId(flightId);

            foreach (int identifier in crewIdentifiers)
            {
                Employee? employee = employeeRepository.GetEmployeeById(identifier);
                if (employee != null)
                {
                    flightCrew.Add(employee);
                }
            }

            return flightCrew;
        }

        public List<Flight> GetEmployeeSchedule(int employeeId)
        {
            if (employeeId <= 0)
            {
                return new List<Flight>();
            }

            List<int> assignedFlightIdentifiers = employeeFlightRepository.GetFlightsByEmployeeId(employeeId);
            List<Flight> scheduledFlights = new List<Flight>();

            foreach (int identifier in assignedFlightIdentifiers)
            {
                Flight? flight = flightRepository.GetFlightById(identifier);
                if (flight != null)
                {
                    scheduledFlights.Add(flight);
                }
            }

            return scheduledFlights;
        }

        public List<EmployeeScheduleItem> GetFormattedEmployeeSchedule(int employeeId)
        {
            List<EmployeeScheduleItem> scheduleItems = new List<EmployeeScheduleItem>();
            if (employeeId <= 0)
            {
                return scheduleItems;
            }

            List<Flight> flights = this.GetEmployeeSchedule(employeeId);
            flights.Sort(new FlightDateComparer());

            foreach (Flight flight in flights)
            {
                Route? route = routeRepository.GetRouteById(flight.Route.Id);

                Gate? gate = null;
                if (flight.Gate != null && flight.Gate.Id > 0)
                {
                    gate = gateService.GetGateById(flight.Gate.Id);
                }

                Runway? runway = null;
                if (flight.Runway != null)
                {
                    runway = runwayService.GetRunwayById(flight.Runway.Id);
                }

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

        public bool IsEmployeeAvailable(int employeeId, DateTime targetDate, int targetRouteId, int? excludedFlightId = null)
        {
            Route? targetRoute = routeRepository.GetRouteById(targetRouteId);
            if (targetRoute == null)
            {
                return false;
            }

            List<Flight> completeSchedule = this.GetEmployeeSchedule(employeeId);

            foreach (Flight scheduledFlight in completeSchedule)
            {
                if (scheduledFlight.Date.Date == targetDate.Date && scheduledFlight.Id != excludedFlightId)
                {
                    Route? scheduledRoute = routeRepository.GetRouteById(scheduledFlight.Route.Id);

                    if (scheduledRoute != null)
                    {
                        bool isTimeOverlap = targetRoute.DepartureTime < scheduledRoute.ArrivalTime &&
                                             targetRoute.ArrivalTime > scheduledRoute.DepartureTime;

                        if (isTimeOverlap)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public void AssignEmpolyeesToFlightUsingIds(int flightId, List<int> employeeIds)
        {
            foreach (int employeeId in employeeIds)
            {
                try
                {
                    AssignEmployeeToFlightUsingIds(flightId, employeeId);
                }
                catch
                {
                    //--intent: ignore existing assignments or minor conflicts during bulk operation.
                }
            }
        }

        public void UpdateEmployeesForFlightUsingIds(int flightId, List<int> updatedEmployeeIds)
        {
            List<int> existingCrewId = employeeFlightRepository.GetEmployeesByFlightId(flightId);

            foreach (int existingId in existingCrewId)
            {
                if (!updatedEmployeeIds.Contains(existingId))
                {
                    this.RemoveEmployeeFromFlightUsingIds(flightId, existingId);
                }
            }

            foreach (int newId in updatedEmployeeIds)
            {
                if (!existingCrewId.Contains(newId))
                {
                    this.AssignEmployeeToFlightUsingIds(flightId, newId);
                }
            }
        }

        public List<EmployeeScheduleItem> GenerateFormattedSchedule(int employeeId)
        {
            List<EmployeeScheduleItem> formattedItems = new List<EmployeeScheduleItem>();
            if (employeeId <= 0)
            {
                return formattedItems;
            }

            List<Flight> scheduledFlights = this.GetEmployeeSchedule(employeeId);
            scheduledFlights.Sort(new FlightDateComparer());

            foreach (Flight flight in scheduledFlights)
            {
                Route? route = routeRepository.GetRouteById(flight.Route.Id);
                Gate? gate = flight.Gate?.Id > 0 ? gateService.GetGateById(flight.Gate.Id) : null;
                Runway? runway = runwayService.GetRunwayById(flight.Runway?.Id ?? 0);

                formattedItems.Add(new EmployeeScheduleItem
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

            return formattedItems;
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

        public List<Employee> GetAvailableEmployeesGroupedByRole(Flight flight)
        {
            List<Employee> allEmployees = employeeRepository.GetAllEmployees();
            List<Employee> availableEmployees = new List<Employee>();

            foreach (Employee candidate in allEmployees)
            {
                if (this.IsEmployeeAvailable(candidate.Id, flight.Date, flight.Route.Id, flight.Id))
                {
                    availableEmployees.Add(candidate);
                }
            }

            availableEmployees.Sort(new EmployeeRoleAndNameComparer());
            return availableEmployees;
        }

        private class EmployeeRoleAndNameComparer : IComparer<Employee>
        {
            public int Compare(Employee? firstEmployee, Employee? secondEmployee)
            {
                if (firstEmployee == null || secondEmployee == null)
                {
                    return 0;
                }

                int roleComparison = firstEmployee.Role.CompareTo(secondEmployee.Role);

                if (roleComparison != 0)
                {
                    return roleComparison;
                }

                return string.Compare(firstEmployee.Name, secondEmployee.Name, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}