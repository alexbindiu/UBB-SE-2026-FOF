using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using TicketSellingModule.Domain;

namespace TicketSellingModule.Repo
{
    public class FlightRepo
    {
        private readonly DbConnectionFactory _connectionFactory;

        public FlightRepo(DbConnectionFactory factory)
        {
            _connectionFactory = factory;
        }

        public List<Flight> GetAll()
        {
            List<Flight> flights = new List<Flight>();

            using (SqlConnection connection = _connectionFactory.GetConnection())
            {
                connection.Open();
                string sql = "SELECT id, date, flight_number, route_id, runway_id, gate_id FROM Flights";

                using (SqlCommand command = new SqlCommand(sql, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Flight flight = new Flight
                        {
                            Id = reader.GetInt32(0),
                            Date = reader.GetDateTime(1),
                            FlightNumber = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            RouteId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                            RunwayId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                            GateId = reader.IsDBNull(5) ? 0 : reader.GetInt32(5)
                        };
                        flights.Add(flight);
                    }
                }
            }
            return flights;
        }

        public Flight GetById(int id)
        {
            Flight flight = null;

            using (SqlConnection connection = _connectionFactory.GetConnection())
            {
                connection.Open();
                string sql = "SELECT id, date, flight_number, route_id, runway_id, gate_id FROM Flights WHERE id = @id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            flight = new Flight
                            {
                                Id = reader.GetInt32(0),
                                Date = reader.GetDateTime(1),
                                FlightNumber = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                RouteId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                                RunwayId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                GateId = reader.IsDBNull(5) ? 0 : reader.GetInt32(5)
                            };
                        }
                    }
                }
            }
            return flight;
        }

        public List<Flight> GetFlightsByRoute(int routeId)
        {
            List<Flight> flights = new List<Flight>();

            using (SqlConnection connection = _connectionFactory.GetConnection())
            {
                connection.Open();
                string sql = "SELECT id, date, flight_number, route_id, runway_id, gate_id FROM Flights WHERE route_id = @route_id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@route_id", routeId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Flight flight = new Flight
                            {
                                Id = reader.GetInt32(0),
                                Date = reader.GetDateTime(1),
                                FlightNumber = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                RouteId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                                RunwayId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                GateId = reader.IsDBNull(5) ? 0 : reader.GetInt32(5)
                            };
                            flights.Add(flight);
                        }
                    }
                }
            }
            return flights;
        }


        public List<Flight> GetFlightsByRunway(int runwayId)
        {
            List<Flight> flights = new List<Flight>();

            using (SqlConnection connection = _connectionFactory.GetConnection())
            {
                connection.Open();
                string sql = "SELECT id, date, flight_number, route_id, runway_id, gate_id FROM Flights WHERE runway_id = @runway_id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@runway_id", runwayId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            flights.Add(new Flight
                            {
                                Id = reader.GetInt32(0),
                                Date = reader.GetDateTime(1),
                                FlightNumber = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                RouteId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                                RunwayId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                GateId = reader.IsDBNull(5) ? 0 : reader.GetInt32(5)
                            });
                        }
                    }
                }
            }
            return flights;
        }


        public List<Flight> GetFlightsByGate(int gateId)
        {
            List<Flight> flights = new List<Flight>();
            using (SqlConnection connection = _connectionFactory.GetConnection())
            {
                connection.Open();
                string sql = "SELECT id, date, flight_number, route_id, runway_id, gate_id FROM Flights WHERE gate_id = @gate_id";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@gate_id", gateId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            flights.Add(new Flight { Id = reader.GetInt32(0), FlightNumber = reader.IsDBNull(2) ? "" : reader.GetString(2) });
                        }
                    }
                }
            }
            return flights;
        }

        public List<Flight> GetFlightsByAirport(int airportId)
        {
            List<Flight> flights = new List<Flight>();
            using (SqlConnection connection = _connectionFactory.GetConnection())
            {
                connection.Open();
                string sql = @"SELECT id, flight_number FROM Flights 
                       WHERE route_id IN (SELECT id FROM Routes WHERE airport_id = @airportId)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@airportId", airportId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            flights.Add(new Flight { Id = reader.GetInt32(0), FlightNumber = reader.IsDBNull(1) ? "" : reader.GetString(1) });
                        }
                    }
                }
            }
            return flights;
        }


        public int Add(Flight flight)
        {
            using (SqlConnection connection = _connectionFactory.GetConnection())
            {
                connection.Open();
                string sql = @"INSERT INTO Flights (route_id, date, runway_id, gate_id, flight_number)
                               OUTPUT INSERTED.id
                               VALUES (@route_id, @date, @runway_id, @gate_id, @flight_number)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@route_id", (object)flight.RouteId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@date", flight.Date);
                    command.Parameters.AddWithValue("@runway_id", (object)flight.RunwayId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@gate_id", (object)flight.GateId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@flight_number", flight.FlightNumber ?? (object)DBNull.Value);

                    return (int)command.ExecuteScalar();
                }
            }
        }

        

        public void Update(Flight flight)
        {
            using (SqlConnection connection = _connectionFactory.GetConnection())
            {
                connection.Open();
                string sql = @"UPDATE Flights
                               SET route_id = @route_id,
                                   date = @date,
                                   runway_id = @runway_id,
                                   gate_id = @gate_id,
                                   flight_number = @flight_number
                               WHERE id = @id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@route_id", (object)flight.RouteId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@date", flight.Date);
                    command.Parameters.AddWithValue("@runway_id", (object)flight.RunwayId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@gate_id", (object)flight.GateId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@flight_number", flight.FlightNumber ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@id", flight.Id);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (SqlConnection connection = _connectionFactory.GetConnection())
            {
                connection.Open();
                string delete_link_sql = "DELETE FROM Flight_employees WHERE id_flight = @id";
                string delete_flight_sql = "DELETE FROM Flights WHERE id = @id";

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd1 = new SqlCommand(delete_link_sql, connection, transaction))
                        {
                            cmd1.Parameters.AddWithValue("@id", id);
                            cmd1.ExecuteNonQuery();
                        }
                        using (SqlCommand cmd2 = new SqlCommand(delete_flight_sql, connection, transaction))
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