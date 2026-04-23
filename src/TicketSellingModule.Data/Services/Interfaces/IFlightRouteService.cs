namespace TicketSellingModule.Data.Services.Interfaces;

public interface IFlightRouteService
{
    int Add(int companyID, int airportID, string route_type, int recurrence_interval,
                       DateTime start_date, DateTime end_date, TimeOnly dep_time, TimeOnly arr_time,
                       int capacity, string flight_number, int runwayID, int gateID);
    void CreateFlightWithSchedule(
            int companyId, string routeType, int airportId, int capacity, TimeSpan departureOffset, TimeSpan arrivalOffset,
            bool isRecurrent, DateTime? startDate, DateTime? endDate, DateTime? singleDate, string recurrenceType, string customDaysText,
            int runwayId, int gateId, Func<int, string> flightCodeGenerator);
    List<Flight> GetAllFlightsWithDetails();
    Route? GetRouteById(int routeId);

    Flight? GetFlightById(int flightId);

    List<Route> GetAllRoutes();

    List<Flight> GetAllFlights();
    void DeleteFlight(int flightId);
    List<Flight> GetFlightsByCompany(int companyId);
    string GetDestinationText(Flight flight);
}
