namespace TicketSellingModule.Data.Repositories
{
    public class DatabaseConnectionFactory
    {
        private readonly string connectionString;

        public DatabaseConnectionFactory()
        {
            // connectionString = "Server=DESKTOP-1JCJMN6\\SQLEXPRESS;Database=AirportDB;Trusted_Connection=True;TrustServerCertificate=True;";
            connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=AirportDB;Trusted_Connection=True;TrustServerCertificate=True;";
            // connectionString = @"Server=DESKTOP\SQLEXPRESS;Database=AirportDB;Trusted_Connection=True;TrustServerCertificate=True;";
            // connectionString = @"Server=IONUT\SQLEXPRESS;Database=AirportDB;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}