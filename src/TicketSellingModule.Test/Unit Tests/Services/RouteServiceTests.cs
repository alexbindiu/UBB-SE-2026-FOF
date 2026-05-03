using Moq;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class RouteServiceTests
{
    private const int ValidRouteId = 1;
    private const int InvalidRouteId = 99;
    private const string DepartureRouteType = "DEP";
    private const string ArrivalRouteType = "ARR";
    private const string DepartureRouteTypeLowercase = "dep";
    private const string ArrivalRouteTypeLowercase = "arr";
    private const string DepartureRouteTypeFull = "DEPARTURE";
    private const string ArrivalRouteTypeFull = "ARRIVAL";
    private const string DepartureRouteTypeLowercaseFull = "departure";
    private const string ArrivalRouteTypeLowercaseFull = "arrival";
    private const int NumberOfRoutes = 2;
    private const string DashString = "-";
    private const string UnknownRouteType = "transit";
    private static readonly TimeOnly ArrivalTime = new TimeOnly(14, 30);
    private static readonly TimeOnly DepartureTime = new TimeOnly(10, 00);
    private const string ArrivalTimeString = "14:30";
    private const string DepartureTimeString = "10:00";
    private const int ValidGateId = 1;
    private const int ValidRunwayId = 1;
    private const int DefaultRunwayId = 1;

    private const int DefaultCompanyId = 1;
    private const int DefaultAirportId = 1;
    private const int DefaultRecurrenceInterval = 0;
    private static readonly TimeOnly DefaultDepartureTime = new TimeOnly(10, 0);
    private static readonly TimeOnly DefaultArrivalTime = new TimeOnly(12, 0);
    private const int DefaultCapacity = 100;
    private const string DifferentFlightName = "FL999";
    private const int DefaultGateId = 1;
    private const int DefaultRouteId = 1;
    private static readonly DateTime TargetDate = new DateTime(2025, 1, 1);
    private static readonly DateTime DefaultDate = new DateTime(2025, 1, 1);
    private static readonly DateTime DifferentDate = new DateTime(2025, 2, 1);

    private static readonly TimeOnly DefaultWrapDepartureTime = new TimeOnly(23, 0);
    private static readonly TimeOnly DefaultWrapArrivalTime = new TimeOnly(1, 0);
    private static readonly TimeOnly DefaultWrap2DepartureTime = new TimeOnly(23, 30);
    private static readonly TimeOnly DefaultWrap2ArrivalTime = new TimeOnly(0, 30);
    private const string Wrap2FlightName = "WRAP2";
    private const string ValidFlightName = "VALID";
    private const string DefaultFlightNumber = "FL001";

    private static RouteService BuildService(
        Mock<IRouteRepository> routeRepo,
        Mock<IFlightRepository> flightRepo,
        Mock<ICompanyRepository> companyRepo = null,
        Mock<IAirportRepository> airportRepo = null)
    {
        return new RouteService(
            routeRepo.Object,
            flightRepo.Object,
            (companyRepo ?? new Mock<ICompanyRepository>()).Object,
            (airportRepo ?? new Mock<IAirportRepository>()).Object);
    }

    [Fact]
    public void GetById_Should_Return_Route_When_Route_Exists()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var route = new Route { RouteType = DepartureRouteType };
        mockRouteRepo.Setup(getRoute => getRoute.GetRouteById(ValidRouteId)).Returns(route);

        var routeService = BuildService(mockRouteRepo, mockFlightRepo);

        Assert.Equal(route, routeService.GetRouteById(ValidRouteId));
    }

    [Fact]
    public void GetById_Should_Return_Null_When_Route_Not_Found()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockRouteRepo.Setup(getNoRoute => getNoRoute.GetRouteById(InvalidRouteId)).Returns((Route)null);
        var routeService = BuildService(mockRouteRepo, mockFlightRepo);

        Assert.Null(routeService.GetRouteById(InvalidRouteId));
    }

    [Fact]
    public void GetAll_Should_Return_All_Routes_Always()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var routes = new List<Route> { new Route(), new Route() };
        mockRouteRepo.Setup(getAllRoutes => getAllRoutes.GetAllRoutes()).Returns(routes);

        var routeService = BuildService(mockRouteRepo, mockFlightRepo);

        Assert.Equal(NumberOfRoutes, routeService.GetAllRoutes().Count);
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_Dash_For_Null()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal(DashString, routeService.NormalizeFlightType(null));
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_Dash_For_Empty_String()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal(DashString, routeService.NormalizeFlightType(string.Empty));
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_Dash_For_Whitespace()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal(DashString, routeService.NormalizeFlightType("  "));
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_ARR_For_Arrival_Variants()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal(ArrivalRouteType, routeService.NormalizeFlightType(ArrivalRouteTypeLowercase));
        Assert.Equal(ArrivalRouteType, routeService.NormalizeFlightType(ArrivalRouteType));
        Assert.Equal(ArrivalRouteType, routeService.NormalizeFlightType(ArrivalRouteTypeLowercaseFull));
        Assert.Equal(ArrivalRouteType, routeService.NormalizeFlightType(ArrivalRouteTypeFull));
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_DEP_For_Departure_Variants()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal(DepartureRouteType, routeService.NormalizeFlightType(DepartureRouteTypeLowercase));
        Assert.Equal(DepartureRouteType, routeService.NormalizeFlightType(DepartureRouteType));
        Assert.Equal(DepartureRouteType, routeService.NormalizeFlightType(DepartureRouteTypeLowercaseFull));
        Assert.Equal(DepartureRouteType, routeService.NormalizeFlightType(DepartureRouteTypeFull));
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_Uppercased_Value_For_Unknown_Type()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal(UnknownRouteType.ToUpper(), routeService.NormalizeFlightType(UnknownRouteType));
    }

    [Fact]
    public void GetRelevantTime_Should_Return_Dash_For_Null_Route()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal(DashString, routeService.GetRelevantTime(null));
    }

    [Fact]
    public void GetRelevantTime_Should_Return_ArrivalTime_For_ARR_Route()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());
        var route = new Route
        {
            RouteType = ArrivalRouteType,
            ArrivalTime = ArrivalTime,
            DepartureTime = DepartureTime
        };

        Assert.Equal(ArrivalTimeString, routeService.GetRelevantTime(route));
    }

    [Fact]
    public void GetRelevantTime_Should_Return_DepartureTime_For_DEP_Route()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());
        var route = new Route
        {
            RouteType = DepartureRouteType,
            ArrivalTime = ArrivalTime,
            DepartureTime = DepartureTime
        };

        Assert.Equal(DepartureTimeString, routeService.GetRelevantTime(route));
    }

    [Fact]
    public void AddWithInitialFlight_Should_Succeed_When_No_Conflicts()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockCompanyRepo = new Mock<ICompanyRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        mockFlightRepo.Setup(getFlights => getFlights.GetAllFlights()).Returns(new List<Flight>());
        mockCompanyRepo.Setup(getCompany => getCompany.GetCompanyById(It.IsAny<int>())).Returns(new Company());
        mockAirportRepo.Setup(getAirport => getAirport.GetAirportById(It.IsAny<int>())).Returns(new Airport());
        mockRouteRepo.Setup(addRoute => addRoute.AddRoute(It.IsAny<Route>())).Returns(ValidRouteId);

        var routeService = new RouteService(mockRouteRepo.Object, mockFlightRepo.Object,
            mockCompanyRepo.Object, mockAirportRepo.Object);

        var result = routeService.AddWithInitialFlight(DefaultCompanyId, DefaultAirportId, DepartureRouteType, DefaultRecurrenceInterval, DefaultDate, DefaultDate,
            DepartureTime, ArrivalTime, DefaultCapacity, DefaultFlightNumber, DefaultRunwayId, DefaultGateId);

        Assert.Equal(ValidRouteId, result);
        mockRouteRepo.Verify(callsRepositorytoAddRoute => callsRepositorytoAddRoute.AddRoute(It.IsAny<Route>()), Times.Once);
        mockFlightRepo.Verify(callsRepositoryToAddflight => callsRepositoryToAddflight.AddFlight(It.IsAny<Flight>()), Times.Once);
    }

    [Fact]
    public void AddWithInitialFlight_Should_Not_Conflict_When_Times_Do_Not_Overlap()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockCompanyRepo = new Mock<ICompanyRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        var existingFlight = new Flight
        {
            Gate = new Gate { Id = ValidGateId },
            Runway = new Runway { Id = ValidRunwayId },
            Route = new Route { Id = ValidRouteId }
        };
        var existingRoute = new Route
        {
            DepartureTime = DepartureTime,
            ArrivalTime = ArrivalTime
        };

        mockFlightRepo.Setup(getFlights => getFlights.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(getRoute => getRoute.GetRouteById(ValidRouteId)).Returns(existingRoute);
        mockCompanyRepo.Setup(getCompany => getCompany.GetCompanyById(It.IsAny<int>())).Returns(new Company());
        mockAirportRepo.Setup(getAirport => getAirport.GetAirportById(It.IsAny<int>())).Returns(new Airport());
        mockRouteRepo.Setup(addRoute => addRoute.AddRoute(It.IsAny<Route>())).Returns(ValidRouteId);

        var routeService = new RouteService(mockRouteRepo.Object, mockFlightRepo.Object,
            mockCompanyRepo.Object, mockAirportRepo.Object);

        var result = routeService.AddWithInitialFlight(DefaultCompanyId, DefaultAirportId, DepartureRouteType, DefaultRecurrenceInterval, DefaultDate, DefaultDate,
            DepartureTime, ArrivalTime, DefaultCapacity, DefaultFlightNumber, DefaultRunwayId, DefaultGateId);

        Assert.Equal(ValidRouteId, result);
    }

    [Fact]
    public void AddWithInitialFlight_Should_Handle_Midnight_Wrap_Overlaps_When_Exist()
    {
        var existingRoute = new Route { DepartureTime = DefaultWrap2DepartureTime, ArrivalTime = DefaultWrap2ArrivalTime };
        {
            var mockRouteRepo = new Mock<IRouteRepository>();
            var mockFlightRepo = new Mock<IFlightRepository>();
            var existingFlight = new Flight { Date = DefaultDate, Gate = new Gate { Id = ValidGateId }, Runway = new Runway { Id = ValidRunwayId }, Route = new Route { Id = ValidRouteId } };
            mockFlightRepo.Setup(getFlight => getFlight.GetAllFlights()).Returns(new List<Flight> { existingFlight });
            mockRouteRepo.Setup(getRoute => getRoute.GetRouteById(ValidRouteId)).Returns(existingRoute);
            var routeService = BuildService(mockRouteRepo, mockFlightRepo);

            Assert.Throws<InvalidOperationException>(() =>
                routeService.AddWithInitialFlight(DefaultCompanyId, DefaultAirportId, DepartureRouteType, DefaultRecurrenceInterval, DefaultDate, DefaultDate, DefaultWrapDepartureTime, DefaultWrapArrivalTime, DefaultCapacity, Wrap2FlightName, DefaultRouteId, DefaultGateId));
        }

        {
            var mockRouteRepo = new Mock<IRouteRepository>();
            var mockFlightRepo = new Mock<IFlightRepository>();
            var existingFlight = new Flight { Date = DefaultDate, Gate = new Gate { Id = ValidGateId }, Runway = new Runway { Id = ValidRunwayId }, Route = new Route { Id = ValidRouteId } };
            mockFlightRepo.Setup(getFlight => getFlight.GetAllFlights()).Returns(new List<Flight> { existingFlight });
            mockRouteRepo.Setup(getRoute => getRoute.GetRouteById(ValidRouteId)).Returns(existingRoute);
            var service = BuildService(mockRouteRepo, mockFlightRepo);

            Assert.Throws<InvalidOperationException>(() =>
                service.AddWithInitialFlight(DefaultCompanyId, DefaultAirportId, DepartureRouteType, DefaultRecurrenceInterval, DefaultDate, DefaultDate, DefaultWrapDepartureTime, DefaultWrapArrivalTime, DefaultCapacity, Wrap2FlightName, DefaultRouteId, DefaultGateId));
        }
    }

    [Fact]
    public void AddWithInitialFlight_Should_Continue_If_Existing_Route_Is_Null()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();

        var existingFlight = new Flight { Date = DefaultDate, Gate = new Gate { Id = ValidGateId }, Runway = new Runway { Id = ValidRunwayId }, Route = new Route { Id = ValidRouteId } };

        mockFlightRepo.Setup(getFlight => getFlight.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(getNullInsteadOfRoute => getNullInsteadOfRoute.GetRouteById(InvalidRouteId)).Returns((Route)null);
        mockRouteRepo.Setup(addRoute => addRoute.AddRoute(It.IsAny<Route>())).Returns(ValidRouteId);

        var routeService = BuildService(mockRouteRepo, mockFlightRepo);

        int resultId = routeService.AddWithInitialFlight(DefaultCompanyId, DefaultAirportId, DepartureRouteType, DefaultRecurrenceInterval, DefaultDate, DefaultDate, DefaultDepartureTime, DefaultArrivalTime, DefaultCapacity, ValidFlightName, DefaultGateId, DefaultRouteId);

        Assert.Equal(DefaultRouteId, resultId);
    }

    [Fact]
    public void AddWithInitialFlight_Should_Skip_Flights_On_Different_Date()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockCompanyRepo = new Mock<ICompanyRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        var otherDayFlight = new Flight
        {
            Date = DifferentDate,
            Gate = new Gate { Id = ValidGateId },
            Runway = new Runway { Id = ValidRunwayId },
            Route = new Route { Id = ValidRouteId }
        };

        mockFlightRepo.Setup(getFlight => getFlight.GetAllFlights()).Returns(new List<Flight> { otherDayFlight });
        mockCompanyRepo.Setup(getCompany => getCompany.GetCompanyById(It.IsAny<int>())).Returns(new Company());
        mockAirportRepo.Setup(getAirport => getAirport.GetAirportById(It.IsAny<int>())).Returns(new Airport());
        mockRouteRepo.Setup(addRoute => addRoute.AddRoute(It.IsAny<Route>())).Returns(ValidRouteId);

        var routeService = new RouteService(mockRouteRepo.Object, mockFlightRepo.Object,
            mockCompanyRepo.Object, mockAirportRepo.Object);

        var result = routeService.AddWithInitialFlight(DefaultCompanyId, DefaultAirportId, DepartureRouteType, DefaultRecurrenceInterval, TargetDate, TargetDate,
            DefaultDepartureTime, DefaultArrivalTime, DefaultCapacity, DifferentFlightName, DefaultGateId, DefaultRouteId);

        Assert.Equal(ValidRouteId, result);
        mockRouteRepo.Verify(doesNotCallRepository => doesNotCallRepository.GetRouteById(It.IsAny<int>()), Times.Never);
    }
}