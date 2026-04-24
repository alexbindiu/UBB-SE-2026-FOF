namespace TicketSellingModule.Data.Repositories
{
    public class RunwayRepository(DatabaseConnectionFactory databaseConnectionFactory) : IRunwayRepository
    {
        private const string SelectAllQuery = "SELECT id, name, handle_time FROM runways";
        private const string SelectByIdQuery = "SELECT id, name, handle_time FROM runways WHERE id = @runwayId";
        private const string InsertQuery = "INSERT INTO runways(name, handle_time) OUTPUT INSERTED.id VALUES (@name, @handleTime)";
        private const string UpdateQuery = "UPDATE runways SET name = @name, handle_time = @handleTime WHERE id = @runwayId";
        private const string DeleteQuery = "DELETE FROM runways WHERE id = @runwayId";
        private const string DeleteRelatedFlightsQuery = "DELETE FROM Flights WHERE runway_id = @runwayId";

        public List<Runway> GetAllRunways()
        {
            List<Runway> runways = new List<Runway>();

            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(SelectAllQuery, databaseConnection);
            using SqlDataReader dataReader = sqlCommand.ExecuteReader();

            while (dataReader.Read())
            {
                runways.Add(new Runway
                {
                    Id = dataReader.GetInt32(0),
                    Name = dataReader.GetString(1),
                    HandleTime = dataReader.IsDBNull(2) ? 0 : dataReader.GetInt32(2)
                });
            }

            return runways;
        }

        public Runway? GetRunwayById(int runwayId)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(SelectByIdQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@runwayId", runwayId);

            using SqlDataReader dataReader = sqlCommand.ExecuteReader();

            if (dataReader.Read())
            {
                return new Runway
                {
                    Id = dataReader.GetInt32(0),
                    Name = dataReader.GetString(1),
                    HandleTime = dataReader.IsDBNull(2) ? 0 : dataReader.GetInt32(2)
                };
            }

            return null;
        }

        public int AddRunway(Runway newRunway)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(InsertQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@name", newRunway.Name);
            sqlCommand.Parameters.AddWithValue("@handleTime", newRunway.HandleTime);

            return (int)sqlCommand.ExecuteScalar();
        }

        public void UpdateRunway(Runway updatedRunway)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(UpdateQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@runwayId", updatedRunway.Id);
            sqlCommand.Parameters.AddWithValue("@name", updatedRunway.Name);
            sqlCommand.Parameters.AddWithValue("@handleTime", updatedRunway.HandleTime);

            sqlCommand.ExecuteNonQuery();
        }

        public void DeleteRunwayUsingId(int runwayId)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlTransaction databaseTransaction = databaseConnection.BeginTransaction();
            try
            {
                using SqlCommand deleteFlightsCommand = new SqlCommand(DeleteRelatedFlightsQuery, databaseConnection, databaseTransaction);
                deleteFlightsCommand.Parameters.AddWithValue("@runwayId", runwayId);
                deleteFlightsCommand.ExecuteNonQuery();

                using SqlCommand deleteRunwayCommand = new SqlCommand(DeleteQuery, databaseConnection, databaseTransaction);
                deleteRunwayCommand.Parameters.AddWithValue("@runwayId", runwayId);
                deleteRunwayCommand.ExecuteNonQuery();

                databaseTransaction.Commit();
            }
            catch
            {
                databaseTransaction.Rollback();
                throw;
            }
        }
    }
}
