using TicketSellingModule.Data.Repositories.Interfaces;

namespace TicketSellingModule.Data.Repositories
{
    public class GateRepository(DatabaseConnectionFactory databaseConnectionFactory) : IGateRepository
    {
        private const string SelectAllQuery = "SELECT id, name FROM Gates";
        private const string SelectByIdQuery = "SELECT id, name FROM Gates WHERE id = @gateId";
        private const string InsertQuery = "INSERT INTO Gates (name) OUTPUT INSERTED.id VALUES (@name)";
        private const string UpdateQuery = "UPDATE Gates SET name = @name WHERE id = @gateId";
        private const string DeleteQuery = "DELETE FROM Gates WHERE id = @gateId";
        private const string DeleteRelatedFlightsQuery = "DELETE FROM Flights WHERE gate_id = @gateId";

        public List<Gate> GetAllGates()
        {
            List<Gate> gates = new List<Gate>();

            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(SelectAllQuery, databaseConnection);
            using SqlDataReader dataReader = sqlCommand.ExecuteReader();

            while (dataReader.Read())
            {
                gates.Add(new Gate
                {
                    Id = dataReader.GetInt32(0),
                    Name = dataReader.GetString(1)
                });
            }

            return gates;
        }
        public Gate? GetGateById(int gateId)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(SelectByIdQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@gateId", gateId);

            using SqlDataReader dataReader = sqlCommand.ExecuteReader();

            if (dataReader.Read())
            {
                return new Gate
                {
                    Id = dataReader.GetInt32(0),
                    Name = dataReader.GetString(1)
                };
            }

            return null;
        }

        public int AddGate(Gate newGate)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(InsertQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@name", newGate.Name);

            return (int)sqlCommand.ExecuteScalar();
        }

        public void DeleteGateUsingId(int gateId)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlTransaction databaseTransaction = databaseConnection.BeginTransaction();
            try
            {
                using SqlCommand deleteFlightsCommand = new SqlCommand(DeleteRelatedFlightsQuery, databaseConnection, databaseTransaction);
                deleteFlightsCommand.Parameters.AddWithValue("@gateId", gateId);
                deleteFlightsCommand.ExecuteNonQuery();

                using SqlCommand deleteGateCommand = new SqlCommand(DeleteQuery, databaseConnection, databaseTransaction);
                deleteGateCommand.Parameters.AddWithValue("@gateId", gateId);
                deleteGateCommand.ExecuteNonQuery();

                databaseTransaction.Commit();
            }
            catch
            {
                databaseTransaction.Rollback();
                throw;
            }
        }

        public void UpdateGate(Gate updatedGate)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(UpdateQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@gateId", updatedGate.Id);
            sqlCommand.Parameters.AddWithValue("@name", updatedGate.Name);

            sqlCommand.ExecuteNonQuery();
        }
    }
}
