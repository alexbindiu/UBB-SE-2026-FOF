using TicketSellingModule.Data.Repositories.Interfaces;
using TicketSellingModule.Data.Services.Interfaces;

namespace TicketSellingModule.Data.Services
{
    public class FlightRouteService : IFlightRouteService
    {
        private readonly IFlightRepo flightRepo;
        private readonly IRouteRepo routeRepo;
        private readonly ICompanyRepo companyRepo;
        private readonly IAirportRepo airportRepo;
        private readonly IRunwayService runwayService;
        private readonly IGateService gateService;
        private readonly IAirportService airportService;

        public FlightRouteService(
            IFlightRepo flightRepo,
            IRouteRepo routeRepo,
            ICompanyRepo companyRepo,
            IAirportRepo airportRepo,
            IRunwayService runwayService,
            IGateService gateService,
            IAirportService airportService)
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
            DateTime flightFullDateTime = start_date.Date.Add(dep_time.ToTimeSpan());

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
                Date = flightFullDateTime,
                FlightNumber = flight_number,
                Runway = new Runway { Id = runwayID },
                Gate = new Gate { Id = gateID }
            };

            flightRepo.Add(initialFlight);

            return routeId;
        }

        public void CreateFlightWithSchedule(
            int companyId, string routeType, int airportId, int capacity, TimeSpan departureOffset, TimeSpan arrivalOffset,
            bool isRecurrent, DateTime? startDate, DateTime? endDate, DateTime? singleDate, string recurrenceType, string customDaysText,
            int runwayId, int gateId, Func<int, string> flightCodeGenerator)
        {
            DateTime start = isRecurrent ? startDate?.Date ?? DateTime.Today : singleDate?.Date ?? DateTime.Today;
            DateTime end = isRecurrent ? endDate?.Date ?? start : start;

            if (isRecurrent && end < start)
            {
                throw new InvalidOperationException("End date must be after start date.");
            }

            int interval = 0;
            if (isRecurrent)
            {
                interval = recurrenceType switch
                {
                    "Daily" => 1,
                    "Weekly" => 7,
                    "Monthly" => 30,
                    "Custom" => int.TryParse(customDaysText, out int custom) && custom > 0 ? custom : throw new InvalidOperationException("Invalid custom interval."),
                    _ => throw new InvalidOperationException("Recurrence type is required.")
                };
            }

            TimeOnly depTime = TimeOnly.FromTimeSpan(departureOffset);
            TimeOnly arrTime = TimeOnly.FromTimeSpan(arrivalOffset);

            if (depTime == arrTime)
            {
                throw new InvalidOperationException("Arrival time cannot be the same as departure time.");
            }

            string flightNum = flightCodeGenerator(companyId);

            Add(companyId, airportId, routeType, interval, start, end, depTime, arrTime, capacity, flightNum, runwayId, gateId);
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

        public string GetDestinationText(Flight flight)
        {
            if (flight.Route?.Airport == null)
            {
                return "-";
            }

            return $"{flight.Route.Airport.AirportCode} - {flight.Route.Airport.AirportName}";
        }
    }
}
