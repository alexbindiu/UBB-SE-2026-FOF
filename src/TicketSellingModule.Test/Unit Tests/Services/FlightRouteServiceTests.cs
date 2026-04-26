using Moq;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class FlightRouteServiceTests
{
    private const int TargetCompanyId = 1;
    private const int TargetAirportId = 1;
    private const int TargetFlightId = 1;
    private const int TargetRouteId = 5;
    private const int TargetRunwayId = 1;
    private const int TargetGateId = 1;
    private const int ConflictingRunwayId = 99;
    private const int ConflictingGateId = 99;
    private const int ConflictingRouteId = 99;
    private const int GeneratedRouteId = 20;
    private const int ValidCapacity = 100;
    private const int InvalidCapacity = 0;
    private const int ValidInterval = 1;
    private const string RouteTypeDeparture = "DEP";
    private const string ValidFlightNumber = "FL001";
    private const string ConflictingFlightNumber = "EX001";
    private const string CustomRecurrenceType = "Custom";
    private const string DailyRecurrenceType = "Daily";
    private const string WeeklyRecurrenceType = "Weekly";
    private const string MonthlyRecurrenceType = "Monthly";
    private const string ValidCustomDaysInterval = "5";
    private const string InvalidBiweeklyRecurrenceType = "Biweekly";
    private const int MissingRouteId = 999;
    private const int ExpectedRouteId = 8;
    private const int NonRecurrentInterval = 0;
    private const string NullFlightNumber = "NULLROUTE";
    private const string TargetAirportCode = "LHR";
    private const string TargetRunwayName = "Runway 2";
    private const string TargetGateName = "Gate 3";
    private const string InvalidCustomDaysInterval = "abc";
    private const string RouteTypeArrival = "ARR";
    private const int InvalidNegativeFlightId = -1;
    private const string InvalidZeroDaysInterval = "0";

    private const int OtherCompanyId = 99;
    private const int EmptyForeignKeyId = 0;

    private const string MissingDestinationPlaceholder = "-";
    private const string AirportCodeJfk = "JFK";
    private const string AirportNameJfk = "John F. Kennedy";
    private const string ExpectedDestinationTextJfk = "JFK - John F. Kennedy";

    private const string AirportCodeLhr = "LHR";
    private const string AirportNameLhr = "Heathrow";
    private const string RunwayTwoName = "Runway 2";
    private const string GateThreeName = "Gate 3";

    private static readonly DateTime TargetStartDate = new DateTime(2026, 6, 10);
    private static readonly DateTime TargetEndDate = new DateTime(2026, 6, 20);
    private static readonly DateTime InvalidPastDate = new DateTime(2026, 6, 1);

    private static readonly TimeOnly TargetDepartureTime = new TimeOnly(10, 0);
    private static readonly TimeOnly TargetArrivalTime = new TimeOnly(12, 0);
    private static readonly TimeOnly ConflictingDepartureTime = new TimeOnly(10, 30);
    private static readonly TimeOnly ConflictingArrivalTime = new TimeOnly(11, 30);

    private static readonly TimeOnly BeforeMidnightDepartureTimeExisting = new TimeOnly(23, 0);
    private static readonly TimeOnly AfterMidnightArrivalTimeExisting = new TimeOnly(1, 0);
    private static readonly TimeOnly BeforeMidnightDepartureTimeTarget = new TimeOnly(22, 30);
    private static readonly TimeOnly AfterMidnightArrivalTimeTarget = new TimeOnly(0, 30);

    private static FlightRouteService CreateTestService(
        Mock<IFlightRepository> flightRepository,
        Mock<IRouteRepository> routeRepository,
        Mock<ICompanyRepository> companyRepository = null,
        Mock<IAirportRepository> airportRepository = null,
        Mock<IRunwayService> runwayService = null,
        Mock<IGateService> gateService = null,
        Mock<IAirportService> airportService = null)
    {
        return new FlightRouteService(
            flightRepository.Object,
            routeRepository.Object,
            (companyRepository ?? new Mock<ICompanyRepository>()).Object,
            (airportRepository ?? new Mock<IAirportRepository>()).Object,
            (runwayService ?? new Mock<IRunwayService>()).Object,
            (gateService ?? new Mock<IGateService>()).Object,
            (airportService ?? new Mock<IAirportService>()).Object);
    }

    private static (
        Mock<IFlightRepository> flightRepository,
        Mock<IRouteRepository> routeRepository,
        Mock<ICompanyRepository> companyRepository,
        Mock<IAirportRepository> airportRepository) ConfigureSuccessfulRepositorys()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();
        var companyRepository = new Mock<ICompanyRepository>();
        var airportRepository = new Mock<IAirportRepository>();

        flightRepository.Setup(getNoFlights => getNoFlights.GetAllFlights()).Returns(new List<Flight>());
        companyRepository.Setup(getDefaultCompanyForAnyId => getDefaultCompanyForAnyId.GetCompanyById(It.IsAny<int>())).Returns(new Company());
        airportRepository.Setup(getDefaultAriportForAnyId => getDefaultAriportForAnyId.GetAirportById(It.IsAny<int>())).Returns(new Airport());
        routeRepository.Setup(addTagerRouteToRepoAndGiveItsId => addTagerRouteToRepoAndGiveItsId.AddRoute(It.IsAny<Route>())).Returns(TargetRouteId);

        return (flightRepository, routeRepository, companyRepository, airportRepository);
    }

    [Fact]
    public void AddFlightToRoute_ThrowsArgumentException_WhenStartDateIsAfterEndDate()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();
        flightRepository.Setup(getNoFlights => getNoFlights.GetAllFlights()).Returns(new List<Flight>());

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        Assert.Throws<ArgumentException>(() =>
            flightRouteService.AddFlightToRoute(TargetCompanyId, TargetAirportId, RouteTypeDeparture, ValidInterval,
                TargetStartDate, InvalidPastDate, TargetDepartureTime, TargetArrivalTime, ValidCapacity, ValidFlightNumber, TargetRunwayId, TargetGateId));
    }

    [Fact]
    public void AddFlightToRoute_ThrowsArgumentException_WhenCapacityIsZero()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();
        flightRepository.Setup(getNoFlights => getNoFlights.GetAllFlights()).Returns(new List<Flight>());
        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        var exception = Assert.Throws<ArgumentException>(() =>
            flightRouteService.AddFlightToRoute(TargetCompanyId, TargetAirportId, RouteTypeDeparture, ValidInterval,
                TargetStartDate, TargetEndDate, TargetDepartureTime, TargetArrivalTime, InvalidCapacity, ValidFlightNumber, TargetRunwayId, TargetGateId));

        Assert.Equal("Capacity must be a positive number greater than 0.", exception.Message);
    }

    [Fact]
    public void AddFlightToRoute_ThrowsInvalidOperationException_WhenGateIsOccupied()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var existingFlight = new Flight
        {
            Gate = new Gate { Id = TargetGateId },
            Runway = new Runway { Id = ConflictingRunwayId },
            Route = new Route { Id = TargetRouteId },
            FlightNumber = ConflictingFlightNumber,
            Date = TargetStartDate
        };
        var existingRoute = new Route
        {
            DepartureTime = ConflictingDepartureTime,
            ArrivalTime = ConflictingArrivalTime
        };

        flightRepository.Setup(getExistingFlight => getExistingFlight.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        routeRepository.Setup(getExistingRoute => getExistingRoute.GetRouteById(TargetRouteId)).Returns(existingRoute);

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        Assert.Throws<InvalidOperationException>(() =>
            flightRouteService.AddFlightToRoute(TargetCompanyId, TargetAirportId, RouteTypeDeparture, ValidInterval,
                TargetStartDate, TargetStartDate, TargetDepartureTime, TargetArrivalTime, ValidCapacity, ValidFlightNumber, ConflictingRunwayId, TargetGateId));
    }

    [Fact]
    public void AddFlightToRoute_ThrowsInvalidOperationException_WhenRunwayIsOccupied()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var existingFlight = new Flight
        {
            Gate = new Gate { Id = ConflictingGateId },
            Runway = new Runway { Id = TargetRunwayId },
            Route = new Route { Id = TargetRouteId },
            FlightNumber = ConflictingFlightNumber,
            Date = TargetStartDate
        };

        var existingRoute = new Route
        {
            DepartureTime = ConflictingDepartureTime,
            ArrivalTime = ConflictingArrivalTime
        };

        flightRepository.Setup(getExistingFlight => getExistingFlight.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        routeRepository.Setup(getExistingRoute => getExistingRoute.GetRouteById(TargetRouteId)).Returns(existingRoute);

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        Assert.Throws<InvalidOperationException>(() =>
            flightRouteService.AddFlightToRoute(TargetCompanyId, TargetAirportId, RouteTypeDeparture, ValidInterval,
                TargetStartDate, TargetStartDate, TargetDepartureTime, TargetArrivalTime, ValidCapacity, ValidFlightNumber, TargetRunwayId, ConflictingGateId));
    }

    [Fact]
    public void AddFlightToRoute_ReturnsGeneratedRouteId_WhenNoConflictsExist()
    {
        var (flightRepository, routeRepository, companyRepository, airportRepository) = ConfigureSuccessfulRepositorys();
        routeRepository.Setup(source => source.AddRoute(It.IsAny<Route>())).Returns(GeneratedRouteId);

        var flightRouteService = CreateTestService(flightRepository, routeRepository, companyRepository, airportRepository);

        int resultId = flightRouteService.AddFlightToRoute(TargetCompanyId, TargetAirportId, RouteTypeDeparture, ValidInterval,
            TargetStartDate, TargetStartDate, TargetDepartureTime, TargetArrivalTime, ValidCapacity, ValidFlightNumber, TargetRunwayId, TargetGateId);

        Assert.Equal(GeneratedRouteId, resultId);
        routeRepository.Verify(callRepositoryToAddRoute => callRepositoryToAddRoute.AddRoute(It.IsAny<Route>()), Times.Once);
        flightRepository.Verify(callRepositoryToAddFlight => callRepositoryToAddFlight.AddFlight(It.IsAny<Flight>()), Times.Once);
    }

    [Fact]
    public void AddFlightToRoute_DoesNotCheckRoute_WhenExistingFlightIsOnDifferentDate()
    {
        var (flightRepository, routeRepository, companyRepository, airportRepository) = ConfigureSuccessfulRepositorys();

        var otherDayFlight = new Flight
        {
            Date = TargetStartDate.AddDays(1),
            Gate = new Gate { Id = TargetGateId },
            Runway = new Runway { Id = TargetRunwayId },
            Route = new Route { Id = TargetRouteId },
            FlightNumber = ConflictingFlightNumber
        };

        flightRepository.Setup(getOtherDayFlight => getOtherDayFlight.GetAllFlights()).Returns(new List<Flight> { otherDayFlight });
        routeRepository.Setup(addRouteToRepositoryAndGetGeneratedRouteId => addRouteToRepositoryAndGetGeneratedRouteId.AddRoute(It.IsAny<Route>())).Returns(GeneratedRouteId);

        var flightRouteService = CreateTestService(flightRepository, routeRepository, companyRepository, airportRepository);

        int resultId = flightRouteService.AddFlightToRoute(TargetCompanyId, TargetAirportId, RouteTypeDeparture, ValidInterval,
            TargetStartDate, TargetStartDate, TargetDepartureTime, TargetArrivalTime, ValidCapacity, ValidFlightNumber, TargetRunwayId, TargetGateId);

        Assert.Equal(GeneratedRouteId, resultId);
        routeRepository.Verify(source => source.GetRouteById(TargetRouteId), Times.Never);
    }

    [Fact]
    public void AddFlightToRoute_ThrowsInvalidOperationException_WhenTimesOverlapAcrossMidnight()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var existingFlight = new Flight
        {
            Date = TargetStartDate,
            Route = new Route { Id = TargetRouteId },
            Gate = new Gate { Id = TargetGateId },
            Runway = new Runway { Id = ConflictingRunwayId }
        };

        var routeCrossingMidnight = new Route
        {
            DepartureTime = BeforeMidnightDepartureTimeExisting,
            ArrivalTime = AfterMidnightArrivalTimeExisting
        };

        flightRepository.Setup(getExistingFlight => getExistingFlight.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        routeRepository.Setup(getRouteCrossingMidnight => getRouteCrossingMidnight.GetRouteById(TargetRouteId)).Returns(routeCrossingMidnight);

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        Assert.Throws<InvalidOperationException>(() =>
            flightRouteService.AddFlightToRoute(TargetCompanyId, TargetAirportId, RouteTypeDeparture, ValidInterval,
                TargetStartDate, TargetStartDate, BeforeMidnightDepartureTimeTarget, AfterMidnightArrivalTimeTarget, ValidCapacity, ValidFlightNumber, ConflictingRunwayId, TargetGateId));
    }

    [Fact]
    public void AddFlightToRoute_ReturnsGeneratedRouteId_WhenExistingRouteIsNull()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();
        var companyRepository = new Mock<ICompanyRepository>();
        var airportRepository = new Mock<IAirportRepository>();

        var flightWithMissingRoute = new Flight
        {
            Date = TargetStartDate,
            Gate = new Gate { Id = TargetGateId },
            Runway = new Runway { Id = TargetRunwayId },
            Route = new Route { Id = MissingRouteId },
            FlightNumber = NullFlightNumber
        };

        flightRepository.Setup(getFlightWithMissingroute => getFlightWithMissingroute.GetAllFlights()).Returns(new List<Flight> { flightWithMissingRoute });
        routeRepository.Setup(getNullInsteadOfRoute => getNullInsteadOfRoute.GetRouteById(MissingRouteId)).Returns((Route?)null);
        companyRepository.Setup(getDefaultCompany => getDefaultCompany.GetCompanyById(It.IsAny<int>())).Returns(new Company());
        airportRepository.Setup(getDefaultAirport => getDefaultAirport.GetAirportById(It.IsAny<int>())).Returns(new Airport());

        routeRepository.Setup(addExpectedRouteAndGetItsId => addExpectedRouteAndGetItsId.AddRoute(It.IsAny<Route>())).Returns(ExpectedRouteId);

        var flightRouteService = CreateTestService(
            flightRepository: flightRepository,
            routeRepository: routeRepository,
            companyRepository: companyRepository,
            airportRepository: airportRepository);

        int resultId = flightRouteService.AddFlightToRoute(
            TargetCompanyId,
            TargetAirportId,
            RouteTypeDeparture,
            NonRecurrentInterval,
            TargetStartDate,
            TargetStartDate,
            TargetDepartureTime,
            TargetArrivalTime,
            ValidCapacity,
            ValidFlightNumber,
            TargetRunwayId,
            TargetGateId);

        Assert.Equal(ExpectedRouteId, resultId);
    }

    [Fact]
    public void GetRouteById_ReturnsRouteObject_WhenIdIsValid()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var targetRoute = new Route { RouteType = RouteTypeArrival };

        routeRepository.Setup(getTargetRoute => getTargetRoute.GetRouteById(TargetRouteId)).Returns(targetRoute);

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        Route? resultRoute = flightRouteService.GetRouteById(TargetRouteId);

        Assert.Equal(targetRoute, resultRoute);
    }

    [Fact]
    public void GetFlightById_ReturnsFlightObject_WhenIdIsValid()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var targetFlight = new Flight { FlightNumber = ValidFlightNumber };

        flightRepository.Setup(getTargetFlight => getTargetFlight.GetFlightById(TargetFlightId)).Returns(targetFlight);

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        Flight? resultFlight = flightRouteService.GetFlightById(TargetFlightId);

        Assert.Equal(targetFlight, resultFlight);
    }

    [Fact]
    public void GetAllRoutes_ReturnsAllRecords_WhenRecordsExist()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var existingRoutesList = new List<Route> { new Route(), new Route() };

        routeRepository.Setup(getAllRoutes => getAllRoutes.GetAllRoutes()).Returns(existingRoutesList);

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        List<Route> resultList = flightRouteService.GetAllRoutes();

        Assert.Equal(2, resultList.Count);
    }

    [Fact]
    public void GetAllFlights_ReturnsAllRecords_WhenRecordsExist()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var existingFlightsList = new List<Flight> { new Flight(), new Flight() };

        flightRepository.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(existingFlightsList);

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        List<Flight> resultList = flightRouteService.GetAllFlights();

        Assert.Equal(2, resultList.Count);
    }

    [Fact]
    public void DeleteFlightUsingId_ThrowsArgumentException_WhenIdIsNegative()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        Assert.Throws<ArgumentException>(() => flightRouteService.DeleteFlightUsingId(InvalidNegativeFlightId));
    }

    [Fact]
    public void DeleteFlightUsingId_ThrowsArgumentException_WhenFlightIsNotFound()
    {
        var flightRepositoryThatReturnsNull = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        flightRepositoryThatReturnsNull.Setup(getNullInsteadOfFlight => getNullInsteadOfFlight.GetFlightById(TargetFlightId)).Returns((Flight?)null);

        var flightRouteService = CreateTestService(flightRepositoryThatReturnsNull, routeRepository);

        Assert.Throws<ArgumentException>(() => flightRouteService.DeleteFlightUsingId(TargetFlightId));
    }

    [Fact]
    public void DeleteFlightUsingId_CallsRepositoryDelete_WhenIdIsValid()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        flightRepository.Setup(getDefaultFlight => getDefaultFlight.GetFlightById(TargetFlightId)).Returns(new Flight());

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        flightRouteService.DeleteFlightUsingId(TargetFlightId);

        flightRepository.Verify(callRepositoryToDeleteFlight => callRepositoryToDeleteFlight.DeleteFlightUsingId(TargetFlightId), Times.Once);
    }

    [Fact]
    public void GetFlightsByCompanyId_ReturnsFilteredFlights_WhenRoutesMatchCompany()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var routesList = new List<Route>
        {
            new Route { Id = TargetRouteId, Company = new Company { Id = TargetCompanyId } },
            new Route { Id = ConflictingRouteId, Company = new Company { Id = OtherCompanyId } }
        };

        var flightsList = new List<Flight>
        {
            new Flight { FlightNumber = ValidFlightNumber, Route = new Route { Id = TargetRouteId } },
            new Flight { FlightNumber = ConflictingFlightNumber, Route = new Route { Id = ConflictingRouteId } }
        };

        routeRepository.Setup(getAllRoutes => getAllRoutes.GetAllRoutes()).Returns(routesList);
        flightRepository.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(flightsList);

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        List<Flight> resultList = flightRouteService.GetFlightsByCompanyId(TargetCompanyId);

        Assert.Single(resultList);
        Assert.Equal(ValidFlightNumber, resultList[0].FlightNumber);
    }

    [Fact]
    public void GetFlightsByCompanyId_ReturnsEmptyList_WhenNoRoutesMatchCompany()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        routeRepository.Setup(getNoRoutes => getNoRoutes.GetAllRoutes()).Returns(new List<Route>());
        flightRepository.Setup(getTargetFlightWithTargetRoute => getTargetFlightWithTargetRoute.GetAllFlights()).Returns(new List<Flight> { new Flight { Route = new Route { Id = TargetRouteId } } });

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        List<Flight> resultList = flightRouteService.GetFlightsByCompanyId(TargetCompanyId);

        Assert.Empty(resultList);
    }

    [Fact]
    public void GetDestinationText_ReturnsPlaceholder_WhenRouteIsNull()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        var flightWithNoRoute = new Flight { Route = null };

        string resultText = flightRouteService.GetDestinationText(flightWithNoRoute);

        Assert.Equal(MissingDestinationPlaceholder, resultText);
    }

    [Fact]
    public void GetDestinationText_ReturnsPlaceholder_WhenAirportIsNull()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        var flightWithNoAirport = new Flight { Route = new Route { Airport = null } };

        string resultText = flightRouteService.GetDestinationText(flightWithNoAirport);

        Assert.Equal(MissingDestinationPlaceholder, resultText);
    }

    [Fact]
    public void GetDestinationText_ReturnsFormattedString_WhenAirportIsValid()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        var validFlight = new Flight
        {
            Route = new Route
            {
                Airport = new Airport { AirportCode = AirportCodeJfk, AirportName = AirportNameJfk }
            }
        };

        string resultText = flightRouteService.GetDestinationText(validFlight);

        Assert.Equal(ExpectedDestinationTextJfk, resultText);
    }

    [Fact]
    public void GetAllFlightsWithDetails_SkipsHydration_WhenForeignKeysAreNullOrInvalid()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var flightWithMissingForeignKeys = new Flight
        {
            Runway = new Runway { Id = EmptyForeignKeyId },
            Gate = null,
            Route = new Route { Id = TargetRouteId, Airport = new Airport { Id = EmptyForeignKeyId } }
        };

        flightRepository.Setup(getFlightWithMissingForeignKeys => getFlightWithMissingForeignKeys.GetAllFlights()).Returns(new List<Flight> { flightWithMissingForeignKeys });

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        List<Flight> resultList = flightRouteService.GetAllFlightsWithDetails();

        Assert.Single(resultList);
    }

    [Fact]
    public void CreateFlightWithSchedule_ThrowsInvalidOperationException_WhenRecurrentEndIsBeforeStart()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        Assert.Throws<InvalidOperationException>(() =>
            flightRouteService.CreateFlightWithSchedule(TargetCompanyId, RouteTypeDeparture, TargetAirportId, ValidCapacity,
                TargetDepartureTime.ToTimeSpan(), TargetArrivalTime.ToTimeSpan(),
                isRecurrent: true, TargetStartDate, InvalidPastDate, null, DailyRecurrenceType, string.Empty, TargetRunwayId, TargetGateId, _ => ValidFlightNumber));
    }

    [Fact]
    public void CreateFlightWithSchedule_ThrowsInvalidOperationException_ForEqualDepartureAndArrivalTimes()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        Assert.Throws<InvalidOperationException>(() =>
            flightRouteService.CreateFlightWithSchedule(TargetCompanyId, RouteTypeDeparture, TargetAirportId, ValidCapacity,
                TargetDepartureTime.ToTimeSpan(), TargetDepartureTime.ToTimeSpan(),
                isRecurrent: false, null, null, TargetStartDate, DailyRecurrenceType, string.Empty, TargetRunwayId, TargetGateId, _ => ValidFlightNumber));
    }

    [Fact]
    public void CreateFlightWithSchedule_ThrowsInvalidOperationException_ForInvalidRecurrenceType()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        Assert.Throws<InvalidOperationException>(() =>
            flightRouteService.CreateFlightWithSchedule(TargetCompanyId, RouteTypeDeparture, TargetAirportId, ValidCapacity,
                TargetDepartureTime.ToTimeSpan(), TargetArrivalTime.ToTimeSpan(),
                isRecurrent: true, TargetStartDate, TargetStartDate, null, InvalidBiweeklyRecurrenceType, string.Empty, TargetRunwayId, TargetGateId, _ => ValidFlightNumber));
    }

    [Fact]
    public void CreateFlightWithSchedule_ThrowsInvalidOperationException_WhenCustomIntervalIsInvalid()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        flightRepository.Setup(getNoFlights => getNoFlights.GetAllFlights()).Returns(new List<Flight>());

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        Assert.Throws<InvalidOperationException>(() =>
            flightRouteService.CreateFlightWithSchedule(
                TargetCompanyId,
                RouteTypeDeparture,
                TargetAirportId,
                ValidCapacity,
                TargetDepartureTime.ToTimeSpan(),
                TargetArrivalTime.ToTimeSpan(),
                isRecurrent: true,
                TargetStartDate,
                TargetStartDate,
                null,
                CustomRecurrenceType,
                InvalidCustomDaysInterval,
                TargetRunwayId,
                TargetGateId,
                _ => ValidFlightNumber));
    }

    [Fact]
    public void CreateFlightWithSchedule_Succeeds_ForNonRecurrentFlight()
    {
        var (flightRepository, routeRepository, companyRepository, airportRepository) = ConfigureSuccessfulRepositorys();
        var flightRouteService = CreateTestService(flightRepository, routeRepository, companyRepository, airportRepository);

        flightRouteService.CreateFlightWithSchedule(TargetCompanyId, RouteTypeDeparture, TargetAirportId, ValidCapacity,
            TargetDepartureTime.ToTimeSpan(), TargetArrivalTime.ToTimeSpan(),
            isRecurrent: false, null, null, TargetStartDate, string.Empty, string.Empty, TargetRunwayId, TargetGateId, _ => ValidFlightNumber);

        routeRepository.Verify(callsRepositoryToAddRoute => callsRepositoryToAddRoute.AddRoute(It.IsAny<Route>()), Times.Once);
        flightRepository.Verify(callsRepositoryToAddFlight => callsRepositoryToAddFlight.AddFlight(It.IsAny<Flight>()), Times.Once);
    }

    [Fact]
    public void CreateFlightWithSchedule_Succeeds_ForDailyRecurrence()
    {
        var (flightRepository, routeRepository, companyRepository, airportRepository) = ConfigureSuccessfulRepositorys();
        var flightRouteService = CreateTestService(flightRepository, routeRepository, companyRepository, airportRepository);

        flightRouteService.CreateFlightWithSchedule(
            TargetCompanyId,
            RouteTypeDeparture,
            TargetAirportId,
            ValidCapacity,
            TargetDepartureTime.ToTimeSpan(),
            TargetArrivalTime.ToTimeSpan(),
            isRecurrent: true,
            TargetStartDate,
            TargetEndDate,
            null,
            DailyRecurrenceType,
            null,
            TargetRunwayId,
            TargetGateId,
            _ => ValidFlightNumber);

        routeRepository.Verify(callRepoToAddRoute => callRepoToAddRoute.AddRoute(It.IsAny<Route>()), Times.Once);
    }

    [Fact]
    public void CreateFlightWithSchedule_Succeeds_ForWeeklyRecurrence()
    {
        var (flightRepository, routeRepository, companyRepository, airportRepository) = ConfigureSuccessfulRepositorys();
        var flightRouteService = CreateTestService(flightRepository, routeRepository, companyRepository, airportRepository);

        flightRouteService.CreateFlightWithSchedule(
            TargetCompanyId,
            RouteTypeDeparture,
            TargetAirportId,
            ValidCapacity,
            TargetDepartureTime.ToTimeSpan(),
            TargetArrivalTime.ToTimeSpan(),
            isRecurrent: true,
            TargetStartDate,
            TargetEndDate,
            null,
            WeeklyRecurrenceType,
            null,
            TargetRunwayId,
            TargetGateId,
            _ => ValidFlightNumber);

        routeRepository.Verify(callRepoToAddRoute => callRepoToAddRoute.AddRoute(It.IsAny<Route>()), Times.Once);
    }

    [Fact]
    public void CreateFlightWithSchedule_Succeeds_ForMonthlyRecurrence()
    {
        var (flightRepository, routeRepository, companyRepository, airportRepository) = ConfigureSuccessfulRepositorys();
        var flightRouteService = CreateTestService(flightRepository, routeRepository, companyRepository, airportRepository);

        flightRouteService.CreateFlightWithSchedule(
            TargetCompanyId,
            RouteTypeDeparture,
            TargetAirportId,
            ValidCapacity,
            TargetDepartureTime.ToTimeSpan(),
            TargetArrivalTime.ToTimeSpan(),
            isRecurrent: true,
            TargetStartDate,
            TargetEndDate,
            null,
            MonthlyRecurrenceType,
            null,
            TargetRunwayId,
            TargetGateId,
            _ => ValidFlightNumber);

        routeRepository.Verify(callRepoToAddRoute => callRepoToAddRoute.AddRoute(It.IsAny<Route>()), Times.Once);
    }

    [Fact]
    public void CreateFlightWithSchedule_Succeeds_ForCustomRecurrence()
    {
        var (flightRepository, routeRepository, companyRepository, airportRepository) = ConfigureSuccessfulRepositorys();
        var flightRouteService = CreateTestService(flightRepository, routeRepository, companyRepository, airportRepository);

        flightRouteService.CreateFlightWithSchedule(TargetCompanyId, RouteTypeDeparture, TargetAirportId, ValidCapacity,
            TargetDepartureTime.ToTimeSpan(), TargetArrivalTime.ToTimeSpan(),
            isRecurrent: true, TargetStartDate, TargetStartDate.AddDays(14), null, CustomRecurrenceType, ValidCustomDaysInterval, TargetRunwayId, TargetGateId, _ => ValidFlightNumber);

        routeRepository.Verify(callsRepositoryToAddRoute => callsRepositoryToAddRoute.AddRoute(It.IsAny<Route>()), Times.Once);
    }

    [Fact]
    public void GetFlightsByCompanyId_ReturnsFilteredFlights_WhenCompanyHasRoutes()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var routesList = new List<Route>
        {
            new Route { Id = TargetRouteId, Company = new Company { Id = TargetCompanyId } }
        };
        var flightsList = new List<Flight>
        {
            new Flight { FlightNumber = ValidFlightNumber, Route = new Route { Id = TargetRouteId } }
        };

        routeRepository.Setup(source => source.GetAllRoutes()).Returns(routesList);
        flightRepository.Setup(source => source.GetAllFlights()).Returns(flightsList);

        var flightRouteService = CreateTestService(flightRepository, routeRepository);
        List<Flight> resultList = flightRouteService.GetFlightsByCompanyId(TargetCompanyId);

        Assert.Single(resultList);
        Assert.Equal(ValidFlightNumber, resultList[0].FlightNumber);
    }

    [Fact]
    public void GetAllFlightsWithDetails_HydratesAllProperties_WhenForeignKeysAreValid()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();
        var runwayService = new Mock<IRunwayService>();
        var gateService = new Mock<IGateService>();
        var airportServic = new Mock<IAirportService>();

        var targetAirport = new Airport { Id = TargetAirportId, AirportCode = TargetAirportCode };
        var targetRoute = new Route { Id = TargetRouteId, Airport = new Airport { Id = TargetAirportId } };
        var targetRunway = new Runway { Id = TargetRunwayId, Name = TargetRunwayName };
        var targetGate = new Gate { Id = TargetGateId, Name = TargetGateName };

        var targetFlight = new Flight
        {
            Runway = new Runway { Id = TargetRunwayId },
            Gate = new Gate { Id = TargetGateId },
            Route = new Route { Id = TargetRouteId }
        };

        flightRepository.Setup(getTargetFlight => getTargetFlight.GetAllFlights()).Returns(new List<Flight> { targetFlight });
        routeRepository.Setup(getTargetRoute => getTargetRoute.GetRouteById(TargetRouteId)).Returns(targetRoute);
        runwayService.Setup(getTargetRunway => getTargetRunway.GetRunwayById(TargetRunwayId)).Returns(targetRunway);
        gateService.Setup(getTargetGate => getTargetGate.GetGateById(TargetGateId)).Returns(targetGate);
        airportServic.Setup(getTargetAirport => getTargetAirport.GetAirportById(TargetAirportId)).Returns(targetAirport);

        var flightRouteService = CreateTestService(flightRepository, routeRepository,
            runwayService: runwayService, gateService: gateService, airportService: airportServic);

        List<Flight> resultList = flightRouteService.GetAllFlightsWithDetails();

        Assert.Single(resultList);
        Assert.Equal(TargetRunwayName, resultList[0].Runway.Name);
        Assert.Equal(TargetGateName, resultList[0].Gate.Name);
        Assert.Equal(TargetAirportCode, resultList[0].Route.Airport.AirportCode);
    }

    [Fact]
    public void CreateFlightWithSchedule_ThrowsInvalidOperationException_WhenCustomIntervalIsZero()
    {
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        flightRepository.Setup(getNoFlights => getNoFlights.GetAllFlights()).Returns(new List<Flight>());

        var flightRouteService = CreateTestService(flightRepository, routeRepository);

        Assert.Throws<InvalidOperationException>(() =>
            flightRouteService.CreateFlightWithSchedule(
                TargetCompanyId,
                RouteTypeDeparture,
                TargetAirportId,
                ValidCapacity,
                TargetDepartureTime.ToTimeSpan(),
                TargetArrivalTime.ToTimeSpan(),
                isRecurrent: true,
                TargetStartDate,
                TargetStartDate,
                null,
                CustomRecurrenceType,
                InvalidZeroDaysInterval,
                TargetRunwayId,
                TargetGateId,
                _ => ValidFlightNumber));
    }
}