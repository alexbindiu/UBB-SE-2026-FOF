using System;
using System.Collections.Generic;
using System.Text;

namespace TicketSellingModule.Data.Services.Interfaces;

public interface IAirportService
{
    List<Airport> GetAll();
    Airport GetById(int id);
    int Add(string code, string namde, string city);
    void Update(int id, string? newCity = null, string? newName = null, string? newCode = null);
    void Delete(int id);
    bool HasFlights(int airportId);
}
