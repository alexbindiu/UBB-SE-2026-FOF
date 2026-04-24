namespace TicketSellingModule.Data.Repositories
{
    public class EmployeeRepository(DatabaseConnectionFactory databaseConnectionFactory) : IEmployeeRepository
    {
        private const string SelectAllQuery = "SELECT id, name, role_id, birthday, salary, hiring_date FROM Employees";
        private const string SelectByIdQuery = "SELECT id, name, role_id, birthday, salary, hiring_date FROM Employees WHERE id = @employeeId";
        private const string InsertQuery = @"INSERT INTO Employees (name, role_id, birthday, salary, hiring_date) 
                                             OUTPUT INSERTED.id 
                                             VALUES (@name, @roleId, @birthday, @salary, @hiringDate)";
        private const string UpdateQuery = @"UPDATE Employees SET 
                                             name = @name, 
                                             role_id = @roleId, 
                                             birthday = @birthday, 
                                             salary = @salary, 
                                             hiring_date = @hiringDate 
                                             WHERE id = @employeeId";
        private const string DeleteQuery = "DELETE FROM Employees WHERE id = @employeeId";

        public List<Employee> GetAllEmployees()
        {
            List<Employee> allEmployees = new List<Employee>();

            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(SelectAllQuery, databaseConnection);
            using SqlDataReader dataReader = sqlCommand.ExecuteReader();

            while (dataReader.Read())
            {
                allEmployees.Add(new Employee
                {
                    Id = dataReader.GetInt32(0),
                    Name = dataReader.GetString(1),
                    Role = (EmployeeRole)dataReader.GetInt32(2),
                    Birthday = DateOnly.FromDateTime(dataReader.GetDateTime(3)),
                    Salary = dataReader.GetInt32(4),
                    HiringDate = DateOnly.FromDateTime(dataReader.GetDateTime(5))
                });
            }

            return allEmployees;
        }

        public Employee? GetEmployeeById(int employeeId)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(SelectByIdQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@employeeId", employeeId);

            using SqlDataReader dataReader = sqlCommand.ExecuteReader();

            if (dataReader.Read())
            {
                return new Employee
                {
                    Id = dataReader.GetInt32(0),
                    Name = dataReader.GetString(1),
                    Role = (EmployeeRole)dataReader.GetInt32(2),
                    Birthday = DateOnly.FromDateTime(dataReader.GetDateTime(3)),
                    Salary = dataReader.GetInt32(4),
                    HiringDate = DateOnly.FromDateTime(dataReader.GetDateTime(5))
                };
            }

            return null;
        }

        public int AddEmployee(Employee newEmployee)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(InsertQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@name", newEmployee.Name);
            sqlCommand.Parameters.AddWithValue("@roleId", (int)newEmployee.Role);
            sqlCommand.Parameters.AddWithValue("@salary", newEmployee.Salary);
            sqlCommand.Parameters.AddWithValue("@birthday", newEmployee.Birthday.ToDateTime(TimeOnly.MinValue));
            sqlCommand.Parameters.AddWithValue("@hiringDate", newEmployee.HiringDate.ToDateTime(TimeOnly.MinValue));

            int generatedIdentifier = (int)sqlCommand.ExecuteScalar();
            return generatedIdentifier;
        }

        public void UpdateEmployee(Employee updatedEmployee)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(UpdateQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@employeeId", updatedEmployee.Id);
            sqlCommand.Parameters.AddWithValue("@name", updatedEmployee.Name);
            sqlCommand.Parameters.AddWithValue("@roleId", (int)updatedEmployee.Role);
            sqlCommand.Parameters.AddWithValue("@salary", updatedEmployee.Salary);
            sqlCommand.Parameters.AddWithValue("@birthday", updatedEmployee.Birthday.ToDateTime(TimeOnly.MinValue));
            sqlCommand.Parameters.AddWithValue("@hiringDate", updatedEmployee.HiringDate.ToDateTime(TimeOnly.MinValue));

            sqlCommand.ExecuteNonQuery();
        }

        public void DeleteEmployee(int employeeId)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlTransaction databaseTransaction = databaseConnection.BeginTransaction();
            try
            {
                using SqlCommand deleteEmployeeCommand = new SqlCommand(DeleteQuery, databaseConnection, databaseTransaction);
                deleteEmployeeCommand.Parameters.AddWithValue("@employeeId", employeeId);
                deleteEmployeeCommand.ExecuteNonQuery();

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