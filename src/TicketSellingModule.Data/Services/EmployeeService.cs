namespace TicketSellingModule.Data.Services
{
    public class EmployeeService(
        IEmployeeRepository employeeRepository,
        IEmployeeFlightService employeeFlightService) : IEmployeeService
    {
        public List<Employee> GetAllEmployees()
        {
            return employeeRepository.GetAllEmployees();
        }

        public Employee? GetEmployeeById(int employeeId)
        {
            if (employeeId <= 0)
            {
                return null;
            }

            return employeeRepository.GetEmployeeById(employeeId);
        }

        public List<Employee> GetPilots()
        {
            return this.GetEmployeeByRole(EmployeeRole.Pilot);
        }

        public List<Employee> GetFlightAttendants()
        {
            return this.GetEmployeeByRole(EmployeeRole.FlightAttendant);
        }

        public List<Employee> GetCoPilots()
        {
            return this.GetEmployeeByRole(EmployeeRole.CoPilot);
        }

        public List<Employee> GetFlightDispatchers()
        {
            return this.GetEmployeeByRole(EmployeeRole.FlightDispatcher);
        }

        private List<Employee> GetEmployeeByRole(EmployeeRole role)
        {
            List<Employee> allEmployees = this.GetAllEmployees();
            List<Employee> filteredEmployees = new List<Employee>();

            foreach (Employee employee in allEmployees)
            {
                if (employee.Role == role)
                {
                    filteredEmployees.Add(employee);
                }
            }

            return filteredEmployees;
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

            DateOnly finalBirthday = DateOnly.FromDateTime(birthday.Value.DateTime);
            DateOnly finalHiringDate = DateOnly.FromDateTime(hiringDate.Value.DateTime);

            if (finalBirthday > DateOnly.FromDateTime(DateTime.Now))
            {
                throw new ArgumentException("Birthday cannot be in the future.");
            }

            editingEmployee.Salary = parsedSalary;

            if (editingEmployee.Id == 0)
            {
                this.AddEmployee(
                    editingEmployee.Name,
                    editingEmployee.Role,
                    finalBirthday,
                    editingEmployee.Salary,
                    finalHiringDate);
            }
            else
            {
                this.UpdateEmployee(
                    editingEmployee.Id,
                    editingEmployee.Name,
                    editingEmployee.Role,
                    editingEmployee.Salary,
                    finalBirthday,
                    finalHiringDate);
            }
        }

        public void DeleteWithAssignments(int employeeId)
        {
            if (employeeId <= 0)
            {
                throw new ArgumentException("Invalid employee selected.");
            }

            employeeFlightService.RemoveAllFlightsAssignmentsForEmployee(employeeId);
            this.DeleteEmployeeUsingId(employeeId);
        }

        public int AddEmployee(string name, EmployeeRole role, DateOnly birthday, int salary, DateOnly hiringDate)
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

            Employee newEmployee = new Employee
            {
                Name = name,
                Role = role,
                Birthday = birthday,
                Salary = salary,
                HiringDate = hiringDate
            };

            return employeeRepository.AddEmployee(newEmployee);
        }

        public void UpdateEmployee(
            int employeeId,
            string? name = null,
            EmployeeRole? role = null,
            int? salary = null,
            DateOnly? birthday = null,
            DateOnly? hiringDate = null)
        {
            Employee? existingEmployee = employeeRepository.GetEmployeeById(employeeId);

            if (existingEmployee == null)
            {
                return;
            }

            if (name != null)
            {
                existingEmployee.Name = name;
            }

            if (role.HasValue)
            {
                existingEmployee.Role = role.Value;
            }

            if (salary.HasValue)
            {
                existingEmployee.Salary = salary.Value;
            }

            if (birthday.HasValue)
            {
                existingEmployee.Birthday = birthday.Value;
            }

            if (hiringDate.HasValue)
            {
                existingEmployee.HiringDate = hiringDate.Value;
            }

            employeeRepository.UpdateEmployee(existingEmployee);
        }

        public void DeleteEmployeeUsingId(int employeeId)
        {
            if (employeeId <= 0)
            {
                return;
            }

            employeeRepository.DeleteEmployee(employeeId);
        }
    }
}