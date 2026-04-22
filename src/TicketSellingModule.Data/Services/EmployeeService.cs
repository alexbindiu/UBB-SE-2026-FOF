namespace TicketSellingModule.Data.Services
{
    public class EmployeeService
    {
        private readonly EmployeeRepo employeeRepo;

        public EmployeeService(EmployeeRepo employeeRepo)
        {
            this.employeeRepo = employeeRepo;
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

        public int Add(string name, string role, DateOnly birthday, int salary, DateOnly hiringDate)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name can not be empty.");
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                throw new ArgumentException("Role can not be empty.");
            }

            if (salary < 0)
            {
                throw new ArgumentException("Salary can not be negative.");
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

        public void Update(int id, string? name = null, string? role = null, int? salary = null)
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

            if (role != null)
            {
                existingEmp.Role = role;
            }

            if (salary.HasValue)
            {
                existingEmp.Salary = salary.Value;
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
