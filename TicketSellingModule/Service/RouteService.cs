using System;
using System.Collections.Generic;
using System.Linq;
using TicketSellingModule.Domain;
using TicketSellingModule.Repo;

namespace TicketSellingModule.Service
{
    public class RouteService
    {
        private readonly RouteRepo _routeRepo;
        private readonly FlightRepo _flightRepo;
        private readonly CompanyRepo _companyRepo;
        private readonly AirportRepo _airportRepo;

        public RouteService(RouteRepo routeRepo, FlightRepo flightRepo,
                            CompanyRepo companyRepo, AirportRepo airportRepo)
        {
            _routeRepo = routeRepo;
            _flightRepo = flightRepo;
            _companyRepo = companyRepo;
            _airportRepo = airportRepo;
        }

        public List<Route> GetAll()
        {
            return _routeRepo.GetAllRoutes();
        }

        public Route? GetById(int id)
        {
            if (id <= 0) return null;
            return _routeRepo.GetRouteById(id);
        }

        public void Delete(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid route ID.");
            _routeRepo.DeleteRoute(id);
        }

        public void Update(Route updatedRoute)
        {
            if (updatedRoute == null)
                throw new ArgumentNullException(nameof(updatedRoute));
            _routeRepo.UpdateRoute(updatedRoute);
        }

        public int AddWithInitialFlight(int companyId, int airportId, string routeType,
                                        int recurrenceInterval, DateTime startDate, DateTime endDate,
                                        TimeOnly depTime, TimeOnly arrTime, int capacity,
                                        string flightNumber, int runwayId, int gateId)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date cannot be after end date.");
            if (capacity <= 0)
                throw new ArgumentException("Capacity must be greater than 0.");
            //if (depTime >= arrTime)
            //    throw new ArgumentException("Departure time must be before arrival time.");

            CheckConflicts(startDate, depTime, arrTime, gateId, runwayId, flightNumber);

            Route newRoute = new Route
            {
                CompanyId = companyId,
                Company = _companyRepo.GetCompanyById(companyId),
                AirportId = airportId,
                Airport = _airportRepo.GetAirportById(airportId),
                RouteType = routeType,
                RecurrenceInterval = recurrenceInterval,
                StartDate = DateOnly.FromDateTime(startDate),
                EndDate = DateOnly.FromDateTime(endDate),
                DepartureTime = depTime,
                ArrivalTime = arrTime,
                Capacity = capacity
            };

            int routeId = _routeRepo.AddRoute(newRoute);

            Flight initialFlight = new Flight
            {
                RouteId = routeId,
                Date = startDate,
                FlightNumber = flightNumber,
                RunwayId = runwayId,
                GateId = gateId
            };

            _flightRepo.Add(initialFlight);

            return routeId;
        }

        private void CheckConflicts(DateTime date, TimeOnly depTime, TimeOnly arrTime,
                                    int gateId, int runwayId, string flightNumber)
        {
            var allFlights = _flightRepo.GetAll();
            var sameDayFlights = allFlights.Where(f => f.Date.Date == date.Date).ToList();

            foreach (var existing in sameDayFlights)
            {
                if (existing.GateId != gateId && existing.RunwayId != runwayId)
                    continue;

                var existingRoute = _routeRepo.GetRouteById(existing.RouteId);
                if (existingRoute == null) continue;

                bool overlap = depTime < existingRoute.ArrivalTime &&
                               arrTime > existingRoute.DepartureTime;

                if (!overlap) continue;

                if (existing.GateId == gateId)
                    throw new InvalidOperationException(
                        $"Gate conflict: gate {gateId} is occupied by flight {existing.FlightNumber} " +
                        $"from {existingRoute.DepartureTime} to {existingRoute.ArrivalTime}.");

                if (existing.RunwayId == runwayId)
                    throw new InvalidOperationException(
                        $"Runway conflict: runway {runwayId} is in use by flight {existing.FlightNumber} " +
                        $"from {existingRoute.DepartureTime} to {existingRoute.ArrivalTime}.");
            }
        }
    }
}