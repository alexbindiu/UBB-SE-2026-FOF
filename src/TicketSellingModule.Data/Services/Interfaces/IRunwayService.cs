namespace TicketSellingModule.Data.Services.Interfaces;

public interface IRunwayService
{
    List<Runway> GetAll();
    Runway GetById(int id);
    Runway? GetByIdSafe(int id);
    int Add(string name, int handleTime);
    void Update(int id, string? newName = null, int? newHandleTime = null);
    void Delete(int id);
    void SaveRunway(int id, string name, string handleTimeText);
    bool HasFlights(int runwayId);
}
