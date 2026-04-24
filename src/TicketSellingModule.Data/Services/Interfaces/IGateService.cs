namespace TicketSellingModule.Data.Services.Interfaces;

public interface IGateService
{
    List<Gate> GetAllGates();
    Gate? GetGateById(int gateId);
    int Add(string name);
    void Update(int gateId, string? newName = null);
    void DeleteGateUsingId(int gateId);
    void SaveGate(int gateId, string name);
    bool HasFlights(int gateId);
}
