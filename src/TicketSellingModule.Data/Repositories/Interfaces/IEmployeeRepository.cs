namespace TicketSellingModule.Data.Repositories.Interfaces;

public interface IEmployeeRepository
{
    List<Employee> GetAllEmployees();
    Employee? GetEmployeeById(int employeeId);
    int AddEmployee(Employee newEmployee);
    void UpdateEmployee(Employee updatedEmployee);
    void DeleteEmployee(int employeeId);
}
