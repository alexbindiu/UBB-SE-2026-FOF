using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSellingModule.Domain
{
    internal class Flight
    {
        public int Id { get; set; }
        public DateTime date { get; set; }
        public string FlightNumber{ get; set; }
        private List<Employee> Employees;
        public List<Employee> GetParticipatingEmployees()
        {
            return this.Employees;
        }
        public void AddEmployee(Employee employee)
        {
            Employees.add(employee);
        }
        public void RemoveEmployee(Employee employee)
        {
            Employees.remove(employee);
        }
        public Gate Gate { get; set; }
        public Runway Runway{ get; set; }
        public Route{ get; set; }
    }
}
