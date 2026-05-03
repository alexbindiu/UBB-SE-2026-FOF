namespace TicketSellingModule.Data.Services.Interfaces;

public interface IFlightService
{
    List<Flight> GetAllFlights();
    Flight? GetFlightById(int flightId);
    List<Flight> GetFlightsByRouteId(int routeId);
    int AddFlight(string flightNumber, int routeId, DateTime date, int runwayId, int gateId);
    void UpdateFlight(int id, DateTime? date = null, string? flightNumber = null,
                           int? runwayId = null, int? gateId = null);
    void DeleteFlightUsingId(int flightId);
}
