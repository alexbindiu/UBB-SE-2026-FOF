using System;
using System.Collections.Generic;
using System.Text;

namespace TicketSellingModule.Data.Repositories.Interfaces;

public interface IFlightRepo
{
    List<Flight> GetAll();
    Flight GetById(int id);
    List<Flight> GetFlightsByRoute(int routeId);
    List<Flight> GetFlightsByRunway(int runwayId);
    List<Flight> GetFlightsByGate(int gateId);
    List<Flight> GetFlightsByAirport(int airportId);
    int Add(Flight flight);
    void Update(Flight flight);
    void Delete(int id);

}
