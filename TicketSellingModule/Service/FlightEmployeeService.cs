using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketSellingModule.Domain;
using TicketSellingModule.Repo;

namespace TicketSellingModule.Service
{
    internal class FlightEmployeeService
    {
        private readonly FlightEmployeeRepo _linkRepo;
        private readonly EmployeeRepo _employeeRepo;

        public FlightEmployeeService(FlightEmployeeRepo linkRepo, EmployeeRepo employeeRepo)
        {
            _linkRepo = linkRepo;
            _employeeRepo = employeeRepo;
        }

        public void AssignCrewMember(int flightId, int employeeId)
        {
            if (flightId <= 0 || employeeId <= 0) return;
            var emp = _employeeRepo.GetEmployeeById(employeeId);
            if (emp == null)
                throw new Exception("The employee does not exist in data base.");
            var currentCrew = _linkRepo.GetEmployeesByFlight(flightId);
            if (currentCrew.Contains(employeeId))
                throw new InvalidOperationException("Employee already assigned to this flight.");

            _linkRepo.AssignFlightToEmployee(employeeId, flightId);
        }

        public void RemoveCrewMember(int flightId, int employeeId)
        {
            _linkRepo.RemoveFlightFromEmployee(employeeId, flightId);
        }

        public List<Employee> GetFlightCrew(int flightId)
        {
            List<Employee> crew = new List<Employee>();
            List<int> employeeIds = _linkRepo.GetEmployeesByFlight(flightId);

            foreach (var id in employeeIds)
            {
                var emp = _employeeRepo.GetEmployeeById(id);
                if (emp != null) crew.Add(emp);
            }

            return crew;
        }

        public List<Flight> GetEmployeeSchedule(int employeeId)
        {
            if (employeeId <= 0) return new List<Flight>();
            return _linkRepo.GetFlightsByEmployee(employeeId);
        }

    }
}
