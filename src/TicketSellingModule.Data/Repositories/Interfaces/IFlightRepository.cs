namespace TicketSellingModule.Data.Repositories.Interfaces;

public interface IFlightRepository
{
    List<Flight> GetAllFlights();
    Flight? GetById(int flightId);
    List<Flight> GetFlightsByRouteId(int routeId);
    List<Flight> GetFlightsByRunwayId(int runwayId);
    List<Flight> GetFlightsByGateId(int gateId);
    List<Flight> GetFlightsByAirportId(int airportId);
    int AddFlight(Flight newFlight);
    void UpdateFlight(Flight updatedFlight);
    void DeleteFlightUsingId(int flightid);
}
