using System;
using System.Collections.Generic;
using System.Text;

namespace TicketSellingModule.Data.Repositories.Interfaces;

public interface IGateRepo
{
    List<Gate> GetAllGates();
    Gate GetGateById(int id);
    int AddGate(Gate newGate);
    void DeleteGate(int id);
    void UpdateGate(Gate updatedGate);
}
