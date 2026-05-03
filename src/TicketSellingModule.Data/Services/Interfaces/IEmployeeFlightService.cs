namespace TicketSellingModule.Data.Services.Interfaces;

public interface IEmployeeFlightService
{
    void AssignEmployeeToFlightUsingIds(int flightId, int employeeId);
    void RemoveEmployeeFromFlightUsingIds(int flightId, int employeeId);
    List<Employee> GetEmployeesAssignedToFlight(int flightId);
    List<Flight> GetEmployeeSchedule(int employeeId);
    bool IsEmployeeAvailable(int employeeId, DateTime targetDate, int targetRouteId, int? excludedFlightId);
    void AssignEmpolyeesToFlightUsingIds(int flightId, List<int> employeeIds);
    void UpdateEmployeesForFlightUsingIds(int flightId, List<int> updatedEmployeeIds);
    void RemoveAllCrewAssignmentsForFlight(int flightId);
    void RemoveAllFlightsAssignmentsForEmployee(int employeeId);
    List<EmployeeScheduleItem> GetFormattedEmployeeSchedule(int employeeId);
    List<Employee> GetAvailableEmployeesGroupedByRole(Flight flight);
    string FormatCrewList(int flightId);
    public List<CrewMemberSelectionData> GetCrewSelectionData(Flight flight);
}
