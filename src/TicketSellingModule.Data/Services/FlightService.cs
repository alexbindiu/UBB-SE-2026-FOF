namespace TicketSellingModule.Data.Services
{
    public class FlightService(IFlightRepository flightRepository) : IFlightService
    {
        public List<Flight> GetAllFlights()
        {
            return flightRepository.GetAllFlights();
        }

        public Flight? GetFlightById(int flightId)
        {
            if (flightId <= 0)
            {
                return null;
            }

            return flightRepository.GetFlightById(flightId);
        }

        public List<Flight> GetFlightsByRouteId(int routeId)
        {
            if (routeId <= 0)
            {
                return new List<Flight>();
            }

            return flightRepository.GetFlightsByRouteId(routeId);
        }

        public int AddFlight(string flightNumber, int routeId, DateTime date, int runwayId, int gateId)
        {
            if (string.IsNullOrWhiteSpace(flightNumber))
            {
                throw new ArgumentException("The flight number cannot be empty.");
            }

            if (routeId <= 0)
            {
                throw new ArgumentException("A valid route Id is required.");
            }

            Flight newFlight = new Flight
            {
                FlightNumber = flightNumber,
                Route = new Route { Id = routeId },
                Date = date,
                Runway = new Runway { Id = runwayId },
                Gate = new Gate { Id = gateId }
            };

            return flightRepository.AddFlight(newFlight);
        }

        public void UpdateFlight(
            int flightId,
            DateTime? date = null,
            string? flightNumber = null,
            int? runwayId = null,
            int? gateId = null)
        {
            Flight? existingFlight = flightRepository.GetFlightById(flightId);

            if (existingFlight == null)
            {
                throw new InvalidOperationException($"Flight with Id {flightId} does not exist in the system.");
            }

            if (date.HasValue)
            {
                existingFlight.Date = date.Value;
            }

            if (flightNumber != null)
            {
                existingFlight.FlightNumber = flightNumber;
            }

            if (runwayId.HasValue)
            {
                existingFlight.Runway = new Runway { Id = runwayId.Value };
            }

            if (gateId.HasValue)
            {
                existingFlight.Gate = new Gate { Id = gateId.Value };
            }

            flightRepository.UpdateFlight(existingFlight);
        }

        public void DeleteFlightUsingId(int flightId)
        {
            if (flightId <= 0)
            {
                throw new ArgumentException("The provided flight Id is invalid.");
            }

            if (flightRepository.GetFlightById(flightId) == null)
            {
                throw new InvalidOperationException($"Flight with Id {flightId} does not exist.");
            }

            flightRepository.DeleteFlightUsingId(flightId);
        }
    }

}