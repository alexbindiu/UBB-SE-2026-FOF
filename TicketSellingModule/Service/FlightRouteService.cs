using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketSellingModule.Domain;
using TicketSellingModule.Repo;

namespace TicketSellingModule.Service
{
    internal class FlightRouteService
    {
        private readonly FlightRepo _flightRepo;
        private readonly RouteRepo _routeRepo;

        public FlightRouteService(FlightRepo flightRepo, RouteRepo routeRepo)
        {
            _flightRepo = flightRepo;
            _routeRepo = routeRepo;
        }

        public int Add(int companyID, int airportID, string route_type, int recurrence_interval,
                       DateTime start_date, DateTime end_date, TimeOnly dep_time, TimeOnly arr_time,
                       int capacity, string flight_number)
        {
            if (start_date > end_date)
                throw new ArgumentException("Start date can not be after end date.");

            if (capacity <= 0)
                throw new ArgumentException("Capacity must be greater than 0.");

            Route newRoute = new Route
            {
                CompanyId = companyID,
                AirportId = airportID,
                RouteType = route_type,
                RecurrenceInterval = recurrence_interval,
                StartDate = DateOnly.FromDateTime(start_date),
                EndDate = DateOnly.FromDateTime(end_date),
                DepartureTime = dep_time,
                ArrivalTime = arr_time,
                Capacity = capacity
            };

            int routeId = _routeRepo.AddRoute(newRoute);

            
            Flight initialFlight = new Flight
            {
                RouteId = routeId,
                Date = start_date,
                FlightNumber = flight_number,
                RouteId = routeId,
                Date = start_date,
                FlightNumber = flight_number,
                RunwayId = runwayID, 
                GateId = gateID,      
            };

            _flightRepo.Add(initialFlight);

            return routeId;
        }

        public Route? GetRouteById(int routeId)
        {
            return _routeRepo.GetRouteById(routeId);
        }

        public Flight? GetFlightById(int flightId)
        {
            return _flightRepo.GetById(flightId);
        }

        public List<Route> GetAllRoutes()
        {
            return _routeRepo.GetAllRoutes();
        }

        public List<Flight> GetAllFlights()
        {
            return _flightRepo.GetAll();
        }

        public void DeleteFlight(int flightId)
        {
            if (_flightRepo.GetById(flightId) == null)
                throw new ArgumentException("Flight with the given ID does not exist.");

            if (_flightRepo.GetById(flightId) < 0)
                throw new ArgumentException("Flight with the given ID does not exist.");
            _flightRepo.Delete(flightId);
        }
    }
}
