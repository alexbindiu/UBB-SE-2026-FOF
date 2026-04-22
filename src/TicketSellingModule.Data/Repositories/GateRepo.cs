
namespace TicketSellingModule.Data.Repositories
{
    public class GateRepo
    {
        private readonly DbConnectionFactory _connectionFactory;
        public GateRepo(DbConnectionFactory factory)
        {
            _connectionFactory = factory;
        }

        public List<Gate> GetAllGates()
        {
            List<Gate> AllGates = new List<Gate>();
            using(SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "Select id, name from gates";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using(SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string name = reader.GetString(1);
                            Gate NewGate = new Gate();
                            NewGate.Id = id;
                            NewGate.Name = name;
                            AllGates.Add(NewGate);
                        }
                    }
                }    
            }
            return AllGates;
        }
        public Gate GetGateById(int id)
        { 
            using(SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "Select id, name from gates where @id = id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using(SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if(reader.Read())
                        {
                            string name = reader.GetString(1);
                            Gate NewGate = new Gate();
                            NewGate.Id = id;
                            NewGate.Name = name;
                            return NewGate;
                        }
                    }
                }    
            }
            return null;
        }

        public int AddGate(Gate newGate)
        {
            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "Insert into gates(name) OUTPUT INSERTED.id VALUES (@name)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", newGate.Name);
                    int newId = (int)cmd.ExecuteScalar();
                    return newId;
                }
            }
        }

        public void DeleteGate(int id)
        {
            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string deleteFlights = "DELETE FROM Flights WHERE gate_id = @id";
                        using (SqlCommand cmd1 = new SqlCommand(deleteFlights, conn, transaction))
                        {
                            cmd1.Parameters.AddWithValue("@id", id);
                            cmd1.ExecuteNonQuery();
                        }

                        string deleteGate = "DELETE FROM gates WHERE id = @id";
                        using (SqlCommand cmd2 = new SqlCommand(deleteGate, conn, transaction))
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

        public void UpdateGate(Gate updatedGate)
        {
            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "Update gates SET name = @name where @id = id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", updatedGate.Id);
                    cmd.Parameters.AddWithValue("@name", updatedGate.Name);
                    cmd.ExecuteNonQuery();
                }    
            }
        }
    }
}
