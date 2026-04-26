namespace TicketSellingModule.Data.Services
{
    public class FlightRouteService(
        IFlightRepository flightRepository,
        IRouteRepository routeRepository,
        ICompanyRepository companyRepository,
        IAirportRepository airportRepository,
        IRunwayService runwayService,
        IGateService gateService,
        IAirportService airportService) : IFlightRouteService
    {
        private const int MinutesInADay = 1440;
        private const string DailyReccurance = "Daily";
        private const string WeeklyReccurance = "Weekly";
        private const string MonthlyReccurance = "Monthly";
        private const string CustomReccurance = "Custom";
        private const string FullDateFormatting = "dd.MM.yyyy HH:mm";

        private bool CheckOverlappingTimes(TimeOnly startTime1, TimeOnly endTime1, TimeOnly startTime2, TimeOnly endTime2)
        {
            int startMinutes1 = (startTime1.Hour * 60) + startTime1.Minute;
            int endMinutes1 = (endTime1.Hour * 60) + endTime1.Minute;

            if (endMinutes1 <= startMinutes1)
            {
                endMinutes1 += MinutesInADay;
            }

            int startMinutes2 = (startTime2.Hour * 60) + startTime2.Minute;
            int endMinutes2 = (endTime2.Hour * 60) + endTime2.Minute;

            if (endMinutes2 <= startMinutes2)
            {
                endMinutes2 += MinutesInADay;
            }

            return startMinutes1 < endMinutes2 && startMinutes2 < endMinutes1;
        }

        public int AddFlightToRoute(
            int companyId,
            int airportId,
            string routeType,
            int recurrenceInterval,
            DateTime startDate,
            DateTime endDate,
            TimeOnly departureTime,
            TimeOnly arrivalTime,
            int capacity,
            string flightNumber,
            int runwayId,
            int gateId)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException("The start date cannot be after the end date.");
            }

            if (capacity <= 0)
            {
                throw new ArgumentException("Capacity must be a positive number greater than 0.");
            }

            List<Flight> allFlights = flightRepository.GetAllFlights();

            foreach (Flight existingFlight in allFlights)
            {
                if (existingFlight.Date.Date == startDate.Date)
                {
                    if (existingFlight.Gate.Id == gateId || existingFlight.Runway.Id == runwayId)
                    {
                        Route? existingRoute = routeRepository.GetRouteById(existingFlight.Route.Id);

                        if (existingRoute != null)
                        {
                            bool isTimeOverlap = this.CheckOverlappingTimes(
                                departureTime,
                                arrivalTime,
                                existingRoute.DepartureTime,
                                existingRoute.ArrivalTime);

                            if (isTimeOverlap)
                            {
                                if (existingFlight.Gate.Id == gateId)
                                {
                                    throw new InvalidOperationException($"Gate Conflict: Gate {gateId} is occupied by Flight {existingFlight.FlightNumber}.");
                                }

                                if (existingFlight.Runway.Id == runwayId)
                                {
                                    throw new InvalidOperationException($"Runway Conflict: Runway {runwayId} is occupied by Flight {existingFlight.FlightNumber}.");
                                }
                            }
                        }
                    }
                }
            }

            Route newRoute = new Route
            {
                Company = companyRepository.GetCompanyById(companyId),
                Airport = airportRepository.GetAirportById(airportId),
                RouteType = routeType,
                RecurrenceInterval = recurrenceInterval,
                StartDate = DateOnly.FromDateTime(startDate),
                EndDate = DateOnly.FromDateTime(endDate),
                DepartureTime = departureTime,
                ArrivalTime = arrivalTime,
                Capacity = capacity
            };

            int routeId = routeRepository.AddRoute(newRoute);

            DateTime flightFullDateTime = startDate.Date.Add(departureTime.ToTimeSpan());

            Flight initialFlight = new Flight
            {
                Route = new Route { Id = routeId },
                Date = flightFullDateTime,
                FlightNumber = flightNumber,
                Runway = new Runway { Id = runwayId },
                Gate = new Gate { Id = gateId }
            };

            flightRepository.AddFlight(initialFlight);

            return routeId;
        }

        public void CreateFlightWithSchedule(
            int companyId,
            string routeType,
            int airportId,
            int capacity,
            TimeSpan departureOffset,
            TimeSpan arrivalOffset,
            bool isRecurrent,
            DateTime? startDate,
            DateTime? endDate,
            DateTime? singleDate,
            string recurrenceType,
            string customDaysText,
            int runwayId,
            int gateId,
            Func<int, string> flightCodeGenerator)
        {
            DateTime start = isRecurrent ? startDate?.Date ?? DateTime.Today : singleDate?.Date ?? DateTime.Today;
            DateTime end = isRecurrent ? endDate?.Date ?? start : start;

            if (isRecurrent && end < start)
            {
                throw new InvalidOperationException("The end date must be after the start date.");
            }

            int interval = 0;
            if (isRecurrent)
            {
                interval = recurrenceType switch
                {
                    DailyReccurance => 1,
                    WeeklyReccurance => 7,
                    MonthlyReccurance => 30,
                    CustomReccurance => int.TryParse(customDaysText, out int custom) && custom > 0 ? custom : throw new InvalidOperationException("Invalid custom interval."),
                    _ => throw new InvalidOperationException("A recurrence type is required for recurrent flights.")
                };
            }

            TimeOnly depTime = TimeOnly.FromTimeSpan(departureOffset);
            TimeOnly arrTime = TimeOnly.FromTimeSpan(arrivalOffset);

            if (depTime == arrTime)
            {
                throw new InvalidOperationException("Arrival time cannot be identical to departure time.");
            }

            string flightNum = flightCodeGenerator(companyId);

            this.AddFlightToRoute(companyId, airportId, routeType, interval, start, end, depTime, arrTime, capacity, flightNum, runwayId, gateId);
        }

        public List<Flight> GetAllFlightsWithDetails()
        {
            List<Flight> flights = this.GetAllFlights();

            foreach (Flight flight in flights)
            {
                if (flight.Runway != null && flight.Runway.Id > 0)
                {
                    flight.Runway = runwayService.GetRunwayById(flight.Runway.Id);
                }

                if (flight.Gate != null && flight.Gate.Id > 0)
                {
                    flight.Gate = gateService.GetGateById(flight.Gate.Id);
                }

                if (flight.Route != null && flight.Route.Id > 0)
                {
                    flight.Route = this.GetRouteById(flight.Route.Id);

                    if (flight.Route?.Airport != null && flight.Route.Airport.Id > 0)
                    {
                        flight.Route.Airport = airportService.GetAirportById(flight.Route.Airport.Id);
                    }
                }
            }

            return flights;
        }

        public List<Flight> SearchFlights(List<Flight> flights, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return flights;
            }

            List<Flight> matchingFlights = new List<Flight>();
            foreach (Flight flight in flights)
            {
                if (this.IsFlightMatch(flight, query))
                {
                    matchingFlights.Add(flight);
                }
            }

            return matchingFlights;
        }

        private bool IsFlightMatch(Flight flight, string query)
        {
            if (flight.FlightNumber != null && flight.FlightNumber.ToLowerInvariant().Contains(query))
            {
                return true;
            }

            if (flight.Date.ToString(FullDateFormatting).ToLowerInvariant().Contains(query))
            {
                return true;
            }

            string destination = this.GetDestinationText(flight).ToLowerInvariant();
            if (destination.Contains(query))
            {
                return true;
            }

            if (flight.Runway?.Name != null && flight.Runway.Name.ToLowerInvariant().Contains(query))
            {
                return true;
            }

            if (flight.Gate?.Name != null && flight.Gate.Name.ToLowerInvariant().Contains(query))
            {
                return true;
            }

            return false;
        }

        public Route? GetRouteById(int routeId)
        {
            return routeRepository.GetRouteById(routeId);
        }

        public Flight? GetFlightById(int flightId)
        {
            return flightRepository.GetFlightById(flightId);
        }

        public List<Route> GetAllRoutes()
        {
            return routeRepository.GetAllRoutes();
        }

        public List<Flight> GetAllFlights()
        {
            return flightRepository.GetAllFlights();
        }

        public void DeleteFlightUsingId(int flightId)
        {
            if (flightId <= 0)
            {
                throw new ArgumentException("The provided flight Id is invalid.");
            }

            if (this.GetFlightById(flightId) == null)
            {
                throw new ArgumentException("A flight with the specified Id does not exist.");
            }

            flightRepository.DeleteFlightUsingId(flightId);
        }

        public List<Flight> GetFlightsByCompanyId(int companyId)
        {
            List<Route> allRoutes = routeRepository.GetAllRoutes();
            List<int> companyRouteIds = new List<int>();

            foreach (Route route in allRoutes)
            {
                if (route.Company.Id == companyId)
                {
                    companyRouteIds.Add(route.Id);
                }
            }

            List<Flight> allFlights = flightRepository.GetAllFlights();
            List<Flight> filteredFlights = new List<Flight>();

            foreach (Flight flight in allFlights)
            {
                if (companyRouteIds.Contains(flight.Route.Id))
                {
                    filteredFlights.Add(flight);
                }
            }

            return filteredFlights;
        }

        public string GetDestinationText(Flight flight)
        {
            if (flight.Route == null || flight.Route.Airport == null)
            {
                return "-";
            }

            return $"{flight.Route.Airport.AirportCode} - {flight.Route.Airport.AirportName}";
        }
    }
}