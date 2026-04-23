namespace TicketSellingModule.Data.Repositories.Interfaces;

public interface IEmployeeFlightRepository
{
    void AssignFlightToEmployeesUsingIds(int employeeId, int flightId);
    void RemoveFlightFromEmployeeUsingIds(int employeeId, int flightId);
    List<int> GetFlightsByEmployeeId(int employeeId);
    List<int> GetEmployeesByFlightId(int flightId);
    void RemoveAllByFlightId(int flightId);
    void RemoveAllByEmployeeId(int employeeId);
}
