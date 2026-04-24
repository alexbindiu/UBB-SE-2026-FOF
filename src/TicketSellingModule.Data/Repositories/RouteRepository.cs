namespace TicketSellingModule.Data.Repositories
{
    public class RouteRepository(DatabaseConnectionFactory databaseConnectionFactory) : IRouteRepository
    {
        private const string SelectAllQuery = @"SELECT id, company_id, route_type, airport_id, reccurence_interval, 
                                                start_date, end_date, departure_time, arrival_time, capacity 
                                                FROM Routes";

        private const string SelectByIdQuery = @"SELECT id, company_id, route_type, airport_id, reccurence_interval, 
                                                 start_date, end_date, departure_time, arrival_time, capacity 
                                                 FROM Routes 
                                                 WHERE id = @routeId";

        private const string InsertQuery = @"INSERT INTO Routes 
                                             (company_id, route_type, airport_id, reccurence_interval, start_date, end_date, departure_time, arrival_time, capacity) 
                                             OUTPUT INSERTED.id 
                                             VALUES (@companyId, @routeType, @airportId, @recurrenceInterval, @startDate, @endDate, @departureTime, @arrivalTime, @capacity)";

        private const string UpdateQuery = @"UPDATE Routes SET 
                                             company_id = @companyId, 
                                             route_type = @routeType, 
                                             airport_id = @airportId, 
                                             reccurence_interval = @recurrenceInterval, 
                                             start_date = @startDate, 
                                             end_date = @endDate, 
                                             departure_time = @departureTime, 
                                             arrival_time = @arrivalTime, 
                                             capacity = @capacity 
                                             WHERE id = @routeId";

        private const string DeleteQuery = "DELETE FROM Routes WHERE id = @routeId";

        public List<Route> GetAllRoutes()
        {
            List<Route> routes = new List<Route>();

            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(SelectAllQuery, databaseConnection);
            using SqlDataReader dataReader = sqlCommand.ExecuteReader();

            while (dataReader.Read())
            {
                routes.Add(this.MapReaderToRoute(dataReader));
            }

            return routes;
        }

        public Route? GetRouteById(int routeId)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(SelectByIdQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@routeId", routeId);

            using SqlDataReader dataReader = sqlCommand.ExecuteReader();

            if (dataReader.Read())
            {
                return this.MapReaderToRoute(dataReader);
            }

            return null;
        }

        public int AddRoute(Route newRoute)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(InsertQuery, databaseConnection);
            this.AddRouteParameters(sqlCommand, newRoute);

            return (int)sqlCommand.ExecuteScalar();
        }

        public void DeleteRoute(int routeId)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(DeleteQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@routeId", routeId);

            sqlCommand.ExecuteNonQuery();
        }

        public void UpdateRoute(Route route)
        {
            using SqlConnection databaseConnection = databaseConnectionFactory.GetConnection();
            databaseConnection.Open();

            using SqlCommand sqlCommand = new SqlCommand(UpdateQuery, databaseConnection);
            this.AddRouteParameters(sqlCommand, route);
            sqlCommand.Parameters.AddWithValue("@routeId", route.Id);

            sqlCommand.ExecuteNonQuery();
        }

        //--helper for repetitive mapping logic
        private Route MapReaderToRoute(SqlDataReader dataReader)
        {
            return new Route
            {
                Id = dataReader.GetInt32(0),
                Company = new Company { Id = dataReader.GetInt32(1) },
                RouteType = dataReader.GetString(2),
                Airport = new Airport { Id = dataReader.GetInt32(3) },
                RecurrenceInterval = dataReader.GetInt32(4),
                StartDate = DateOnly.FromDateTime(dataReader.GetDateTime(5)),
                EndDate = DateOnly.FromDateTime(dataReader.GetDateTime(6)),
                DepartureTime = TimeOnly.FromDateTime(dataReader.GetDateTime(7)),
                ArrivalTime = TimeOnly.FromDateTime(dataReader.GetDateTime(8)),
                Capacity = dataReader.GetInt32(9)
            };
        }

        //--helper for repetitive parameter logic
        private void AddRouteParameters(SqlCommand sqlCommand, Route route)
        {
            sqlCommand.Parameters.AddWithValue("@companyId", route.Company.Id);
            sqlCommand.Parameters.AddWithValue("@airportId", route.Airport.Id);
            sqlCommand.Parameters.AddWithValue("@routeType", route.RouteType);
            sqlCommand.Parameters.AddWithValue("@recurrenceInterval", route.RecurrenceInterval);
            sqlCommand.Parameters.AddWithValue("@capacity", route.Capacity);
            sqlCommand.Parameters.AddWithValue("@startDate", route.StartDate.ToDateTime(TimeOnly.MinValue));
            sqlCommand.Parameters.AddWithValue("@endDate", route.EndDate.ToDateTime(TimeOnly.MinValue));
            sqlCommand.Parameters.AddWithValue("@departureTime", route.DepartureTime.ToTimeSpan());
            sqlCommand.Parameters.AddWithValue("@arrivalTime", route.ArrivalTime.ToTimeSpan());
        }
    }
}
