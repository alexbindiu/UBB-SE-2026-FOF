using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TicketSellingModule.Domain;

namespace TicketSellingModule.Repo
{
    internal class AirportRepo
    {
        DbConnectionFactory _connectionFactory = new DbConnectionFactory();
        
        public List<Airport> GetAllAirports()
        {
            List<Airport> AllAirports = new List<Airport>();

            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "Select * from Airports";
                using(SqlCommand cmd = new SqlCommand(query,conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string city = reader.GetString(1);
                            Airport NewAirport = new Airport();
                            NewAirport.City = city;
                            NewAirport.Id = id;
                            AllAirports.Add(NewAirport);
                        }
                    }
                }
            }
            return AllAirports;
        }

        public Airport GetAirportById(int id)
        {

            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "Select * from Airports WHERE id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Airport foundAirport = new Airport();
                            string city = reader.GetString(1);
                            foundAirport.Id = id;
                            foundAirport.City = city;
                            return foundAirport;
                        }
                    }
                }
                return null;
            }
        }

        public int AddAirport(Airport newAirport)
        {
            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                string SqlInsert = "Insert into Airports (city) OUTPUT.INSERTED.id VALUES (@city)";
                using (SqlCommand cmd = new SqlCommand(SqlInsert, conn))
                {
                    cmd.Parameters.AddWithValue("@city", newAirport.City);
                    int newId = (int)cmd.ExecuteScalar();
                    return newId;
                }
            }
            
        }

        public void DeleteAirport(int id)
        {
            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "DELETE FROM Airports WHERE id = @id";
                using (SqlCommand cmd = new SqlCommand(query,conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }

        }
        public void UpdateAirport(Airport airport)
        {
            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "Update Airports SET city = @city where id = @id";
                using (SqlCommand cmd = new SqlCommand(query,conn))
                {
                    cmd.Parameters.AddWithValue("@city", airport.City);
                    cmd.Parameters.AddWithValue("@id", airport.Id);
                    cmd.ExecuteNonQuery();

                }
            }
        }
    }
}
