using System;
using System.Collections.Generic;
using System.Text;

namespace TicketSellingModule.Data.Services.Interfaces;

public interface IFlightService
{
    List<Flight> GetAll();
    Flight? GetById(int id);
    List<Flight> GetByRoute(int routeId);
    int Add(string flightNumber, int routeId, DateTime date, int runwayId, int gateId);
    void Update(int id, DateTime? date = null, string? flightNumber = null,
                           int? runwayId = null, int? gateId = null);
    void Delete(int id);
}
