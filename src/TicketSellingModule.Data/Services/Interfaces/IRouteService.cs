using System;
using System.Collections.Generic;
using System.Text;

namespace TicketSellingModule.Data.Services.Interfaces;

public interface IRouteService
{
    int AddWithInitialFlight(int companyId, int airportId, string routeType,
                                        int interval, DateTime start, DateTime end,
                                        TimeOnly dep, TimeOnly arr, int capacity,
                                        string flightNum, int runwayId, int gateId);
    Route? GetById(int id);
    List<Route> GetAll();
    string NormalizeFlightType(string? routeType);
    string GetRelevantTime(Route? route);
}
