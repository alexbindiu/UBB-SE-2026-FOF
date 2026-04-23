namespace TicketSellingModule.Data.Repositories.Interfaces;

public interface IGateRepository
{
    List<Gate> GetAllGates();
    Gate? GetGateById(int gateId);
    int AddGate(Gate newGate);
    void DeleteGateUsingId(int gateId);
    void UpdateGate(Gate updatedGate);
}
