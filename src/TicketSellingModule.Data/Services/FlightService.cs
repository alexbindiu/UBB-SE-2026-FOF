using TicketSellingModule.Data.Repositories.Interfaces;
using TicketSellingModule.Data.Services.Interfaces;

namespace TicketSellingModule.Data.Services
{
    public class FlightService : IFlightService
    {
        private readonly IFlightRepository flightRepo;

        public FlightService(IFlightRepository flightRepo)
        {
            this.flightRepo = flightRepo;
        }

        public List<Flight> GetAll()
        {
            return flightRepo.GetAllFlights();
        }

        public Flight? GetById(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return flightRepo.GetById(id);
        }

        public List<Flight> GetByRoute(int routeId)
        {
            if (routeId <= 0)
            {
                return new List<Flight>();
            }

            return flightRepo.GetFlightsByRouteId(routeId);
        }

        public int Add(string flightNumber, int routeId, DateTime date, int runwayId, int gateId)
        {
            if (string.IsNullOrWhiteSpace(flightNumber))
            {
                throw new ArgumentException("Flight number cannot be empty.");
            }

            if (routeId <= 0)
            {
                throw new ArgumentException("Invalid route ID.");
            }

            Flight newFlight = new Flight
            {
                FlightNumber = flightNumber,
                Route = new Route { Id = routeId },
                Date = date,
                Runway = new Runway { Id = runwayId },
                Gate = new Gate { Id = gateId }
            };

            return flightRepo.AddFlight(newFlight);
        }

        public void Update(int id, DateTime? date = null, string? flightNumber = null,
                           int? runwayId = null, int? gateId = null)
        {
            var existing = flightRepo.GetById(id);
            if (existing == null)
            {
                throw new InvalidOperationException($"Flight with ID {id} does not exist.");
            }

            if (date.HasValue)
            {
                existing.Date = date.Value;
            }

            if (flightNumber != null)
            {
                existing.FlightNumber = flightNumber;
            }

            if (runwayId.HasValue)
            {
                existing.Runway = new Runway { Id = runwayId.Value };
            }

            if (gateId.HasValue)
            {
                existing.Gate = new Gate { Id = gateId.Value };
            }

            flightRepo.UpdateFlight(existing);
        }

        public void Delete(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid flight ID.");
            }

            if (flightRepo.GetById(id) == null)
            {
                throw new InvalidOperationException($"Flight with ID {id} does not exist.");
            }

            flightRepo.DeleteFlightUsingId(id);
        }
    }
}