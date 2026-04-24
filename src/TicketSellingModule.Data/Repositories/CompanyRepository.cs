namespace TicketSellingModule.Data.Repositories
{
    public class CompanyRepository(DatabaseConnectionFactory databaseConnectionFactory) : ICompanyRepository
    {
        private const string SelectAllQuery = "SELECT id, name FROM Companies";
        private const string SelectByIdQuery = "SELECT id, name FROM Companies WHERE id = @companyId";
        private const string InsertQuery = "INSERT INTO Companies (name) OUTPUT INSERTED.id VALUES (@name)";
        private const string DeleteQuery = "DELETE FROM Companies WHERE id = @companyId";
        private const string UpdateQuery = "UPDATE Companies SET name = @name WHERE id = @companyId";

        public List<Company> GetAllCompanies()
        {
            List<Company> allCompanies = new List<Company>();
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(SelectAllQuery, databaseConnection);
            using SqlDataReader dataReader = sqlCommand.ExecuteReader();

            while (dataReader.Read())
            {
                allCompanies.Add(new Company
                {
                    Id = dataReader.GetInt32(0),
                    Name = dataReader.GetString(1)
                });
            }
            return allCompanies;
        }
        public Company? GetCompanyById(int companyId)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(SelectByIdQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@companyId", companyId);

            using SqlDataReader dataReader = sqlCommand.ExecuteReader();

            if (dataReader.Read())
            {
                return new Company
                {
                    Id = dataReader.GetInt32(0),
                    Name = dataReader.GetString(1)
                };
            }

            return null;
        }

        public int AddCompany(Company newCompany)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(InsertQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@name", newCompany.Name);

            int generatedId = (int)sqlCommand.ExecuteScalar();
            return generatedId;
        }

        public void DeleteCompanyUsingId(int companyId)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(DeleteQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@companyId", companyId);

            sqlCommand.ExecuteNonQuery();
        }
        public void UpdateCompany(Company companyToUpdate)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(UpdateQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@companyId", companyToUpdate.Id);
            sqlCommand.Parameters.AddWithValue("@name", companyToUpdate.Name);

            sqlCommand.ExecuteNonQuery();
        }
    }
}
