namespace TicketSellingModule.Data.Services
{
    public class RouteService
    {
        private readonly RouteRepo routeRepo;
        private readonly FlightRepo flightRepo;
        private readonly CompanyRepo companyRepo;
        private readonly AirportRepo airportRepo;

        public RouteService(RouteRepo routeRepo, FlightRepo flightRepo,
                            CompanyRepo companyRepo, AirportRepo airportRepo)
        {
            this.routeRepo = routeRepo;
            this.flightRepo = flightRepo;
            this.companyRepo = companyRepo;
            this.airportRepo = airportRepo;
        }

        private bool CheckOverlappingTimes(TimeOnly start1, TimeOnly end1, TimeOnly start2, TimeOnly end2)
        {
            int s1 = (start1.Hour * 60) + start1.Minute;
            int e1 = (end1.Hour * 60) + end1.Minute;
            if (e1 <= s1)
            {
                e1 += 1440;
            }

            int s2 = (start2.Hour * 60) + start2.Minute;
            int e2 = (end2.Hour * 60) + end2.Minute;
            if (e2 <= s2)
            {
                e2 += 1440;
            }

            return s1 < e2 && s2 < e1;
        }

        public int AddWithInitialFlight(int companyId, int airportId, string routeType,
                                        int interval, DateTime start, DateTime end,
                                        TimeOnly dep, TimeOnly arr, int capacity,
                                        string flightNum, int runwayId, int gateId)
        {
            var sameDayFlights = flightRepo.GetAll().Where(f => f.Date.Date == start.Date);
            foreach (var existing in sameDayFlights)
            {
                if (existing.Gate?.Id == gateId || existing.Runway?.Id == runwayId)
                {
                    var existingRoute = routeRepo.GetRouteById(existing.Route.Id);
                    if (existingRoute != null && CheckOverlappingTimes(dep, arr, existingRoute.DepartureTime, existingRoute.ArrivalTime))
                    {
                        throw new InvalidOperationException("Resource Conflict: Gate or Runway occupied.");
                    }
                }
            }

            Route newRoute = new Route
            {
                Company = companyRepo.GetCompanyById(companyId),
                Airport = airportRepo.GetAirportById(airportId),
                RouteType = routeType,
                RecurrenceInterval = interval,
                StartDate = DateOnly.FromDateTime(start),
                EndDate = DateOnly.FromDateTime(end),
                DepartureTime = dep,
                ArrivalTime = arr,
                Capacity = capacity
            };

            int routeId = routeRepo.AddRoute(newRoute);

            Flight initialFlight = new Flight
            {
                Route = new Route { Id = routeId },
                Date = start,
                FlightNumber = flightNum,
                Runway = new Runway { Id = runwayId },
                Gate = new Gate { Id = gateId }
            };
            flightRepo.Add(initialFlight);

            return routeId;
        }

        public Route? GetById(int id) => routeRepo.GetRouteById(id);
        public List<Route> GetAll() => routeRepo.GetAllRoutes();

        public string NormalizeFlightType(string? routeType)
        {
            if (string.IsNullOrWhiteSpace(routeType))
            {
                return "-";
            }

            string value = routeType.Trim().ToUpperInvariant();
            if (value.StartsWith("ARR") || value.StartsWith("ARRIVAL"))
            {
                return "ARR";
            }

            if (value.StartsWith("DEP") || value.StartsWith("DEPARTURE"))
            {
                return "DEP";
            }

            return value;
        }

        public string GetRelevantTime(Route? route)
        {
            if (route == null)
            {
                return "-";
            }

            return NormalizeFlightType(route.RouteType) == "ARR"
                ? route.ArrivalTime.ToString("HH:mm")
                : route.DepartureTime.ToString("HH:mm");
        }
    }
}