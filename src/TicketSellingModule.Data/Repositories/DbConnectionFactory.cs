
namespace TicketSellingModule.Data.Repositories
{
    public class DbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory()
        {
            //_connectionString = "Server=DESKTOP-1JCJMN6\\SQLEXPRESS;Database=AirportDB;Trusted_Connection=True;TrustServerCertificate=True;";
            _connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=AirportDB;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}