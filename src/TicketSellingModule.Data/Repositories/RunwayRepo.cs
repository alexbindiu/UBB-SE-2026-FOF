namespace TicketSellingModule.Data.Repositories
{
    public class RunwayRepo
    {
        private readonly DbConnectionFactory connectionFactory;
        public RunwayRepo(DbConnectionFactory factory)
        {
            connectionFactory = factory;
        }

        public List<Runway> GetAllRunways()
        {
            List<Runway> allRunways = new List<Runway>();
            using (SqlConnection conn = connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "SELECT id, name, handle_time FROM runways";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string name = reader.GetString(1);
                            int handleTime = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);

                            Runway newRunway = new Runway { Id = id, Name = name, HandleTime = handleTime };
                            allRunways.Add(newRunway);
                        }
                    }
                }
            }
            return allRunways;
        }

        public Runway GetRunwayById(int id)
        {
            using (SqlConnection conn = connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "SELECT id, name FROM runways WHERE id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string name = reader.GetString(1);
                            return new Runway { Id = id, Name = name };
                        }
                    }
                }
            }
            return null;
        }

        public int AddRunway(Runway newRunway)
        {
            using (SqlConnection conn = connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "INSERT INTO runways(name, handle_time) OUTPUT INSERTED.id VALUES (@name, @handleTime)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", newRunway.Name);
                    cmd.Parameters.AddWithValue("@handleTime", newRunway.HandleTime);
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public void UpdateRunway(Runway updatedRunway)
        {
            using (SqlConnection conn = connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "UPDATE runways SET name = @name, handle_time = @handleTime WHERE id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", updatedRunway.Id);
                    cmd.Parameters.AddWithValue("@name", updatedRunway.Name);
                    cmd.Parameters.AddWithValue("@handleTime", updatedRunway.HandleTime);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteRunway(int id)
        {
            using (SqlConnection conn = connectionFactory.GetConnection())
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string deleteFlights = "DELETE FROM Flights WHERE runway_id = @id";
                        using (SqlCommand cmd1 = new SqlCommand(deleteFlights, conn, transaction))
                        {
                            cmd1.Parameters.AddWithValue("@id", id);
                            cmd1.ExecuteNonQuery();
                        }

                        string deleteRunway = "DELETE FROM runways WHERE id = @id";
                        using (SqlCommand cmd2 = new SqlCommand(deleteRunway, conn, transaction))
                        {
                            cmd2.Parameters.AddWithValue("@id", id);
                            cmd2.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
