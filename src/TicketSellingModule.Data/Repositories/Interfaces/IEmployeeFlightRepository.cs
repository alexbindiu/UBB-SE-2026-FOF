namespace TicketSellingModule.Data.Repositories.Interfaces;

public interface IEmployeeFlightRepository
{
    void AssignFlightToEmployeeUsingIds(int employeeId, int flightId);
    void RemoveFlightFromEmployeeUsingIds(int employeeId, int flightId);
    List<int> GetFlightsByEmployeeId(int employeeId);
    List<int> GetEmployeesByFlightId(int flightId);
    void RemoveAllByFlightId(int flightId);
    void RemoveAllByEmployeeId(int employeeId);
}
