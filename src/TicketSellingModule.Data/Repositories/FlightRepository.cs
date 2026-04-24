namespace TicketSellingModule.Data.Repositories
{
    public class FlightRepository(DatabaseConnectionFactory databaseConnectionFactory) : IFlightRepository
    {
        private const string SelectAllQuery = "SELECT id, date, flight_number, route_id, runway_id, gate_id FROM Flights";
        private const string SelectByIdQuery = "SELECT id, date, flight_number, route_id, runway_id, gate_id FROM Flights WHERE id = @flightId";
        private const string SelectByRouteQuery = "SELECT id, date, flight_number, route_id, runway_id, gate_id FROM Flights WHERE route_id = @routeId";
        private const string SelectByRunwayQuery = "SELECT id, date, flight_number, route_id, runway_id, gate_id FROM Flights WHERE runway_id = @runwayId";
        private const string SelectByGateQuery = "SELECT id, date, flight_number, route_id, runway_id, gate_id FROM Flights WHERE gate_id = @gateId";
        private const string SelectByAirportQuery = @"SELECT id, flight_number FROM Flights 
                                                      WHERE route_id IN (SELECT id FROM Routes WHERE airport_id = @airportId)";

        private const string InsertQuery = @"INSERT INTO Flights (route_id, date, runway_id, gate_id, flight_number)
                                             OUTPUT INSERTED.id
                                             VALUES (@routeId, @date, @runwayId, @gateId, @flightNumber)";

        private const string UpdateQuery = @"UPDATE Flights
                                             SET route_id = @routeId,
                                                 date = @date,
                                                 runway_id = @runwayId,
                                                 gate_id = @gateId,
                                                 flight_number = @flightNumber
                                             WHERE id = @flightId";

        private const string DeleteQuery = "DELETE FROM Flights WHERE id = @flightId";

        public List<Flight> GetAllFlights()
        {
            List<Flight> flights = new List<Flight>();

            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(SelectAllQuery, databaseConnection);
            using SqlDataReader dataReader = sqlCommand.ExecuteReader();

            while (dataReader.Read())
            {
                flights.Add(new Flight
                {
                    Id = dataReader.GetInt32(0),
                    Date = dataReader.GetDateTime(1),
                    FlightNumber = dataReader.IsDBNull(2) ? string.Empty : dataReader.GetString(2),
                    Route = new Route { Id = dataReader.IsDBNull(3) ? 0 : dataReader.GetInt32(3) },
                    Runway = new Runway { Id = dataReader.IsDBNull(4) ? 0 : dataReader.GetInt32(4) },
                    Gate = new Gate { Id = dataReader.IsDBNull(5) ? 0 : dataReader.GetInt32(5) }
                });
            }

            return flights;
        }

        public Flight? GetFlightById(int flightId)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(SelectByIdQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@flightId", flightId);

            using SqlDataReader dataReader = sqlCommand.ExecuteReader();

            if (dataReader.Read())
            {
                return new Flight
                {
                    Id = dataReader.GetInt32(0),
                    Date = dataReader.GetDateTime(1),
                    FlightNumber = dataReader.IsDBNull(2) ? string.Empty : dataReader.GetString(2),
                    Route = new Route { Id = dataReader.IsDBNull(3) ? 0 : dataReader.GetInt32(3) },
                    Runway = new Runway { Id = dataReader.IsDBNull(4) ? 0 : dataReader.GetInt32(4) },
                    Gate = new Gate { Id = dataReader.IsDBNull(5) ? 0 : dataReader.GetInt32(5) }
                };
            }

            return null;
        }

        public List<Flight> GetFlightsByRouteId(int routeId)
        {
            List<Flight> flights = new List<Flight>();

            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(SelectByRouteQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@routeId", routeId);

            using SqlDataReader dataReader = sqlCommand.ExecuteReader();

            while (dataReader.Read())
            {
                flights.Add(new Flight
                {
                    Id = dataReader.GetInt32(0),
                    Date = dataReader.GetDateTime(1),
                    FlightNumber = dataReader.IsDBNull(2) ? string.Empty : dataReader.GetString(2),
                    Route = new Route { Id = dataReader.IsDBNull(3) ? 0 : dataReader.GetInt32(3) },
                    Runway = new Runway { Id = dataReader.IsDBNull(4) ? 0 : dataReader.GetInt32(4) },
                    Gate = new Gate { Id = dataReader.IsDBNull(5) ? 0 : dataReader.GetInt32(5) }
                });
            }

            return flights;
        }

        public List<Flight> GetFlightsByRunwayId(int runwayId)
        {
            List<Flight> flights = new List<Flight>();

            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(SelectByRunwayQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@runwayId", runwayId);

            using SqlDataReader dataReader = sqlCommand.ExecuteReader();

            while (dataReader.Read())
            {
                flights.Add(new Flight
                {
                    Id = dataReader.GetInt32(0),
                    Date = dataReader.GetDateTime(1),
                    FlightNumber = dataReader.IsDBNull(2) ? string.Empty : dataReader.GetString(2),
                    Route = new Route { Id = dataReader.IsDBNull(3) ? 0 : dataReader.GetInt32(3) },
                    Runway = new Runway { Id = dataReader.IsDBNull(4) ? 0 : dataReader.GetInt32(4) },
                    Gate = new Gate { Id = dataReader.IsDBNull(5) ? 0 : dataReader.GetInt32(5) }
                });
            }

            return flights;
        }

        public List<Flight> GetFlightsByGateId(int gateId)
        {
            List<Flight> flights = new List<Flight>();

            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(SelectByGateQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@gateId", gateId);

            using SqlDataReader dataReader = sqlCommand.ExecuteReader();

            while (dataReader.Read())
            {
                flights.Add(new Flight
                {
                    Id = dataReader.GetInt32(0),
                    FlightNumber = dataReader.IsDBNull(2) ? string.Empty : dataReader.GetString(2)
                });
            }

            return flights;
        }

        public List<Flight> GetFlightsByAirportId(int airportId)
        {
            List<Flight> flights = new List<Flight>();

            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(SelectByAirportQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@airportId", airportId);

            using SqlDataReader dataReader = sqlCommand.ExecuteReader();

            while (dataReader.Read())
            {
                flights.Add(new Flight
                {
                    Id = dataReader.GetInt32(0),
                    FlightNumber = dataReader.IsDBNull(1) ? string.Empty : dataReader.GetString(1)
                });
            }

            return flights;
        }

        public int AddFlight(Flight flight)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(InsertQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@routeId", (object)flight.Route.Id ?? DBNull.Value);
            sqlCommand.Parameters.AddWithValue("@date", flight.Date);
            sqlCommand.Parameters.AddWithValue("@runwayId", (object)flight.Runway.Id ?? DBNull.Value);
            sqlCommand.Parameters.AddWithValue("@gateId", (object)flight.Gate.Id ?? DBNull.Value);
            sqlCommand.Parameters.AddWithValue("@flightNumber", flight.FlightNumber ?? (object)DBNull.Value);

            return (int)sqlCommand.ExecuteScalar();
        }

        public void UpdateFlight(Flight flight)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(UpdateQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@routeId", (object)flight.Route.Id ?? DBNull.Value);
            sqlCommand.Parameters.AddWithValue("@date", flight.Date);
            sqlCommand.Parameters.AddWithValue("@runwayId", (object)flight.Runway.Id ?? DBNull.Value);
            sqlCommand.Parameters.AddWithValue("@gateId", (object)flight.Gate.Id ?? DBNull.Value);
            sqlCommand.Parameters.AddWithValue("@flightNumber", flight.FlightNumber ?? (object)DBNull.Value);
            sqlCommand.Parameters.AddWithValue("@flightId", flight.Id);

            sqlCommand.ExecuteNonQuery();
        }

        public void DeleteFlightUsingId(int flightId)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlTransaction databaseTransaction = databaseConnection.BeginTransaction();
            try
            {
                using SqlCommand deleteFlightCommand = new SqlCommand(DeleteQuery, databaseConnection, databaseTransaction);
                deleteFlightCommand.Parameters.AddWithValue("@flightId", flightId);
                deleteFlightCommand.ExecuteNonQuery();

                databaseTransaction.Commit();
            }
            catch
            {
                databaseTransaction.Rollback();
                throw;
            }
        }
    }
}