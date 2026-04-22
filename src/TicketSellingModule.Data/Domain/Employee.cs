
namespace TicketSellingModule.Data.Domain
{
    public class Employee
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        public DateOnly Birthday { get; set; }
        public DateOnly HiringDate { get; set; }
        public int Salary { get; set; }

       
    }
}
