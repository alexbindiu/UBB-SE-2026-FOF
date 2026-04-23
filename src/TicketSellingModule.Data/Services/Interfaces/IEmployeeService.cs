namespace TicketSellingModule.Data.Services.Interfaces;

public interface IEmployeeService
{
    List<Employee> GetAll();
    Employee? GetById(int id);
    List<Employee> GetPilots();
    List<Employee> GetFlightAttendants();
    List<Employee> GetCoPilots();
    List<Employee> GetFlightDispatchers();
    void SaveEmployee(Employee editingEmployee, DateTimeOffset? birthday, DateTimeOffset? hiringDate, string salaryText);
    void DeleteWithAssignments(int id);
    int Add(string name, EmployeeRole role, DateOnly birthday, int salary, DateOnly hiringDate);
    void Update(int id, string? name = null, EmployeeRole? role = null, int? salary = null, DateOnly? birthday = null, DateOnly? hiringDate = null);
    void Delete(int id);
}
