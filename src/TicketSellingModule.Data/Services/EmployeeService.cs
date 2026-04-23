using TicketSellingModule.Service;

namespace TicketSellingModule.Data.Services
{
    public class EmployeeService
    {
        private readonly EmployeeRepo employeeRepo;
        private readonly EmployeeFlightService employeeFlightService;

        public EmployeeService(EmployeeRepo employeeRepo, EmployeeFlightService employeeFlightService)
        {
            this.employeeRepo = employeeRepo;
            this.employeeFlightService = employeeFlightService;
        }

        public List<Employee> GetAll()
        {
            return employeeRepo.GetAllEmployees();
        }

        public Employee? GetById(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return employeeRepo.GetEmployeeById(id);
        }

        public List<Employee> GetPilots() => GetByRole(EmployeeRole.Pilot);

        public List<Employee> GetFlightAttendants() => GetByRole(EmployeeRole.FlightAttendant);

        public List<Employee> GetCoPilots() => GetByRole(EmployeeRole.CoPilot);

        public List<Employee> GetFlightDispatchers() => GetByRole(EmployeeRole.FlightDispatcher);

        private List<Employee> GetByRole(EmployeeRole role)
        {
            List<Employee> allEmployees = GetAll();
            List<Employee> filtered = new List<Employee>();

            foreach (Employee e in allEmployees)
            {
                if (e.Role == role)
                {
                    filtered.Add(e);
                }
            }
            return filtered;
        }

        public void SaveEmployee(Employee editingEmployee, DateTimeOffset? birthday, DateTimeOffset? hiringDate, string salaryText)
        {
            if (editingEmployee == null)
            {
                throw new ArgumentException("Invalid employee selected.");
            }

            if (birthday == null || hiringDate == null)
            {
                throw new ArgumentException("Birthday and Hiring Date are required.");
            }

            if (!int.TryParse(salaryText, out int parsedSalary))
            {
                throw new ArgumentException("Salary must be a valid number.");
            }

            var finalBirthday = DateOnly.FromDateTime(birthday.Value.DateTime);
            var finalHiringDate = DateOnly.FromDateTime(hiringDate.Value.DateTime);

            if (finalBirthday > DateOnly.FromDateTime(DateTime.Now))
            {
                throw new ArgumentException("Birthday cannot be in the future.");
            }

            editingEmployee.Salary = parsedSalary;

            if (editingEmployee.Id == 0)
            {
                Add(editingEmployee.Name, editingEmployee.Role,
                            DateOnly.FromDateTime(birthday.Value.DateTime),
                            editingEmployee.Salary,
                            DateOnly.FromDateTime(hiringDate.Value.DateTime));
            }
            else
            {
                Update(editingEmployee.Id,
                               editingEmployee.Name,
                               editingEmployee.Role,
                               editingEmployee.Salary,
                               DateOnly.FromDateTime(birthday.Value.DateTime),
                               DateOnly.FromDateTime(hiringDate.Value.DateTime));
            }
        }

        public void DeleteWithAssignments(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid employee selected.");
            }

            employeeFlightService.CleanUpEmployeeAssignments(id);
            Delete(id);
        }

        public int Add(string name, EmployeeRole role, DateOnly birthday, int salary, DateOnly hiringDate)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name can not be empty.");
            }

            if (salary < 0)
            {
                throw new ArgumentException("Salary can not be negative.");
            }

            if (birthday > DateOnly.FromDateTime(DateTime.Now))
            {
                throw new ArgumentException("Birthday cannot be in the future.");
            }

            if (hiringDate > DateOnly.FromDateTime(DateTime.Now))
            {
                throw new ArgumentException("Hiring date can not be in the future.");
            }

            Employee newEmp = new Employee
            {
                Name = name,
                Role = role,
                Birthday = birthday,
                Salary = salary,
                HiringDate = hiringDate
            };

            return employeeRepo.AddEmployee(newEmp);
        }

        public void Update(int id, string? name = null, string? role = null, int? salary = null,
                           DateOnly? birthday = null, DateOnly? hiringDate = null) // Added hiringDate
        public void Update(int id, string? name = null, EmployeeRole? role = null, int? salary = null)
        {
            var existingEmp = employeeRepo.GetEmployeeById(id);
            if (existingEmp == null)
            {
                return;
            }

            if (name != null)
            {
                existingEmp.Name = name;
            }

            if (role.HasValue)
            {
                existingEmp.Role = role.Value;
            }

            if (salary.HasValue)
            {
                existingEmp.Salary = salary.Value;
            }

            // Ensure these assignments are present:
            if (birthday.HasValue)
            {
                existingEmp.Birthday = birthday.Value;
            }

            if (hiringDate.HasValue)
            {
                existingEmp.HiringDate = hiringDate.Value;
            }

            employeeRepo.UpdateEmployee(existingEmp);
        }

        public void Delete(int id)
        {
            if (id <= 0)
            {
                return;
            }

            employeeRepo.DeleteEmployee(id);
        }
    }
}
