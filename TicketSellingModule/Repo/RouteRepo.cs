using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketSellingModule.Domain;

namespace TicketSellingModule.Repo
{
    public class RouteRepo
    {
        private readonly DbConnectionFactory _connectionFactory;
        public RouteRepo(DbConnectionFactory factory)
        {
            _connectionFactory = factory;
        }

        public List<Route> GetAllRoutes()
        {
            List<Route> AllRoutes = new List<Route>();
            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "Select id, company_id, route_type, airport_id, reccurence_interval, start_date, end_date, departure_time, arrival_time, capacity from Routes";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using(SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Route newRoute = new Route();
                            newRoute.Id = reader.GetInt32(0);
                            newRoute.CompanyId = reader.GetInt32(1);
                            newRoute.RouteType = reader.GetString(2);
                            newRoute.AirportId = reader.GetInt32(3);
                            newRoute.RecurrenceInterval = reader.GetInt32(4);
                            newRoute.StartDate = DateOnly.FromDateTime(reader.GetDateTime(5));
                            newRoute.EndDate = DateOnly.FromDateTime(reader.GetDateTime(6));
                            // 1. Get the raw value as a string (e.g., "01/01/1900 2:35:00 pm")
                            string rawDepTime = reader.GetValue(7).ToString();
                            string rawArrTime = reader.GetValue(8).ToString();

                            // 2. Use DateTime.Parse first (it handles the 1900 date easily) 
                            // and then convert the result to TimeOnly
                            newRoute.DepartureTime = TimeOnly.FromDateTime(DateTime.Parse(rawDepTime));
                            newRoute.ArrivalTime = TimeOnly.FromDateTime(DateTime.Parse(rawArrTime));
                            AllRoutes.Add(newRoute);
                        }
                    }
                }
                return AllRoutes;
            }
        }
        public int AddRoute(Route newRoute)
        {
            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                string query = @"INSERT INTO Routes 
                                 (company_id, route_type, airport_id, reccurence_interval, start_date, end_date, departure_time, arrival_time, capacity) 
                                 OUTPUT INSERTED.id 
                                 VALUES (@company_id, @route_type, @airport_id, @reccurence_interval, @start_date, @end_date, @departure_time, @arrival_time, @capacity)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@company_id", newRoute.Company.Id);
                    cmd.Parameters.AddWithValue("@airport_id", newRoute.Airport.Id);

                    cmd.Parameters.AddWithValue("@route_type", newRoute.RouteType);
                    cmd.Parameters.AddWithValue("@reccurence_interval", newRoute.RecurrenceInterval);
                    cmd.Parameters.AddWithValue("@capacity", newRoute.Capacity);

                    cmd.Parameters.AddWithValue("@start_date", newRoute.StartDate.ToDateTime(TimeOnly.MinValue));
                    cmd.Parameters.AddWithValue("@end_date", newRoute.EndDate.ToDateTime(TimeOnly.MinValue));
                    cmd.Parameters.AddWithValue("@departure_time", newRoute.DepartureTime.ToTimeSpan());
                    cmd.Parameters.AddWithValue("@arrival_time", newRoute.ArrivalTime.ToTimeSpan());

                    int newId = (int)cmd.ExecuteScalar();
                    return newId;
                }
            }
        }

        public void DeleteRoute(int id)
        {
            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "DELETE FROM Routes WHERE id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateRoute(Route updatedRoute)
        {
            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                string query = @"UPDATE Routes SET 
                                 company_id = @company_id, 
                                 route_type = @route_type, 
                                 airport_id = @airport_id, 
                                 reccurence_interval = @reccurence_interval, 
                                 start_date = @start_date, 
                                 end_date = @end_date, 
                                 departure_time = @departure_time, 
                                 arrival_time = @arrival_time, 
                                 capacity = @capacity 
                                 WHERE id = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", updatedRoute.Id);

                    cmd.Parameters.AddWithValue("@company_id", updatedRoute.Company.Id);
                    cmd.Parameters.AddWithValue("@airport_id", updatedRoute.Airport.Id);

                    cmd.Parameters.AddWithValue("@route_type", updatedRoute.RouteType);
                    cmd.Parameters.AddWithValue("@reccurence_interval", updatedRoute.RecurrenceInterval);
                    cmd.Parameters.AddWithValue("@capacity", updatedRoute.Capacity);

                    cmd.Parameters.AddWithValue("@start_date", updatedRoute.StartDate.ToDateTime(TimeOnly.MinValue));
                    cmd.Parameters.AddWithValue("@end_date", updatedRoute.EndDate.ToDateTime(TimeOnly.MinValue));
                    cmd.Parameters.AddWithValue("@departure_time", updatedRoute.DepartureTime.ToTimeSpan());
                    cmd.Parameters.AddWithValue("@arrival_time", updatedRoute.ArrivalTime.ToTimeSpan());

                    cmd.ExecuteNonQuery();
                }
            }
        }
        public Route GetRouteById(int id)
        {
            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                string query = @"SELECT id, company_id, route_type, airport_id, reccurence_interval, 
                                        start_date, end_date, departure_time, arrival_time, capacity 
                                 FROM Routes 
                                 WHERE id = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Route foundRoute = new Route();

                            foundRoute.Id = reader.GetInt32(0);

                            foundRoute.CompanyId = reader.GetInt32(1);

                            foundRoute.RouteType = reader.GetString(2);

                            foundRoute.AirportId = reader.GetInt32(3);

                            foundRoute.RecurrenceInterval = reader.GetInt32(4);

                            foundRoute.StartDate = DateOnly.FromDateTime(reader.GetDateTime(5));
                            foundRoute.EndDate = DateOnly.FromDateTime(reader.GetDateTime(6));
                            foundRoute.DepartureTime = TimeOnly.FromDateTime(reader.GetDateTime(7));
                            foundRoute.ArrivalTime = TimeOnly.FromDateTime(reader.GetDateTime(8));

                            foundRoute.Capacity = reader.GetInt32(9);

                            return foundRoute;
                        }
                    }
                }
            }
            return null;
        }
    }
}
