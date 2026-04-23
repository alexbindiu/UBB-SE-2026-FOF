using System;
using System.Collections.Generic;
using System.Text;

namespace TicketSellingModule.Data.Repositories.Interfaces;

public interface IAirportRepo
{
    List<Airport> GetAllAirports();
    Airport GetAirportById(int id);
    int AddAirport(Airport newAirport);
    void DeleteAirport(int id);
    void UpdateAirport(Airport airport);
}
