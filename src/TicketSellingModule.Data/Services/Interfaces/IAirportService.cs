namespace TicketSellingModule.Data.Services.Interfaces;

public interface IAirportService
{
    List<Airport> GetAllAirports();
    Airport? GetAirportById(int airportId);
    int AddAirport(string airportCode, string airportName, string city);
    void UpdateAirport(int airportId, string? newCity = null, string? newName = null, string? newCode = null);
    void SaveAirport(int airportId, string airportCode, string airportName, string city);
    void DeleteAirportUsingId(int airportId);
    bool HasFlights(int airportId);
    string GetDeleteWarningMessage(int airportId);
}
