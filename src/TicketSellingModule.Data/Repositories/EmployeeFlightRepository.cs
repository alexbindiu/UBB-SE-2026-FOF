namespace TicketSellingModule.Data.Repositories
{
    public class EmployeeFlightRepository(DatabaseConnectionFactory databaseConnectionFactory) : IEmployeeFlightRepository
    {
        private const string InsertAssignmentQuery = "INSERT INTO Flight_employees (id_employee, id_flight) VALUES (@employeeId, @flightId)";
        private const string DeleteAssignmentQuery = "DELETE FROM Flight_employees WHERE id_employee = @employeeId AND id_flight = @flightId";
        private const string SelectFlightsByEmployeeQuery = "SELECT id_flight FROM Flight_employees WHERE id_employee = @employeeId";
        private const string SelectEmployeesByFlightQuery = "SELECT id_employee FROM Flight_employees WHERE id_flight = @flightId";
        private const string DeleteAllByFlightQuery = "DELETE FROM Flight_employees WHERE id_flight = @flightId";
        private const string DeleteAllByEmployeeQuery = "DELETE FROM Flight_employees WHERE id_employee = @employeeId";

        public void AssignFlightToEmployeesUsingIds(int employeeId, int flightId)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(InsertAssignmentQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@employeeId", employeeId);
            sqlCommand.Parameters.AddWithValue("@flightId", flightId);

            sqlCommand.ExecuteNonQuery();
        }

        public void RemoveFlightFromEmployeeUsingIds(int employeeId, int flightId)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(DeleteAssignmentQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@employeeId", employeeId);
            sqlCommand.Parameters.AddWithValue("@flightId", flightId);

            sqlCommand.ExecuteNonQuery();
        }

        public List<int> GetFlightsByEmployeeId(int employeeId)
        {
            List<int> flightIdentifiers = new List<int>();

            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(SelectFlightsByEmployeeQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@employeeId", employeeId);

            using SqlDataReader dataReader = sqlCommand.ExecuteReader();

            while (dataReader.Read())
            {
                flightIdentifiers.Add(dataReader.GetInt32(0));
            }

            return flightIdentifiers;
        }

        public List<int> GetEmployeesByFlightId(int flightId)
        {
            List<int> employeeIdentifiers = new List<int>();

            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(SelectEmployeesByFlightQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@flightId", flightId);

            using SqlDataReader dataReader = sqlCommand.ExecuteReader();

            while (dataReader.Read())
            {
                employeeIdentifiers.Add(dataReader.GetInt32(0));
            }

            return employeeIdentifiers;
        }

        public void RemoveAllByFlightId(int flightId)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(DeleteAllByFlightQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@flightId", flightId);

            sqlCommand.ExecuteNonQuery();
        }

        public void RemoveAllByEmployeeId(int employeeId)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(DeleteAllByEmployeeQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@employeeId", employeeId);

            sqlCommand.ExecuteNonQuery();
        }
    }
}