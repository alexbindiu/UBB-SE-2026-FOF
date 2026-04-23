using TicketSellingModule.Data.Repositories.Interfaces;
using TicketSellingModule.Data.Services.Interfaces;

namespace TicketSellingModule.Data.Services
{
    public class AirportService : IAirportService
    {
        private readonly AirportRepository airportRepo;
        private readonly FlightRepository flightRepo;
        public AirportService(AirportRepository airportRepo, FlightRepository flightRepo)
        {
            this.airportRepo = airportRepo;
            this.flightRepo = flightRepo;
        }

        public List<Airport> GetAll()
        {
            return airportRepo.GetAllAirports();
        }

        public Airport GetById(int id)
        {
            if (id <= 0)
            {
                return null;
            }
            return airportRepo.GetAirportById(id);
        }

        public int Add(string code, string name, string city)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Airport code cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Airport name cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(city))
            {
                throw new ArgumentException("City cannot be empty.");
            }

            Airport newAirport = new Airport
            {
                AirportCode = code,
                AirportName = name,
                City = city
            };

            return airportRepo.AddAirport(newAirport);
        }

        public void Update(int id, string? newCity = null, string? newName = null, string? newCode = null)
        {
            var existingAirport = airportRepo.GetAirportById(id);
            if (existingAirport == null)
            {
                return;
            }

            if (newName != null)
            {
                existingAirport.AirportName = newName;
            }
            if (newCity != null)
            {
                existingAirport.City = newCity;
            }
            if (newCode != null)
            {
                existingAirport.AirportCode = newCode;
            }

            airportRepo.UpdateAirport(existingAirport);
        }

        public void Delete(int id)
        {
            if (id > 0)
            {
                airportRepo.DeleteAirportUsingId(id);
            }
        }

        public bool HasFlights(int airportId)
        {
            return flightRepo.GetFlightsByAirportId(airportId).Any();
        }
    }
}
