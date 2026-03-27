using Microsoft.Data.SqlClient;

namespace TicketSellingModule.Repo
{
    public class DbConnectionFactory
    {
        // This holds your map to the database safely inside the class
        private readonly string _connectionString;

        public DbConnectionFactory()
        {
            // This is the exact string we verified earlier for your specific machine
            _connectionString = "Server=DESKTOP-K094GD0\\SQLEXPRESS;Database=AirportDB;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        // Every repository will call this method when it needs to talk to the database
        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}