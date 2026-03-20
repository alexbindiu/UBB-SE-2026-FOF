using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using TicketSellingModule.Domain;


namespace TicketSellingModule.Repo
{
    internal class RunwaysRepo
    {
        private string _connectionString = "Server=localhost\\SQLEXPRESS;Database=AirportDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public List<Runway> GetRunways()
        {
            List<Runway> runways = new List<Runway>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "Select * from Runways";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Runway runway = new Runway
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                HandleTime = reader.GetInt32(2)
                            };
                            runways.Add(runway);
                        }
                    }
                }
            }
            return runways;
        }

        public 
    }
}
