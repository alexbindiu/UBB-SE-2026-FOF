using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketSellingModule.Domain;
using TicketSellingModule.Repo;

namespace TicketSellingModule.Service
{
    public class EmployeeService
    {
        private readonly EmployeeRepo _employeeRepo;
        private readonly EmployeeFlightService _employeeFlightService;

        public EmployeeService(EmployeeRepo employeeRepo, EmployeeFlightService employeeFlightService)
        {
            _employeeRepo = employeeRepo;
            _employeeFlightService = employeeFlightService;
        }

        public List<Employee> GetAll()
        {
            return _employeeRepo.GetAllEmployees();
        }

        public Employee? GetById(int id)
        {
            if (id <= 0) return null;
            return _employeeRepo.GetEmployeeById(id);
        }

        public List<Employee> GetPilots() => GetByRole("Pilot");

        public List<Employee> GetFlightAttendants() => GetByRole("Flight Attendant");

        public List<Employee> GetCoPilots() => GetByRole("Co-Pilot");

        public List<Employee> GetFlightDispatchers() => GetByRole("Flight Dispatcher");

        private List<Employee> GetByRole(string role)
        {
            return GetAll().Where(e => e.Role == role).ToList();
        }

        public void SaveEmployee(Employee editingEmployee, DateTimeOffset? birthday, DateTimeOffset? hiringDate, string salaryText)
        {
            if (editingEmployee == null)
                throw new ArgumentException("Invalid employee selected.");

            if (birthday == null || hiringDate == null)
                throw new ArgumentException("Birthday and Hiring Date are required.");

            if (!int.TryParse(salaryText, out int parsedSalary))
                throw new ArgumentException("Salary must be a valid number.");

            var finalBirthday = DateOnly.FromDateTime(birthday.Value.DateTime);
            var finalHiringDate = DateOnly.FromDateTime(hiringDate.Value.DateTime);

            editingEmployee.Salary = parsedSalary;

            if (editingEmployee.Id == 0)
            {
                Add(
                    name: editingEmployee.Name,
                    role: editingEmployee.Role,
                    birthday: finalBirthday,
                    salary: editingEmployee.Salary,
                    hiringDate: finalHiringDate
                );
            }
            else
            {
                Update(
                    id: editingEmployee.Id,
                    name: editingEmployee.Name,
                    role: editingEmployee.Role,
                    salary: editingEmployee.Salary
                );
            }
        }

        public void DeleteWithAssignments(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid employee selected.");

            _employeeFlightService.CleanUpEmployeeAssignments(id);
            Delete(id);
        }

        public int Add(string name, string role, DateOnly birthday, int salary, DateOnly hiringDate)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name can not be empty.");
            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentException("Role can not be empty.");
            if (salary < 0) throw new ArgumentException("Salary can not be negative.");
            if (hiringDate > DateOnly.FromDateTime(DateTime.Now)) throw new ArgumentException("Hiring date can not be in the future.");

            Employee newEmp = new Employee
            {
                Name = name,
                Role = role,
                Birthday = birthday,
                Salary = salary,
                HiringDate = hiringDate
            };

            return _employeeRepo.AddEmployee(newEmp);
        }

        public void Update(int id, string? name = null, string? role = null, int? salary = null)
        {
            var existingEmp = _employeeRepo.GetEmployeeById(id);
            if (existingEmp == null) return;

            if (name != null) existingEmp.Name = name;
            if (role != null) existingEmp.Role = role;
            if (salary.HasValue) existingEmp.Salary = salary.Value;

            _employeeRepo.UpdateEmployee(existingEmp);
        }

        public void Delete(int id)
        {
            if (id <= 0) return;
            _employeeRepo.DeleteEmployee(id);
        }
    }
}
