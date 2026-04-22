namespace TicketSellingModule.Data.Services
{
    public class FlightRouteService
    {
        private readonly FlightRepo flightRepo;
        private readonly RouteRepo routeRepo;
        private readonly CompanyRepo companyRepo;
        private readonly AirportRepo airportRepo;
        private readonly RunwayService runwayService;
        private readonly GateService gateService;
        private readonly AirportService airportService;

        public FlightRouteService(
            FlightRepo flightRepo,
            RouteRepo routeRepo,
            CompanyRepo companyRepo,
            AirportRepo airportRepo,
            RunwayService runwayService,
            GateService gateService,
            AirportService airportService)
        {
            this.flightRepo = flightRepo;
            this.routeRepo = routeRepo;
            this.companyRepo = companyRepo;
            this.airportRepo = airportRepo;
            this.runwayService = runwayService;
            this.gateService = gateService;
            this.airportService = airportService;
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

        public int Add(int companyID, int airportID, string route_type, int recurrence_interval,
                       DateTime start_date, DateTime end_date, TimeOnly dep_time, TimeOnly arr_time,
                       int capacity, string flight_number, int runwayID, int gateID)
        {
            if (start_date > end_date)
            {
                throw new ArgumentException("Start date can not be after end date.");
            }

            if (capacity <= 0)
            {
                throw new ArgumentException("Capacity must be greater than 0.");
            }

            var allFlights = flightRepo.GetAll();
            var flightsOnSameDate = allFlights.Where(f => f.Date.Date == start_date.Date).ToList();

            foreach (var existingFlight in flightsOnSameDate)
            {
                if (existingFlight.Gate.Id == gateID || existingFlight.Runway.Id == runwayID)
                {
                    var existingRoute = routeRepo.GetRouteById(existingFlight.Route.Id);

                    if (existingRoute != null)
                    {
                        bool isTimeOverlap = CheckOverlappingTimes(dep_time, arr_time, existingRoute.DepartureTime, existingRoute.ArrivalTime);

                        if (isTimeOverlap)
                        {
                            if (existingFlight.Gate.Id == gateID)
                            {
                                throw new InvalidOperationException(
                                    $"Gate Conflict: Gate {gateID} is already occupied by Flight {existingFlight.FlightNumber} " +
                                    $"from {existingRoute.DepartureTime} to {existingRoute.ArrivalTime}.");
                            }

                            if (existingFlight.Runway.Id == runwayID)
                            {
                                throw new InvalidOperationException(
                                    $"Runway Conflict: Runway {runwayID} is already in use by Flight {existingFlight.FlightNumber} " +
                                    $"from {existingRoute.DepartureTime} to {existingRoute.ArrivalTime}.");
                            }
                        }
                    }
                }
            }

            Route newRoute = new Route
            {
                Company = companyRepo.GetCompanyById(companyID),
                Airport = airportRepo.GetAirportById(airportID),
                RouteType = route_type,
                RecurrenceInterval = recurrence_interval,
                StartDate = DateOnly.FromDateTime(start_date),
                EndDate = DateOnly.FromDateTime(end_date),
                DepartureTime = dep_time,
                ArrivalTime = arr_time,
                Capacity = capacity
            };

            int routeId = routeRepo.AddRoute(newRoute);

            Flight initialFlight = new Flight
            {
                Route = new Route { Id = routeId },
                Date = start_date,
                FlightNumber = flight_number,
                Runway = new Runway { Id = runwayID },
                Gate = new Gate { Id = gateID }
            };

            flightRepo.Add(initialFlight);

            return routeId;
        }

        public List<Flight> GetAllFlightsWithDetails()
        {
            var flights = GetAllFlights();
            foreach (var flight in flights)
            {
                if (flight.Runway?.Id > 0)
                {
                    flight.Runway = runwayService.GetById(flight.Runway.Id);
                }

                if (flight.Gate?.Id > 0)
                {
                    flight.Gate = gateService.GetById(flight.Gate.Id);
                }

                if (flight.Route?.Id > 0)
                {
                    flight.Route = GetRouteById(flight.Route.Id);
                    if (flight.Route?.Airport?.Id > 0)
                    {
                        flight.Route.Airport = airportService.GetById(flight.Route.Airport.Id);
                    }
                }
            }
            return flights;
        }

        public Route? GetRouteById(int routeId) => routeRepo.GetRouteById(routeId);

        public Flight? GetFlightById(int flightId) => flightRepo.GetById(flightId);

        public List<Route> GetAllRoutes() => routeRepo.GetAllRoutes();

        public List<Flight> GetAllFlights() => flightRepo.GetAll();

        public void DeleteFlight(int flightId)
        {
            if (flightId <= 0)
            {
                throw new ArgumentException("Invalid flight ID.");
            }

            if (flightRepo.GetById(flightId) == null)
            {
                throw new ArgumentException("Flight with the given ID does not exist.");
            }

            flightRepo.Delete(flightId);
        }

        public List<Flight> GetFlightsByCompany(int companyId)
        {
            var companyRouteIds = routeRepo.GetAllRoutes()
                .Where(r => r.Company.Id == companyId)
                .Select(r => r.Id)
                .ToList();

            return flightRepo.GetAll()
                .Where(f => companyRouteIds.Contains(f.Route.Id))
                .ToList();
        }
    }
}
