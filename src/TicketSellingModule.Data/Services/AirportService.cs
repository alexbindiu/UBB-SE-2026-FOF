namespace TicketSellingModule.Data.Services
{
    public class AirportService(
       IAirportRepository airportRepository,
       IFlightRepository flightRepository) : IAirportService
    {
        public List<Airport> GetAllAirports()
        {
            return airportRepository.GetAllAirports();
        }

        public Airport? GetAirportById(int airportId)
        {
            if (airportId <= 0)
            {
                return null;
            }

            return airportRepository.GetAirportById(airportId);
        }

        public int AddAirport(string airportCode, string airportName, string city)
        {
            if (string.IsNullOrWhiteSpace(airportCode))
            {
                throw new ArgumentException("The airport code cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(airportName))
            {
                throw new ArgumentException("The airport name cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(city))
            {
                throw new ArgumentException("The city name cannot be empty.");
            }

            Airport newAirport = new Airport
            {
                AirportCode = airportCode,
                AirportName = airportName,
                City = city
            };

            return airportRepository.AddAirport(newAirport);
        }

        public void UpdateAirport(int airportId,
            string? newCity = null,
            string? newName = null,
            string? newCode = null)
        {
            Airport? existingAirport = airportRepository.GetAirportById(airportId);

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

            airportRepository.UpdateAirport(existingAirport);
        }

        public void SaveAirport(int airportId, string airportCode, string airportName, string city)
        {
            if (airportId == 0)
            {
                this.AddAirport(airportCode, airportName, city);
            }
            else
            {
                this.UpdateAirport(airportId, city, airportName, airportCode);
            }
        }
        public void DeleteAirportUsingId(int airportId)
        {
            if (airportId > 0)
            {
                airportRepository.DeleteAirportUsingId(airportId);
            }
        }

        public bool HasFlights(int airportId)
        {
            List<Flight> associatedFlights = flightRepository.GetFlightsByAirportId(airportId);
            return associatedFlights.Count > 0;
        }

        public string GetDeleteWarningMessage(int id)
        {
            bool hasFlights = HasFlights(id);
            if (hasFlights)
            {
                return $"CRITICAL: Airport '{GetAirportById(id).AirportName}' has flights assigned. Deleting it will remove ALL associated flights. Proceed?";
            }
            return $"Are you sure you want to delete airport '{GetAirportById(id).AirportName}'?";
        }
    }
}
