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

                // The Master Query: Hops from Flights -> Routes -> Airports to build the whole object
                string sql = @"
                    SELECT 
                        f.id, f.date, f.flight_number, 
                        f.runway_id, r.name AS runway_name,
                        f.gate_id, g.name AS gate_name,
                        f.route_id, rt.airport_id, a.code AS airport_code, a.name AS airport_name
                    FROM Flights f
                    LEFT JOIN Runways r ON f.runway_id = r.id
                    LEFT JOIN Gates g ON f.gate_id = g.id
                    LEFT JOIN Routes rt ON f.route_id = rt.id
                    LEFT JOIN Airports a ON rt.airport_id = a.id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Flight flight = new Flight
                        {
                            Id = reader.GetInt32(0),
                            Date = reader.GetDateTime(1),
                            // Safety check in case you have old flights in the DB without a number
                            FlightNumber = reader.IsDBNull(2) ? "" : reader.GetString(2)
                        };

                        // 1. Build the Runway Object
                        if (!reader.IsDBNull(3))
                        {
                            flight.RunwayId = reader.GetInt32(3);
                            flight.Runway = new Runway
                            {
                                Id = flight.RunwayId,
                                Name = reader.GetString(4)
                            };
                        }

                        // 2. Build the Gate Object
                        if (!reader.IsDBNull(5))
                        {
                            flight.GateId = reader.GetInt32(5);
                            flight.Gate = new Gate
                            {
                                Id = flight.GateId,
                                Name = reader.GetString(6)
                            };
                        }

                        // 3. Build the Route AND Airport Object
                        if (!reader.IsDBNull(7))
                        {
                            flight.RouteId = reader.GetInt32(7);
                            flight.Route = new Route { Id = flight.RouteId };

                            if (!reader.IsDBNull(8))
                            {
                                flight.Route.AirportId = reader.GetInt32(8);
                                flight.Route.Airport = new Airport
                                {
                                    Id = flight.Route.AirportId,
                                    AirportCode = reader.GetString(9),
                                    AirportName = reader.GetString(10)
                                };
                            }
                        }

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
                    // Adding DBNull checks so C# doesn't crash if the user leaves these blank!
                    command.Parameters.AddWithValue("@route_id", (object)flight.RouteId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@date", flight.Date);
                    command.Parameters.AddWithValue("@runway_id", (object)flight.RunwayId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@gate_id", (object)flight.GateId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@flight_number", flight.FlightNumber ?? (object)DBNull.Value);

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
                // Upgraded to use the JOIN so fetching a single flight also gets its Gate/Runway/Airport
                string sql = @"
                    SELECT 
                        f.id, f.date, f.flight_number, 
                        f.runway_id, r.name AS runway_name,
                        f.gate_id, g.name AS gate_name,
                        f.route_id, rt.airport_id, a.code AS airport_code, a.name AS airport_name
                    FROM Flights f
                    LEFT JOIN Runways r ON f.runway_id = r.id
                    LEFT JOIN Gates g ON f.gate_id = g.id
                    LEFT JOIN Routes rt ON f.route_id = rt.id
                    LEFT JOIN Airports a ON rt.airport_id = a.id
                    WHERE f.id = @id";

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
                                FlightNumber = reader.IsDBNull(2) ? "" : reader.GetString(2)
                            };

                            if (!reader.IsDBNull(3))
                            {
                                flight.RunwayId = reader.GetInt32(3);
                                flight.Runway = new Runway { Id = flight.RunwayId, Name = reader.GetString(4) };
                            }

                            if (!reader.IsDBNull(5))
                            {
                                flight.GateId = reader.GetInt32(5);
                                flight.Gate = new Gate { Id = flight.GateId, Name = reader.GetString(6) };
                            }

                            if (!reader.IsDBNull(7))
                            {
                                flight.RouteId = reader.GetInt32(7);
                                flight.Route = new Route { Id = flight.RouteId };

                                if (!reader.IsDBNull(8))
                                {
                                    flight.Route.AirportId = reader.GetInt32(8);
                                    flight.Route.Airport = new Airport
                                    {
                                        Id = flight.Route.AirportId,
                                        AirportCode = reader.GetString(9),
                                        AirportName = reader.GetString(10)
                                    };
                                }
                            }
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
                string sql = @"
                    SELECT 
                        f.id, f.date, f.flight_number, 
                        f.runway_id, r.name AS runway_name,
                        f.gate_id, g.name AS gate_name,
                        f.route_id, rt.airport_id, a.code AS airport_code, a.name AS airport_name
                    FROM Flights f
                    LEFT JOIN Runways r ON f.runway_id = r.id
                    LEFT JOIN Gates g ON f.gate_id = g.id
                    LEFT JOIN Routes rt ON f.route_id = rt.id
                    LEFT JOIN Airports a ON rt.airport_id = a.id
                    WHERE f.route_id = @route_id";

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
                                FlightNumber = reader.IsDBNull(2) ? "" : reader.GetString(2)
                            };

                            if (!reader.IsDBNull(3))
                            {
                                flight.RunwayId = reader.GetInt32(3);
                                flight.Runway = new Runway { Id = flight.RunwayId, Name = reader.GetString(4) };
                            }

                            if (!reader.IsDBNull(5))
                            {
                                flight.GateId = reader.GetInt32(5);
                                flight.Gate = new Gate { Id = flight.GateId, Name = reader.GetString(6) };
                            }

                            if (!reader.IsDBNull(7))
                            {
                                flight.RouteId = reader.GetInt32(7);
                                flight.Route = new Route { Id = flight.RouteId };

                                if (!reader.IsDBNull(8))
                                {
                                    flight.Route.AirportId = reader.GetInt32(8);
                                    flight.Route.Airport = new Airport
                                    {
                                        Id = flight.Route.AirportId,
                                        AirportCode = reader.GetString(9),
                                        AirportName = reader.GetString(10)
                                    };
                                }
                            }

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