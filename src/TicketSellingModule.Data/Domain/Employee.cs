namespace TicketSellingModule.Data.Domain
{
    public enum EmployeeRole
    {
        Other = 0,
        Pilot = 1,
        CoPilot = 2,
        FlightAttendant = 3,
        FlightDispatcher = 4
    }

    public class Employee
    {
        public int Id { get; set; }
        public EmployeeRole Role { get; set; }
        public string Name { get; set; }
        public DateOnly Birthday { get; set; }
        public DateOnly HiringDate { get; set; }
        public int Salary { get; set; }
    }
}
