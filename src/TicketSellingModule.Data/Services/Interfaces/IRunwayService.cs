namespace TicketSellingModule.Data.Services.Interfaces;

public interface IRunwayService
{
    List<Runway> GetAllRunways();
    Runway? GetRunwayById(int runwayId);
    int AddRunway(string name, int handleTime);
    void UpdateRunway(int runwayId, string? newName = null, int? newHandleTime = null);
    void DeleteRunwayUsingId(int runwayId);
    void SaveRunway(int runwayId, string name, string handleTimeText);
    bool HasFlights(int runwayId);
    string GetDeleteWarningMessage(int runwayId);
}
