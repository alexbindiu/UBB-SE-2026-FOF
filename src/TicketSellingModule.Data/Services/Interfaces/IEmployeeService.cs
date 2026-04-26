namespace TicketSellingModule.Data.Services.Interfaces;

public interface IEmployeeService
{
    List<Employee> GetAllEmployees();
    Employee? GetEmployeeById(int id);
    List<Employee> GetPilots();
    List<Employee> GetFlightAttendants();
    List<Employee> GetCoPilots();
    List<Employee> GetFlightDispatchers();
    void SaveEmployee(Employee editingEmployee, DateTimeOffset? birthday, DateTimeOffset? hiringDate, string salaryText);
    void DeleteWithAssignments(int id);
    int AddEmployee(string name, EmployeeRole role, DateOnly birthday, int salary, DateOnly hiringDate);
    void UpdateEmployee(int id, string? name = null, EmployeeRole? role = null, int? salary = null, DateOnly? birthday = null, DateOnly? hiringDate = null);
    void DeleteEmployeeUsingId(int id);
    EmployeeRole ParseRole(string roleText);
    int Login(string employeeIdText);
}
