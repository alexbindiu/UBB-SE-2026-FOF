using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using TicketSellingModule.Domain;

namespace TicketSellingModule.Repo
{
    internal class RunwayRepo
    {
        DbConnectionFactory _connectionFactory = new DbConnectionFactory();

        public List<Runway> GetAllRunways()
        {
            List<Runway> allRunways = new List<Runway>();
            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "SELECT id, name FROM runways";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string name = reader.GetString(1);
                            Runway newRunway = new Runway { Id = id, Name = name };
                            allRunways.Add(newRunway);
                        }
                    }
                }
            }
            return allRunways;
        }

        public Runway GetRunwayById(int id)
        {
            using (SqlConnection conn = _connectionFactory.GetConnection())
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
            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "INSERT INTO runways(name) OUTPUT INSERTED.id VALUES (@name)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", newRunway.Name);
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public void UpdateRunway(Runway updatedRunway)
        {
            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "UPDATE runways SET name = @name WHERE id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", updatedRunway.Id);
                    cmd.Parameters.AddWithValue("@name", updatedRunway.Name);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteRunway(int id)
        {
            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "DELETE FROM runways WHERE id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
