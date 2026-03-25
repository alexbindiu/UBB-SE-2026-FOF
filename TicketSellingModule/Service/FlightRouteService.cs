using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketSellingModule.Domain;
using TicketSellingModule.Repo;

namespace TicketSellingModule.Service
{
    public class FlightRouteService
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
                       int capacity, string flight_number, int runwayID, int gateID)
        {
            if (start_date > end_date)
                throw new ArgumentException("Start date can not be after end date.");

            if (capacity <= 0)
                throw new ArgumentException("Capacity must be greater than 0.");
            
            if (dep_time >= arr_time)
                throw new ArgumentException("Departure time must be before arrival time.");
            
            var allFlights = _flightRepo.GetAll();
            
            var flightsOnSameDate = allFlights.Where(f => f.Date.Date == start_date.Date).ToList();

            foreach (var existingFlight in flightsOnSameDate)
            {
                if (existingFlight.GateId == gateID || existingFlight.RunwayId == runwayID)
                {
                    var existingRoute = _routeRepo.GetRouteById(existingFlight.RouteId);
                    
                    if (existingRoute != null)
                    {
                        bool isTimeOverlap = dep_time < existingRoute.ArrivalTime && arr_time > existingRoute.DepartureTime;

                        if (isTimeOverlap)
                        {
                            if (existingFlight.GateId == gateID)
                            {
                                throw new InvalidOperationException(
                                    $"Gate Conflict: Gate {gateID} is already occupied by Flight {existingFlight.FlightNumber} " +
                                    $"from {existingRoute.DepartureTime} to {existingRoute.ArrivalTime}.");
                            }
                            
                            if (existingFlight.RunwayId == runwayID)
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
                RunwayId = runwayID, 
                GateId = gateID      
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
            if (flightId <= 0)
                throw new ArgumentException("Invalid flight ID.");

            if (_flightRepo.GetById(flightId) == null)
                throw new ArgumentException("Flight with the given ID does not exist.");

            _flightRepo.Delete(flightId);
        }
    }
}