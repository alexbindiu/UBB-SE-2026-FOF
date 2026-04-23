using System;
using System.Collections.Generic;
using System.Text;

namespace TicketSellingModule.Data.Repositories.Interfaces;

public interface IEmployeeRepo
{
    List<Employee> GetAllEmployees();
    Employee GetEmployeeById(int id);
    int AddEmployee(Employee newEmployee);
    void UpdateEmployee(Employee updatedEmployee);
    void DeleteEmployee(int id);
}
