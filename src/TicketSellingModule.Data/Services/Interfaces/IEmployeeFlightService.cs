using System;
using System.Collections.Generic;
using System.Text;

namespace TicketSellingModule.Data.Services.Interfaces;

public interface IEmployeeFlightService
{
    void AssignCrewMember(int flightId, int employeeId);
    void RemoveCrewMember(int flightId, int employeeId);
    List<Employee> GetFlightCrew(int flightId);
    List<Flight> GetEmployeeSchedule(int employeeId);
    List<Employee> GetAvailableEmployeesForFlight(Flight targetFlight);
    void AssignCrewToFlight(int flightId, List<int> employeeIds);
    void UpdateCrewForFlight(int flightId, List<int> newEmployeeIds);
    void CleanUpFlightAssignments(int flightId);
    void CleanUpEmployeeAssignments(int employeeId);
    List<EmployeeScheduleItem> GetFormattedEmployeeSchedule(int employeeId);
    List<Employee> GetAvailableCrewGroupedByRole(Flight flight);
}
