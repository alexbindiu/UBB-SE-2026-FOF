using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketSellingModule.Domain;
using TicketSellingModule.Repo;

namespace TicketSellingModule.Service
{
    internal class EmployeeService
    {
        private readonly EmployeeRepo _employeeRepo;

        public EmployeeService(EmployeeRepo employeeRepo)
        {
            _employeeRepo = employeeRepo;
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
