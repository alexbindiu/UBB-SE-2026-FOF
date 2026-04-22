namespace TicketSellingModule.Data.Repositories
{
    public class AirportRepo
    {
        private readonly DbConnectionFactory connectionFactory;

        public AirportRepo(DbConnectionFactory factory)
        {
            connectionFactory = factory;
        }

        public List<Airport> GetAllAirports()
        {
            List<Airport> allAirports = new List<Airport>();

            using (SqlConnection conn = connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "SELECT id, city, name, code FROM Airports";
                using (SqlCommand cmd = new SqlCommand(query,conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string city = reader.GetString(1);
                            string name = reader.GetString(2);
                            string code = reader.GetString(3);
                            Airport newAirport = new Airport();
                            newAirport.City = city;
                            newAirport.Id = id;
                            newAirport.AirportName = name;
                            newAirport.AirportCode = code;
                            allAirports.Add(newAirport);
                        }
                    }
                }
            }
            return allAirports;
        }

        public Airport GetAirportById(int id)
        {
            using (SqlConnection conn = connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "Select id, city, name, code  from Airports WHERE id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Airport foundAirport = new Airport();
                            string city = reader.GetString(1);
                            string name = reader.GetString(2);
                            string code = reader.GetString(3);
                            foundAirport.Id = id;
                            foundAirport.City = city;
                            foundAirport.AirportCode = code;
                            foundAirport.AirportName = name;
                            return foundAirport;
                        }
                    }
                }
                return null;
            }
        }

        public int AddAirport(Airport newAirport)
        {
            using (SqlConnection conn = connectionFactory.GetConnection())
            {
                conn.Open();
                string sqlInsert = "Insert into Airports (city, name, code) OUTPUT INSERTED.id VALUES (@city, @name, @code)";
                using (SqlCommand cmd = new SqlCommand(sqlInsert, conn))
                {
                    cmd.Parameters.AddWithValue("@city", newAirport.City);
                    cmd.Parameters.AddWithValue("@name", newAirport.AirportName);
                    cmd.Parameters.AddWithValue("@code", newAirport.AirportCode);

                    int newId = (int)cmd.ExecuteScalar();
                    return newId;
                }
            }
        }

        public void DeleteAirport(int id)
        {
            using (SqlConnection conn = connectionFactory.GetConnection())
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string deleteFlights = @"DELETE FROM Flights 
                                         WHERE route_id IN (SELECT id FROM Routes WHERE airport_id = @id)";
                        using (SqlCommand cmd1 = new SqlCommand(deleteFlights, conn, transaction))
                        {
                            cmd1.Parameters.AddWithValue("@id", id);
                            cmd1.ExecuteNonQuery();
                        }

                        string deleteRoutes = "DELETE FROM Routes WHERE airport_id = @id";
                        using (SqlCommand cmd2 = new SqlCommand(deleteRoutes, conn, transaction))
                        {
                            cmd2.Parameters.AddWithValue("@id", id);
                            cmd2.ExecuteNonQuery();
                        }

                        string deleteAirport = "DELETE FROM Airports WHERE id = @id";
                        using (SqlCommand cmd3 = new SqlCommand(deleteAirport, conn, transaction))
                        {
                            cmd3.Parameters.AddWithValue("@id", id);
                            cmd3.ExecuteNonQuery();
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
        public void UpdateAirport(Airport airport)
        {
            using (SqlConnection conn = connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "Update Airports SET city = @city, name = @name, code = @code where id = @id";
                using (SqlCommand cmd = new SqlCommand(query,conn))
                {
                    cmd.Parameters.AddWithValue("@city", airport.City);
                    cmd.Parameters.AddWithValue("@name", airport.AirportName);
                    cmd.Parameters.AddWithValue("@code", airport.AirportCode);
                    cmd.Parameters.AddWithValue("@id", airport.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
