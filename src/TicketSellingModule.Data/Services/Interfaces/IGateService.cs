using System;
using System.Collections.Generic;
using System.Text;

namespace TicketSellingModule.Data.Services.Interfaces;

public interface IGateService
{
    List<Gate> GetAll();
    Gate GetById(int id);
    int Add(string name);
    void Update(int id, string? newName = null);
    void Delete(int id);
    void SaveGate(int id, string name);
    bool HasFlights(int gateId);
}
