using System;
using System.Collections.Generic;
using TicketSellingModule.Domain;
using TicketSellingModule.Repo;

namespace TicketSellingModule.Service
{
    public class FlightService
    {
        private readonly FlightRepo _flightRepo;

        public FlightService(FlightRepo flightRepo)
        {
            _flightRepo = flightRepo;
        }

        public List<Flight> GetAll()
        {
            return _flightRepo.GetAll();
        }

        public Flight? GetById(int id)
        {
            if (id <= 0) return null;
            return _flightRepo.GetById(id);
        }

        public List<Flight> GetByRoute(int routeId)
        {
            if (routeId <= 0) return new List<Flight>();
            return _flightRepo.GetFlightsByRoute(routeId);
        }

        public int Add(string flightNumber, int routeId, DateTime date, int runwayId, int gateId)
        {
            if (string.IsNullOrWhiteSpace(flightNumber))
                throw new ArgumentException("Flight number cannot be empty.");
            if (routeId <= 0)
                throw new ArgumentException("Invalid route ID.");

            Flight newFlight = new Flight
            {
                FlightNumber = flightNumber,
                RouteId = routeId,
                Date = date,
                RunwayId = runwayId,
                GateId = gateId
            };

            return _flightRepo.Add(newFlight);
        }

        public void Update(int id, DateTime? date = null, string? flightNumber = null,
                           int? runwayId = null, int? gateId = null)
        {
            var existing = _flightRepo.GetById(id);
            if (existing == null)
                throw new InvalidOperationException($"Flight with ID {id} does not exist.");

            if (date.HasValue) existing.Date = date.Value;
            if (flightNumber != null) existing.FlightNumber = flightNumber;
            if (runwayId.HasValue) existing.RunwayId = runwayId.Value;
            if (gateId.HasValue) existing.GateId = gateId.Value;

            _flightRepo.Update(existing);
        }

        public void Delete(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid flight ID.");
            if (_flightRepo.GetById(id) == null)
                throw new InvalidOperationException($"Flight with ID {id} does not exist.");

            _flightRepo.Delete(id);
        }
    }
}