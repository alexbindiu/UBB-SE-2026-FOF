namespace TicketSellingModule.Data.Repositories
{
    public class AirportRepository(DatabaseConnectionFactory databaseConnectionFactory) : IAirportRepository
    {
        private const string SelectAllQuery = "SELECT id, city, name, code FROM Airports";
        private const string SelectByIdQuery = "SELECT id, city, name, code FROM Airports WHERE id = @airportId";
        private const string InsertQuery = "INSERT INTO Airports (city, name, code) OUTPUT INSERTED.id VALUES (@city, @name, @code)";
        private const string UpdateQuery = "UPDATE Airports SET city = @city, name = @name, code = @code WHERE id = @airportId";
        private const string DeleteFlightsQuery = "DELETE FROM Flights WHERE route_id IN (SELECT id FROM Routes WHERE airport_id = @airportId)";
        private const string DeleteRoutesQuery = "DELETE FROM Routes WHERE airport_id = @airportId";
        private const string DeleteAirportQuery = "DELETE FROM Airports WHERE id = @airportId";

        public List<Airport> GetAllAirports()
        {
            List<Airport> allAirports = new List<Airport>();

            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(SelectAllQuery, databaseConnection);
            using SqlDataReader dataReader = sqlCommand.ExecuteReader();

            while (dataReader.Read())
            {
                allAirports.Add(new Airport
                {
                    Id = dataReader.GetInt32(0),
                    City = dataReader.GetString(1),
                    AirportName = dataReader.GetString(2),
                    AirportCode = dataReader.GetString(3)
                });
            }

            return allAirports;
        }

        public Airport? GetAirportById(int airportId)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(SelectByIdQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@airportId", airportId);

            using SqlDataReader dataReader = sqlCommand.ExecuteReader();

            if (dataReader.Read())
            {
                return new Airport
                {
                    Id = dataReader.GetInt32(0),
                    City = dataReader.GetString(1),
                    AirportName = dataReader.GetString(2),
                    AirportCode = dataReader.GetString(3)
                };
            }

            return null;
        }

        public int AddAirport(Airport newAirport)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(InsertQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@city", newAirport.City);
            sqlCommand.Parameters.AddWithValue("@name", newAirport.AirportName);
            sqlCommand.Parameters.AddWithValue("@code", newAirport.AirportCode);

            return (int)sqlCommand.ExecuteScalar();
        }

        public void DeleteAirportUsingId(int airportId)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlTransaction databaseTransaction = databaseConnection.BeginTransaction();
            try
            {
                using SqlCommand deleteFlightsCommand = new SqlCommand(DeleteFlightsQuery, databaseConnection, databaseTransaction);
                deleteFlightsCommand.Parameters.AddWithValue("@airportId", airportId);
                deleteFlightsCommand.ExecuteNonQuery();

                using SqlCommand deleteRoutesCommand = new SqlCommand(DeleteRoutesQuery, databaseConnection, databaseTransaction);
                deleteRoutesCommand.Parameters.AddWithValue("@airportId", airportId);
                deleteRoutesCommand.ExecuteNonQuery();

                using SqlCommand deleteAirportCommand = new SqlCommand(DeleteAirportQuery, databaseConnection, databaseTransaction);
                deleteAirportCommand.Parameters.AddWithValue("@airportId", airportId);
                deleteAirportCommand.ExecuteNonQuery();

                databaseTransaction.Commit();
            }
            catch
            {
                databaseTransaction.Rollback();
                throw;
            }
        }

        public void UpdateAirport(Airport airportToUpdate)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(UpdateQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@city", airportToUpdate.City);
            sqlCommand.Parameters.AddWithValue("@name", airportToUpdate.AirportName);
            sqlCommand.Parameters.AddWithValue("@code", airportToUpdate.AirportCode);
            sqlCommand.Parameters.AddWithValue("@airportId", airportToUpdate.Id);

            sqlCommand.ExecuteNonQuery();
        }
    }
}
