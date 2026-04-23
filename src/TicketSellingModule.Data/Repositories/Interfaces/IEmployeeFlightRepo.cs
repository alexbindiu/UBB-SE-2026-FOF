using System;
using System.Collections.Generic;
using System.Text;

namespace TicketSellingModule.Data.Repositories.Interfaces;

public interface IEmployeeFlightRepo
{
    void AssignFlightToEmployee(int employeeId, int flightId);
    void RemoveFlightFromEmployee(int employeeId, int flightId);
    List<int> GetFlightsByEmployee(int employeeId);
    List<int> GetEmployeesByFlight(int flightId);
    void RemoveAllByFlightId(int flightId);
    void RemoveAllByEmployeeId(int employeeId);
}
