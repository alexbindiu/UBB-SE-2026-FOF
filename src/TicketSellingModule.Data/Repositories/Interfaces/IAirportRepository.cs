namespace TicketSellingModule.Data.Repositories.Interfaces;

public interface IAirportRepository
{
    List<Airport> GetAllAirports();
    Airport? GetAirportById(int airportId);
    int AddAirport(Airport newAirport);
    void DeleteAirportUsingId(int airportId);
    void UpdateAirport(Airport updatedAirport);
}
