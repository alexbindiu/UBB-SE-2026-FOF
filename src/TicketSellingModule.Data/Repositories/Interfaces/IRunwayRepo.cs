using System;
using System.Collections.Generic;
using System.Text;

namespace TicketSellingModule.Data.Repositories.Interfaces;

public interface IRunwayRepo
{
    List<Runway> GetAllRunways();
    Runway GetRunwayById(int id);
    int AddRunway(Runway newRunway);
    void UpdateRunway(Runway updatedRunway);
    void DeleteRunway(int id);
}
