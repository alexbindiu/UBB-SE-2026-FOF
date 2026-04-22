//using System.Collections.Generic;
//using Microsoft.Data.SqlClient;

//namespace TicketSellingModule.Repo
//{
//    public class FlightEmployeeRepo
//    {
//        private readonly DbConnectionFactory _connectionFactory;

//        public FlightEmployeeRepo(DbConnectionFactory factory)
//        {
//            _connectionFactory = factory;
//        }

//        public void AssignFlightToEmployee(int employeeId, int flightId)
//        {
//            using (SqlConnection conn = _connectionFactory.GetConnection())
//            {
//                conn.Open();
//                string query = "INSERT INTO Flight_employees (id_employee, id_flight) VALUES (@empId, @flightId)";
//                using (SqlCommand cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@empId", employeeId);
//                    cmd.Parameters.AddWithValue("@flightId", flightId);
//                    cmd.ExecuteNonQuery();
//                }
//            }
//        }

//        public void RemoveFlightFromEmployee(int employeeId, int flightId)
//        {
//            using (SqlConnection conn = _connectionFactory.GetConnection())
//            {
//                conn.Open();
//                string query = "DELETE FROM Flight_employees WHERE id_employee = @empId AND id_flight = @flightId";
//                using (SqlCommand cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@empId", employeeId);
//                    cmd.Parameters.AddWithValue("@flightId", flightId);
//                    cmd.ExecuteNonQuery();
//                }
//            }
//        }

//        public List<int> GetFlightsByEmployee(int employeeId)
//        {
//            List<int> flightIds = new List<int>();
//            using (SqlConnection conn = _connectionFactory.GetConnection())
//            {
//                conn.Open();
//                string query = "SELECT id_flight FROM Flight_employees WHERE id_employee = @empId";
//                using (SqlCommand cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@empId", employeeId);
//                    using (SqlDataReader reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            flightIds.Add(reader.GetInt32(0));
//                        }
//                    }
//                }
//            }
//            return flightIds;
//        }
//        public List<int> GetEmployeesByFlight(int flightId)
//        {
//            List<int> employeeIds = new List<int>();
//            using (SqlConnection conn = _connectionFactory.GetConnection())
//            {
//                conn.Open();
//                string query = "select id_employee from Flight_employees where id_flight = @flightId";
                
//                using (SqlCommand cmd = new SqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("flightId", flightId);
//                    using (SqlDataReader r = cmd.ExecuteReader())
//                    {
//                        while (r.Read())
//                        {
//                            employeeIds.Add(r.GetInt32(0));
//                        }
//                    }
//                }
//            }
//            return employeeIds;
//        }

//    }

//}