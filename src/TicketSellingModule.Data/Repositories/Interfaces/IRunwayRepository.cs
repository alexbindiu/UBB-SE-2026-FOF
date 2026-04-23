namespace TicketSellingModule.Data.Repositories.Interfaces;

public interface IRunwayRepository
{
    List<Runway> GetAllRunways();
    Runway? GetRunwayById(int runwayId);
    int AddRunway(Runway newRunway);
    void UpdateRunway(Runway updatedRunway);
    void DeleteRunwayUsingId(int runwayId);
}
