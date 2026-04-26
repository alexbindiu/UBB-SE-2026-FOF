namespace TicketSellingModule.Data.Services
{
    public class RouteService(
        IRouteRepository routeRepository,
        IFlightRepository flightRepository,
        ICompanyRepository companyRepository,
        IAirportRepository airportRepository) : IRouteService
    {
        private const int MinutesInADay = 1440;
        private const int MinutesInAnHour = 60;
        private const string ArrivalCode = "ARR";
        private const string ArrivalFullName = "ARRIVAL";
        private const string DepartureCode = "DEP";
        private const string DepartureFullName = "DEPARTURE";
        private const string EmptyFieldPlaceholder = "-";
        private const string TimeFormat = "HH:mm";
        private bool CheckOverlappingTimes(TimeOnly startTime1, TimeOnly endTime1, TimeOnly startTime2, TimeOnly endTime2)
        {
            int startMinutes1 = (startTime1.Hour * MinutesInAnHour) + startTime1.Minute;
            int endMinutes1 = (endTime1.Hour * MinutesInAnHour) + endTime1.Minute;

            if (endMinutes1 <= startMinutes1)
            {
                endMinutes1 += MinutesInADay;
            }

            int startMinutes2 = (startTime2.Hour * MinutesInAnHour) + startTime2.Minute;
            int endMinutes2 = (endTime2.Hour * MinutesInAnHour) + endTime2.Minute;

            if (endMinutes2 <= startMinutes2)
            {
                endMinutes2 += MinutesInADay;
            }

            return startMinutes1 < endMinutes2 && startMinutes2 < endMinutes1;
        }

        public int AddWithInitialFlight(
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
            List<Flight> allFlights = flightRepository.GetAllFlights();
            List<Flight> sameDayFlights = new List<Flight>();

            foreach (Flight flight in allFlights)
            {
                if (flight.Date.Date == startDate.Date)
                {
                    sameDayFlights.Add(flight);
                }
            }

            foreach (Flight existingFlight in sameDayFlights)
            {
                if (existingFlight.Gate?.Id == gateId || existingFlight.Runway?.Id == runwayId)
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
                            throw new InvalidOperationException("Resource Conflict: The selected Gate or Runway is already occupied during this time.");
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

            Flight initialFlight = new Flight
            {
                Route = new Route { Id = routeId },
                Date = startDate,
                FlightNumber = flightNumber,
                Runway = new Runway { Id = runwayId },
                Gate = new Gate { Id = gateId }
            };

            flightRepository.AddFlight(initialFlight);

            return routeId;
        }

        public Route? GetRouteById(int routeId)
        {
            return routeRepository.GetRouteById(routeId);
        }

        public List<Route> GetAllRoutes()
        {
            return routeRepository.GetAllRoutes();
        }

        public string NormalizeFlightType(string? routeType)
        {
            if (string.IsNullOrWhiteSpace(routeType))
            {
                return EmptyFieldPlaceholder;
            }

            string upperCaseType = routeType.Trim().ToUpperInvariant();

            if (upperCaseType.StartsWith(ArrivalCode) || upperCaseType.StartsWith(ArrivalFullName))
            {
                return ArrivalCode;
            }

            if (upperCaseType.StartsWith(DepartureCode) || upperCaseType.StartsWith(DepartureFullName))
            {
                return DepartureCode;
            }

            return upperCaseType;
        }

        public string GetRelevantTime(Route? route)
        {
            if (route == null)
            {
                return EmptyFieldPlaceholder;
            }

            string normalizedType = this.NormalizeFlightType(route.RouteType);

            if (normalizedType == ArrivalCode)
            {
                return route.ArrivalTime.ToString(TimeFormat);
            }

            return route.DepartureTime.ToString(TimeFormat);
        }
    }
}