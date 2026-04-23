using TicketSellingModule.Data.Repositories.Interfaces;

namespace TicketSellingModule.Data.Repositories
{
    public class CompanyRepo : ICompanyRepo
    {
        private readonly DbConnectionFactory connectionFactory;
        public CompanyRepo(DbConnectionFactory factory)
        {
            connectionFactory = factory;
        }

        public List<Company> GetAllCompanies()
        {
            List<Company> allCompanies = new List<Company>();
            using (SqlConnection conn = connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "Select id, name from companies";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string name = reader.GetString(1);
                            Company newCompany = new Company();
                            newCompany.Name = name;
                            newCompany.Id = id;
                            allCompanies.Add(newCompany);
                        }
                    }
                }
                return allCompanies;
            }
        }
        public Company GetCompanyById(int id)
        {
            using (SqlConnection conn = connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "Select id, name from companies where @id = id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string name = reader.GetString(1);
                            Company newCompany = new Company();
                            newCompany.Name = name;
                            newCompany.Id = id;
                            return newCompany;
                        }
                    }
                }
                return null;
            }
        }

        public int AddNewCompany(Company company)
        {
            using (SqlConnection conn = connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "Insert into Companies (name) OUTPUT INSERTED.id VALUES (@name)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", company.Name);
                    int newId = (int)cmd.ExecuteScalar();
                    return newId;
                }
            }
        }

        public void DeleteCompany(int id)
        {
            using (SqlConnection conn = connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "Delete from Companies Where @id = id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void UpdateCompany(Company company)
        {
            using (SqlConnection conn = connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "Update Companies SET name = @name Where @id = id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", company.Id);
                    cmd.Parameters.AddWithValue("@name", company.Name);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
