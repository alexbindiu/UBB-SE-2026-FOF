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
                            FlightNumber = reader.GetString(2),
                            RouteId = reader.GetInt32(3),
                            RunwayId = reader.GetInt32(4),
                            GateId = reader.GetInt32(5)
                        };

                        flights.Add(flight);
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
                    command.Parameters.AddWithValue("@route_id", flight.RouteId);
                    command.Parameters.AddWithValue("@date", flight.Date);
                    command.Parameters.AddWithValue("@runway_id", flight.RunwayId);
                    command.Parameters.AddWithValue("@gate_id", flight.GateId);
                    command.Parameters.AddWithValue("@flight_number", flight.FlightNumber);

                    return (int)command.ExecuteScalar();
                }
            }
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
                                FlightNumber = reader.GetString(2),
                                RouteId = reader.GetInt32(3),
                                RunwayId = reader.GetInt32(4),
                                GateId = reader.GetInt32(5)
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
                string sql = "SELECT id, route_id, date, runway_id, gate_id, flight_number FROM Flights WHERE route_id = @route_id";

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
                                RouteId = reader.GetInt32(1),
                                Date = reader.GetDateTime(2),
                                RunwayId = reader.GetInt32(3),
                                GateId = reader.GetInt32(4),
                                FlightNumber = reader.GetString(5)
                            };

                            flights.Add(flight);
                        }
                    }
                }
            }

            return flights;
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
                    command.Parameters.AddWithValue("@route_id", flight.RouteId);
                    command.Parameters.AddWithValue("@date", flight.Date);
                    command.Parameters.AddWithValue("@runway_id", flight.RunwayId);
                    command.Parameters.AddWithValue("@gate_id", flight.GateId);
                    command.Parameters.AddWithValue("@flight_number", flight.FlightNumber);
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
                string sql = "DELETE FROM Flights WHERE id = @id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}