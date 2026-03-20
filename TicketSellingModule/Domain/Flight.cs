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
        public DateTime Date { get; set; }
        public string FlightNumber{ get; set; }
        private List<Employee> Employees = new List<Employee>();
        public List<Employee> GetParticipatingEmployees()
        {
            return this.Employees;
        }
        public void AddEmployee(Employee employee)
        {
            Employees.Add(employee);
        }
        public void RemoveEmployee(Employee employee)
        {
            Employees.Remove(employee);
        }
        public Gate Gate { get; set; }
        public Runway Runway{ get; set; }
        public Route Route{ get; set; }
    }
}
