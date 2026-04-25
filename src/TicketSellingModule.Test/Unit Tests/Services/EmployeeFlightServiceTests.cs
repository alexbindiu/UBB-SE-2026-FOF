using Moq;

using TicketSellingModule.Data;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class EmployeeFlightServiceTests
{
    private const int TargetFlightId = 1;
    private const int TargetEmployeeId = 1;
    private const int TargetRouteId = 1;
    private const int TargetRouteId2 = 2;
    private const int ConflictingFlightId = 99;
    private const int ConflictingRouteId = 2;
    private const int NextDayFlightId = 50;
    private const int InvalidId = 0;
    private const int DepartureTimeHour = 10;
    private const int DepartureTimeMinutes = 0;
    private const int ArrivalTimeHour = 12;
    private const int ArrivalTimeMinutes = 0;
    private const int ConflictingDepartureTimeHour = 10;
    private const int ConflictingDepartureTimeMinutes = 0;
    private const int ConflictingArrivalTimeHour = 12;
    private const int ConflictingArrivalTimeMinutes = 0;
    private const int MissingEmployeeId = 2;
    private const int MissingFlightId = 2;
    private const int MissingRouteId = 99;
    private const int FailingEmployeeId = 1;
    private const int SucceedingEmployeeId = 2;
    private const int EmployeeToRemoveId = 1;
    private const int EmployeeToKeepId = 2;
    private const int EmployeeToAddId = 3;
    private const string DefaultFlightCode = "FL-1000";
    private const string NormalizeUnknownResponseOnNull = "Unknown";
    private const string RelevantTimeNAResponseOnNull = "N/A";
    private const int FutureFlightId = 1;
    private const int CurrentFlightId = 2;
    private const string OneCharFlightType = "X";

    private const int AvailableEmployeeId = 1;
    private const int UnavailableEmployeeId = 2;

    private const int TargetDepartureHour = 10;
    private const int TargetArrivalHour = 12;
    private const int ConflictingDepartureHour = 11;
    private const int ConflictingArrivalHour = 13;

    private const int PilotAliceId = 2;
    private const int PilotCharlieId = 3;
    private const int CoPilotBobId = 1;
    private const string PilotAliceName = "Alice";
    private const string PilotCharlieName = "Charlie";
    private const string CoPilotBobName = "Bob";

    private static EmployeeFlightService CreateTestService(
        Mock<IEmployeeFlightRepository>? employeeFlightRepository = null,
        Mock<IEmployeeRepository>? employeeRepository = null,
        Mock<IFlightRepository>? flightRepository = null,
        Mock<IRouteRepository>? routeRepository = null,
        Mock<IGateService>? gateService = null,
        Mock<IRunwayService>? runwayService = null,
        Mock<IRouteService>? routeService = null)
    {
        return new EmployeeFlightService(
            (employeeFlightRepository ?? new Mock<IEmployeeFlightRepository>()).Object,
            (employeeRepository ?? new Mock<IEmployeeRepository>()).Object,
            (flightRepository ?? new Mock<IFlightRepository>()).Object,
            (routeRepository ?? new Mock<IRouteRepository>()).Object,
            (gateService ?? new Mock<IGateService>()).Object,
            (runwayService ?? new Mock<IRunwayService>()).Object,
            (routeService ?? new Mock<IRouteService>()).Object);
    }

    [Fact]
    public void AssignEmployeeToFlightUsingIds_ThrowsArgumentException_WhenFlightIdIsInvalid()
    {
        var employeeFlightService = CreateTestService();

        Assert.Throws<ArgumentException>(() => employeeFlightService.AssignEmployeeToFlightUsingIds(InvalidId, TargetEmployeeId));
    }

    [Fact]
    public void AssignEmployeeToFlightUsingIds_ThrowsArgumentException_WhenEmployeeIdIsInvalid()
    {
        var employeeFlightService = CreateTestService();

        Assert.Throws<ArgumentException>(() => employeeFlightService.AssignEmployeeToFlightUsingIds(TargetFlightId, InvalidId));
    }

    [Fact]
    public void AssignEmployeeToFlightUsingIds_ThrowsInvalidOperationException_WhenEmployeeDoesNotExist()
    {
        var employeeRepositoryThatReturnsNull = new Mock<IEmployeeRepository>();
        var flightRepository = new Mock<IFlightRepository>();

        employeeRepositoryThatReturnsNull.Setup(getNullInsteadOfEmployee => getNullInsteadOfEmployee.GetEmployeeById(TargetEmployeeId)).Returns((Employee?)null);
        flightRepository.Setup(getDefaultFlight => getDefaultFlight.GetFlightById(TargetFlightId)).Returns(new Flight());

        var employeeFlightService = CreateTestService(employeeRepository: employeeRepositoryThatReturnsNull, flightRepository: flightRepository);

        Assert.Throws<InvalidOperationException>(() => employeeFlightService.AssignEmployeeToFlightUsingIds(TargetFlightId, TargetEmployeeId));
    }

    [Fact]
    public void AssignEmployeeToFlightUsingIds_ThrowsInvalidOperationException_WhenFlightDoesNotExist()
    {
        var employeeRepository = new Mock<IEmployeeRepository>();
        var flightRepositoryThatReturnsNull = new Mock<IFlightRepository>();

        employeeRepository.Setup(getDefaultEmployee => getDefaultEmployee.GetEmployeeById(TargetEmployeeId)).Returns(new Employee());
        flightRepositoryThatReturnsNull.Setup(getNullInsteadOfFlight => getNullInsteadOfFlight.GetFlightById(TargetFlightId)).Returns((Flight?)null);

        var employeeFlightService = CreateTestService(employeeRepository: employeeRepository, flightRepository: flightRepositoryThatReturnsNull);

        Assert.Throws<InvalidOperationException>(() => employeeFlightService.AssignEmployeeToFlightUsingIds(TargetFlightId, TargetEmployeeId));
    }

    [Fact]
    public void AssignEmployeeToFlightUsingIds_ThrowsInvalidOperationException_WhenEmployeeIsAlreadyAssigned()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var employeeRepository = new Mock<IEmployeeRepository>();
        var flightRepository = new Mock<IFlightRepository>();

        employeeRepository.Setup(getDefaultEmployee => getDefaultEmployee.GetEmployeeById(TargetEmployeeId)).Returns(new Employee());
        flightRepository.Setup(getDefaultFlight => getDefaultFlight.GetFlightById(TargetFlightId)).Returns(new Flight { Id = TargetFlightId, Route = new Route { Id = TargetRouteId }, Date = DateTime.Today });

        var existingCrewList = new List<int> { TargetEmployeeId };
        employeeFlightRepository.Setup(getEmployeesAssignedToFlight => getEmployeesAssignedToFlight.GetEmployeesByFlightId(TargetFlightId)).Returns(existingCrewList);

        var employeeFlightService = CreateTestService(employeeFlightRepository: employeeFlightRepository, employeeRepository: employeeRepository, flightRepository: flightRepository);

        Assert.Throws<InvalidOperationException>(() => employeeFlightService.AssignEmployeeToFlightUsingIds(TargetFlightId, TargetEmployeeId));
    }

    [Fact]
    public void AssignEmployeeToFlightUsingIds_CallsRepository_WhenArgumentsAndScheduleAreValid()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var employeeRepository = new Mock<IEmployeeRepository>();
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var validFlight = new Flight { Id = TargetFlightId, Date = DateTime.Today, Route = new Route { Id = TargetRouteId } };
        var validRoute = new Route
        {
            DepartureTime = TimeOnly.FromDateTime(DateTime.Today),
            ArrivalTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(1))
        };

        employeeRepository.Setup(getDefaultEmployee => getDefaultEmployee.GetEmployeeById(TargetEmployeeId)).Returns(new Employee());
        flightRepository.Setup(getDefaultFlight => getDefaultFlight.GetFlightById(TargetFlightId)).Returns(validFlight);
        employeeFlightRepository.Setup(getEmployeesAssignedToFlight => getEmployeesAssignedToFlight.GetEmployeesByFlightId(TargetFlightId)).Returns(new List<int>());
        routeRepository.Setup(getDefaultRoute => getDefaultRoute.GetRouteById(It.IsAny<int>())).Returns(validRoute);
        employeeFlightRepository.Setup(getFlightsAssignedToEmployee => getFlightsAssignedToEmployee.GetFlightsByEmployeeId(TargetEmployeeId)).Returns(new List<int>());

        var employeeFlightService = CreateTestService(
            employeeFlightRepository: employeeFlightRepository,
            employeeRepository: employeeRepository,
            flightRepository: flightRepository,
            routeRepository: routeRepository);

        employeeFlightService.AssignEmployeeToFlightUsingIds(TargetFlightId, TargetEmployeeId);

        employeeFlightRepository.Verify(repositoryIsCalled => repositoryIsCalled.AssignFlightToEmployeeUsingIds(TargetEmployeeId, TargetFlightId), Times.Once);
    }

    [Fact]
    public void RemoveAllCrewAssignmentsForFlight_CallsRepository_WhenIdIsValid()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var employeeFlightService = CreateTestService(employeeFlightRepository: employeeFlightRepository);

        employeeFlightService.RemoveAllCrewAssignmentsForFlight(TargetFlightId);

        employeeFlightRepository.Verify(repositoryIsCalled => repositoryIsCalled.RemoveAllByFlightId(TargetFlightId), Times.Once);
    }

    [Fact]
    public void RemoveAllFlightsAssignmentsForEmployee_CallsRepository_WhenIdIsValid()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var employeeFlightService = CreateTestService(employeeFlightRepository: employeeFlightRepository);

        employeeFlightService.RemoveAllFlightsAssignmentsForEmployee(TargetEmployeeId);

        employeeFlightRepository.Verify(repositoryIsCalled => repositoryIsCalled.RemoveAllByEmployeeId(TargetEmployeeId), Times.Once);
    }

    [Fact]
    public void RemoveAllFlightsAssignmentsForEmployee_ShouldNotCallRepository_WhenIdIsNotValid()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var employeeFlightService = CreateTestService(employeeFlightRepository: employeeFlightRepository);

        employeeFlightService.RemoveAllFlightsAssignmentsForEmployee(InvalidId);

        employeeFlightRepository.Verify(doesNotCallRepository => doesNotCallRepository.RemoveAllByEmployeeId(TargetEmployeeId), Times.Never);
    }

    [Fact]
    public void RemoveEmployeeFromFlightUsingIds_CallsRepositoryWithCorrectIds()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var employeeFlightService = CreateTestService(employeeFlightRepository: employeeFlightRepository);

        employeeFlightService.RemoveEmployeeFromFlightUsingIds(TargetFlightId, TargetEmployeeId);

        employeeFlightRepository.Verify(repositoryIsCalled => repositoryIsCalled.RemoveFlightFromEmployeeUsingIds(TargetEmployeeId, TargetFlightId), Times.Once);
    }

    [Fact]
    public void RemoveEmployeeFromFlightUsingIds_CallsRepositoryWithInvalidEmployeeId()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var employeeFlightService = CreateTestService(employeeFlightRepository: employeeFlightRepository);

        employeeFlightService.RemoveEmployeeFromFlightUsingIds(TargetFlightId, InvalidId);

        employeeFlightRepository.Verify(doesNotCallRepository => doesNotCallRepository.RemoveFlightFromEmployeeUsingIds(TargetEmployeeId, InvalidId), Times.Never);
    }

    [Fact]
    public void RemoveEmployeeFromFlightUsingIds_CallsRepositoryWithInvaliddlightId()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var employeeFlightService = CreateTestService(employeeFlightRepository: employeeFlightRepository);

        employeeFlightService.RemoveEmployeeFromFlightUsingIds(InvalidId, TargetEmployeeId);

        employeeFlightRepository.Verify(doesNotCallRepository => doesNotCallRepository.RemoveFlightFromEmployeeUsingIds(InvalidId, TargetFlightId), Times.Never);
    }

    [Fact]
    public void GetCrewAssignedToFlight_ReturnsOnlyExistingEmployees_WhenIdsAreFound()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var employeeRepository = new Mock<IEmployeeRepository>();

        var assignedIds = new List<int> { TargetEmployeeId, MissingEmployeeId };

        employeeFlightRepository.Setup(getIdsOfAllEmployeesAssignedToflight => getIdsOfAllEmployeesAssignedToflight.GetEmployeesByFlightId(TargetFlightId)).Returns(assignedIds);
        employeeRepository.Setup(getExistingEmployee => getExistingEmployee.GetEmployeeById(TargetEmployeeId)).Returns(new Employee { Id = TargetEmployeeId });
        employeeRepository.Setup(getNullCauseIdNotInRepository => getNullCauseIdNotInRepository.GetEmployeeById(MissingEmployeeId)).Returns((Employee?)null);

        var employeeFlightService = CreateTestService(employeeFlightRepository: employeeFlightRepository, employeeRepository: employeeRepository);

        List<Employee> retrievedCrew = employeeFlightService.GetEmployeesAssignedToFlight(TargetFlightId);

        Assert.Single(retrievedCrew);
        Assert.Equal(TargetEmployeeId, retrievedCrew[0].Id);
    }

    [Fact]
    public void GetEmployeeSchedule_SkipsNullFlights_WhenRepositoryReturnsNullForId()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var flightRepository = new Mock<IFlightRepository>();

        var assignedFlightIds = new List<int> { TargetFlightId, MissingFlightId };

        employeeFlightRepository.Setup(getAllFlightsAssignedToEmployee => getAllFlightsAssignedToEmployee.GetFlightsByEmployeeId(TargetEmployeeId)).Returns(assignedFlightIds);
        flightRepository.Setup(getExistingFlight => getExistingFlight.GetFlightById(TargetFlightId)).Returns(new Flight { Id = TargetFlightId, Route = new Route { Id = TargetRouteId } });
        flightRepository.Setup(getNullCauseIdNotInRepository => getNullCauseIdNotInRepository.GetFlightById(MissingFlightId)).Returns((Flight?)null);

        var employeeFlightService = CreateTestService(employeeFlightRepository: employeeFlightRepository, flightRepository: flightRepository);

        List<Flight> retrievedSchedule = employeeFlightService.GetEmployeeSchedule(TargetEmployeeId);

        Assert.Single(retrievedSchedule);
        Assert.Equal(TargetFlightId, retrievedSchedule[0].Id);
    }

    [Fact]
    public void IsEmployeeAvailable_ReturnsFalse_WhenTargetRouteIsNotFound()
    {
        var routeRepositoryThatReturnsNull = new Mock<IRouteRepository>();
        routeRepositoryThatReturnsNull.Setup(getNullInsteadOfRoute => getNullInsteadOfRoute.GetRouteById(TargetRouteId)).Returns((Route?)null);

        var employeeFlightService = CreateTestService(routeRepository: routeRepositoryThatReturnsNull);

        bool isAvailable = employeeFlightService.IsEmployeeAvailable(TargetEmployeeId, DateTime.Today, TargetRouteId);

        Assert.False(isAvailable);
    }

    [Fact]
    public void IsEmployeeAvailable_ReturnsTrue_WhenNoFlightsOverlap()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var validRoute = new Route
        {
            Id = TargetRouteId,
            DepartureTime = TimeOnly.FromDateTime(DateTime.Today),
            ArrivalTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(1))
        };

        routeRepository.Setup(getDefaultRoute => getDefaultRoute.GetRouteById(It.IsAny<int>())).Returns(validRoute);
        employeeFlightRepository.Setup(getFlightsAssignedToEmployee => getFlightsAssignedToEmployee.GetFlightsByEmployeeId(TargetEmployeeId)).Returns(new List<int>());

        var employeeFlightService = CreateTestService(employeeFlightRepository: employeeFlightRepository, routeRepository: routeRepository);

        bool isAvailable = employeeFlightService.IsEmployeeAvailable(TargetEmployeeId, DateTime.Today, TargetRouteId);

        Assert.True(isAvailable);
    }

    [Fact]
    public void IsEmployeeAvailable_ReturnsFalse_WhenFlightTimesOverlap()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var targetRoute = new Route
        {
            Id = TargetRouteId,
            DepartureTime = new TimeOnly(DepartureTimeHour, DepartureTimeMinutes),
            ArrivalTime = new TimeOnly(ArrivalTimeHour, ArrivalTimeMinutes)
        };

        var existingConflictingRoute = new Route
        {
            Id = ConflictingRouteId,
            DepartureTime = new TimeOnly(ConflictingDepartureTimeHour, ConflictingDepartureTimeMinutes),
            ArrivalTime = new TimeOnly(ConflictingArrivalTimeHour, ConflictingArrivalTimeMinutes)
        };

        routeRepository.Setup(getDefaultRoute => getDefaultRoute.GetRouteById(TargetRouteId)).Returns(targetRoute);
        routeRepository.Setup(getConflictingRoute => getConflictingRoute.GetRouteById(ConflictingRouteId)).Returns(existingConflictingRoute);
        employeeFlightRepository.Setup(getIdsOfConflictingFlights => getIdsOfConflictingFlights.GetFlightsByEmployeeId(TargetEmployeeId)).Returns(new List<int> { ConflictingFlightId });
        flightRepository.Setup(getLostOfConflictingFlights => getLostOfConflictingFlights.GetFlightById(ConflictingFlightId)).Returns(new Flight { Id = ConflictingFlightId, Date = DateTime.Today, Route = new Route { Id = ConflictingRouteId } });

        var employeeFlightService = CreateTestService(
            employeeFlightRepository: employeeFlightRepository,
            flightRepository: flightRepository,
            routeRepository: routeRepository);

        bool isAvailable = employeeFlightService.IsEmployeeAvailable(TargetEmployeeId, DateTime.Today, TargetRouteId);

        Assert.False(isAvailable);
    }

    [Fact]
    public void IsEmployeeAvailable_ReturnsTrue_WhenConflictMatchesExcludedFlightId()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        DateTime targetDate = DateTime.Today;
        var existingFlight = new Flight { Id = ConflictingFlightId, Date = targetDate, Route = new Route { Id = TargetRouteId } };

        employeeFlightRepository.Setup(getConflictingFlightId => getConflictingFlightId.GetFlightsByEmployeeId(TargetEmployeeId)).Returns(new List<int> { ConflictingFlightId });
        flightRepository.Setup(getExistingFlight => getExistingFlight.GetFlightById(ConflictingFlightId)).Returns(existingFlight);
        routeRepository.Setup(getExistingRoute => getExistingRoute.GetRouteById(TargetRouteId)).Returns(existingFlight.Route);

        var employeeFlightService = CreateTestService(
            employeeFlightRepository: employeeFlightRepository,
            flightRepository: flightRepository,
            routeRepository: routeRepository);

        bool isAvailable = employeeFlightService.IsEmployeeAvailable(TargetEmployeeId, targetDate, TargetRouteId, excludedFlightId: ConflictingFlightId);

        Assert.True(isAvailable);
    }

    [Fact]
    public void IsEmployeeAvailable_ReturnsTrue_WhenExistingFlightIsOnDifferentDate()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var targetRoute = new Route
        {
            Id = TargetRouteId,
            DepartureTime = new TimeOnly(DepartureTimeHour, DepartureTimeMinutes),
            ArrivalTime = new TimeOnly(DepartureTimeHour, DepartureTimeMinutes)
        };

        var nextDayFlight = new Flight
        {
            Id = NextDayFlightId,
            Date = DateTime.Today.AddDays(1),
            Route = new Route { Id = ConflictingRouteId }
        };

        routeRepository.Setup(getDefaultRoute => getDefaultRoute.GetRouteById(TargetRouteId)).Returns(targetRoute);
        employeeFlightRepository.Setup(getNextDayFlightId => getNextDayFlightId.GetFlightsByEmployeeId(TargetEmployeeId)).Returns(new List<int> { NextDayFlightId });
        flightRepository.Setup(getNextDayFlight => getNextDayFlight.GetFlightById(NextDayFlightId)).Returns(nextDayFlight);

        var employeeFlightService = CreateTestService(
            employeeFlightRepository: employeeFlightRepository,
            flightRepository: flightRepository,
            routeRepository: routeRepository);

        bool isAvailable = employeeFlightService.IsEmployeeAvailable(TargetEmployeeId, DateTime.Today, TargetRouteId);

        Assert.True(isAvailable);
    }

    [Fact]
    public void IsEmployeeAvailable_ReturnsTrue_WhenExistingFlightRouteIsNull()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var targetRoute = new Route
        {
            Id = TargetRouteId,
            DepartureTime = new TimeOnly(DepartureTimeHour, DepartureTimeMinutes),
            ArrivalTime = new TimeOnly(ArrivalTimeHour, ArrivalTimeMinutes)
        };

        var scheduledFlightWithMissingRoute = new Flight { Id = TargetFlightId, Date = DateTime.Today, Route = new Route { Id = MissingRouteId } };

        routeRepository.Setup(getDefaultRoute => getDefaultRoute.GetRouteById(TargetRouteId)).Returns(targetRoute);
        routeRepository.Setup(getNullCauseMissingRouteId => getNullCauseMissingRouteId.GetRouteById(MissingRouteId)).Returns((Route?)null);
        employeeFlightRepository.Setup(getDefaultFlightId => getDefaultFlightId.GetFlightsByEmployeeId(TargetEmployeeId)).Returns(new List<int> { TargetFlightId });
        flightRepository.Setup(getFlightWithMissingRoute => getFlightWithMissingRoute.GetFlightById(TargetFlightId)).Returns(scheduledFlightWithMissingRoute);

        var employeeFlightService = CreateTestService(
            employeeFlightRepository: employeeFlightRepository,
            flightRepository: flightRepository,
            routeRepository: routeRepository);

        bool isAvailable = employeeFlightService.IsEmployeeAvailable(TargetEmployeeId, DateTime.Today, TargetRouteId);

        Assert.True(isAvailable);
    }

    [Fact]
    public void AssignMultipleEmployeesToFlight_ContinuesProcessing_WhenPartialFailureOccurs()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var employeeRepository = new Mock<IEmployeeRepository>();

        List<int> employeesToAssign = new List<int> { FailingEmployeeId, SucceedingEmployeeId };

        employeeRepository.Setup(throwsDbFailureException => throwsDbFailureException.GetEmployeeById(FailingEmployeeId)).Throws(new Exception("Simulated DB Failure"));
        employeeRepository.Setup(getEmployeeSuccessfuly => getEmployeeSuccessfuly.GetEmployeeById(SucceedingEmployeeId)).Returns(new Employee());

        var employeeFlightService = CreateTestService(
            employeeFlightRepository: employeeFlightRepository,
            employeeRepository: employeeRepository);

        employeeFlightService.AssignEmpolyeesToFlightUsingIds(TargetFlightId, employeesToAssign);

        employeeFlightRepository.Verify(doesNotCallRepository => doesNotCallRepository.AssignFlightToEmployeeUsingIds(SucceedingEmployeeId, TargetFlightId), Times.Never);
    }

    [Fact]
    public void UpdateEmployeesForFlightUsingIds_AddsNewAndRemovesMissingEmployees()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var employeeRepository = new Mock<IEmployeeRepository>();
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var existingCrewIds = new List<int> { EmployeeToRemoveId, EmployeeToKeepId };
        var updatedCrewIds = new List<int> { EmployeeToKeepId, EmployeeToAddId };

        employeeFlightRepository.Setup(getExistingCrewIds => getExistingCrewIds.GetEmployeesByFlightId(TargetFlightId)).Returns(existingCrewIds);
        employeeRepository.Setup(getDefaultEmployee => getDefaultEmployee.GetEmployeeById(It.IsAny<int>())).Returns(new Employee());
        flightRepository.Setup(getDefaultFlight => getDefaultFlight.GetFlightById(It.IsAny<int>())).Returns(new Flight { Id = TargetFlightId, Route = new Route { Id = TargetRouteId }, Date = DateTime.Today });
        routeRepository.Setup(getDefaultRoute => getDefaultRoute.GetRouteById(It.IsAny<int>())).Returns(new Route());
        employeeFlightRepository.Setup(getFlightsOfDefaultEmployee => getFlightsOfDefaultEmployee.GetFlightsByEmployeeId(It.IsAny<int>())).Returns(new List<int>());

        var employeeFlightService = CreateTestService(
            employeeFlightRepository: employeeFlightRepository,
            employeeRepository: employeeRepository,
            flightRepository: flightRepository,
            routeRepository: routeRepository);

        employeeFlightService.UpdateEmployeesForFlightUsingIds(TargetFlightId, updatedCrewIds);

        employeeFlightRepository.Verify(calledRepositoryToRemoveAllFlightsFromEmployee =>
    calledRepositoryToRemoveAllFlightsFromEmployee.RemoveFlightFromEmployeeUsingIds(EmployeeToRemoveId, TargetFlightId), Times.Once);

        employeeFlightRepository.Verify(calledRepositoryToAddTheFlightsToTheNewEmployee =>
            calledRepositoryToAddTheFlightsToTheNewEmployee.AssignFlightToEmployeeUsingIds(EmployeeToAddId, TargetFlightId), Times.Once);
    }

    [Fact]
    public void GetFormattedEmployeeSchedule_UsesDefaultPlaceholders_WhenGateOrRunwayIsNull()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();
        var runwayServiceMock = new Mock<IRunwayService>();
        var routeServiceMock = new Mock<IRouteService>();

        var incompleteflight = new Flight
        {
            Id = TargetFlightId,
            FlightNumber = DefaultFlightCode,
            Date = DateTime.Today,
            Route = new Route { Id = TargetRouteId },
            Gate = null,
            Runway = null
        };

        employeeFlightRepository.Setup(getDefaultFlightId => getDefaultFlightId.GetFlightsByEmployeeId(TargetEmployeeId)).Returns(new List<int> { TargetFlightId });
        flightRepository.Setup(getIncompleteFlight => getIncompleteFlight.GetFlightById(TargetFlightId)).Returns(incompleteflight);
        routeRepository.Setup(getNullFromIncompleteFlight => getNullFromIncompleteFlight.GetRouteById(TargetRouteId)).Returns(incompleteflight.Route);
        routeServiceMock.Setup(getUnknownType => getUnknownType.NormalizeFlightType(null)).Returns(NormalizeUnknownResponseOnNull);
        routeServiceMock.Setup(getNaTime => getNaTime.GetRelevantTime(It.IsAny<Route>())).Returns(RelevantTimeNAResponseOnNull);
        runwayServiceMock.Setup(getNullInsteadOfRunway => getNullInsteadOfRunway.GetRunwayById(0)).Returns((Runway?)null);

        var employeeFlightService = CreateTestService(
            employeeFlightRepository: employeeFlightRepository,
            flightRepository: flightRepository,
            routeRepository: routeRepository,
            runwayService: runwayServiceMock,
            routeService: routeServiceMock);

        EmployeeScheduleItem resultItem = employeeFlightService.GetFormattedEmployeeSchedule(TargetEmployeeId)[0];

        Assert.Equal("-", resultItem.GateName);
        Assert.Equal("-", resultItem.RunwayName);
    }

    [Fact]
    public void GetFormattedEmployeeSchedule_SortsResultsByDate()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();
        var runwayServiceMock = new Mock<IRunwayService>();
        var routeServiceMock = new Mock<IRouteService>();

        var futureFlight = new Flight { Id = FutureFlightId, Date = DateTime.Today.AddDays(1), Route = new Route { Id = TargetRouteId } };
        var currentFlight = new Flight { Id = CurrentFlightId, Date = DateTime.Today, Route = new Route { Id = TargetRouteId } };

        employeeFlightRepository.Setup(getCurrentAndFutureFlightsIds => getCurrentAndFutureFlightsIds.GetFlightsByEmployeeId(TargetEmployeeId)).Returns(new List<int> { FutureFlightId, CurrentFlightId });
        flightRepository.Setup(getFutureFlight => getFutureFlight.GetFlightById(FutureFlightId)).Returns(futureFlight);
        flightRepository.Setup(getCurrentFlight => getCurrentFlight.GetFlightById(CurrentFlightId)).Returns(currentFlight);
        routeRepository.Setup(getDefaultRoute => getDefaultRoute.GetRouteById(It.IsAny<int>())).Returns(new Route());
        routeServiceMock.Setup(getFlightType => getFlightType.NormalizeFlightType(It.IsAny<string>())).Returns(OneCharFlightType);
        routeServiceMock.Setup(getFlightType => getFlightType.GetRelevantTime(It.IsAny<Route>())).Returns(OneCharFlightType);
        runwayServiceMock.Setup(getNullInsteadOfRunway => getNullInsteadOfRunway.GetRunwayById(It.IsAny<int>())).Returns((Runway?)null);

        var employeeFlightService = CreateTestService(
            employeeFlightRepository: employeeFlightRepository,
            flightRepository: flightRepository,
            routeRepository: routeRepository,
            runwayService: runwayServiceMock,
            routeService: routeServiceMock);

        List<EmployeeScheduleItem> sortedResults = employeeFlightService.GetFormattedEmployeeSchedule(TargetEmployeeId);

        Assert.Equal(CurrentFlightId.ToString(), sortedResults[0].Id);
        Assert.Equal(FutureFlightId.ToString(), sortedResults[1].Id);
    }

    [Fact]
    public void GetFormattedEmployeeSchedule_ReturnsEmptyList_WhenIdIsInvalid()
    {
        var employeeFlightService = CreateTestService();

        List<EmployeeScheduleItem> resultList = employeeFlightService.GetFormattedEmployeeSchedule(InvalidId);

        Assert.Empty(resultList);
    }

    [Fact]
    public void GetFormattedEmployeeSchedule_ReturnsList_WhenDataIsValid()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var flightRepository = new Mock<IFlightRepository>();

        var validFlightIdsList = new List<int> { TargetFlightId };
        var validFlight = new Flight { Id = TargetFlightId, Route = new Route() };

        employeeFlightRepository.Setup(getValidFlightIds => getValidFlightIds.GetFlightsByEmployeeId(TargetEmployeeId)).Returns(validFlightIdsList);
        flightRepository.Setup(returnsValidFlight => returnsValidFlight.GetFlightById(TargetFlightId)).Returns(validFlight);

        var employeeFlightService = CreateTestService(
            employeeFlightRepository: employeeFlightRepository,
            flightRepository: flightRepository);

        List<EmployeeScheduleItem> resultList = employeeFlightService.GetFormattedEmployeeSchedule(TargetEmployeeId);

        Assert.NotEmpty(resultList);
    }

    [Fact]
    public void GetAvailableEmployeesSortedByRole_FiltersOutUnavailableEmployees()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var employeeRepository = new Mock<IEmployeeRepository>();
        var flightRepository = new Mock<IFlightRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var targetFlight = new Flight { Id = TargetFlightId, Date = DateTime.Today, Route = new Route { Id = TargetRouteId } };
        var allEmployeesList = new List<Employee>
        {
            new Employee { Id = AvailableEmployeeId },
            new Employee { Id = UnavailableEmployeeId }
        };

        var targetRoute = new Route
        {
            Id = TargetRouteId,
            DepartureTime = new TimeOnly(TargetDepartureHour, 0),
            ArrivalTime = new TimeOnly(TargetArrivalHour, 0)
        };

        var conflictingRoute = new Route
        {
            Id = ConflictingRouteId,
            DepartureTime = new TimeOnly(ConflictingDepartureHour, 0),
            ArrivalTime = new TimeOnly(ConflictingArrivalHour, 0)
        };

        var conflictingFlight = new Flight { Id = ConflictingFlightId, Date = DateTime.Today, Route = new Route { Id = ConflictingRouteId } };

        employeeRepository.Setup(getAllEmployees => getAllEmployees.GetAllEmployees()).Returns(allEmployeesList);
        routeRepository.Setup(getTargetRoute => getTargetRoute.GetRouteById(TargetRouteId)).Returns(targetRoute);

        employeeFlightRepository.Setup(getAvailableEmployee => getAvailableEmployee.GetFlightsByEmployeeId(AvailableEmployeeId)).Returns(new List<int>());

        employeeFlightRepository.Setup(getConflictingFlightId => getConflictingFlightId.GetFlightsByEmployeeId(UnavailableEmployeeId)).Returns(new List<int> { ConflictingFlightId });
        flightRepository.Setup(getConflictingFlight => getConflictingFlight.GetFlightById(ConflictingFlightId)).Returns(conflictingFlight);
        routeRepository.Setup(getConflictingRoute => getConflictingRoute.GetRouteById(ConflictingRouteId)).Returns(conflictingRoute);

        var employeeFlightService = CreateTestService(
            employeeFlightRepository: employeeFlightRepository,
            employeeRepository: employeeRepository,
            flightRepository: flightRepository,
            routeRepository: routeRepository);

        List<Employee> availableEmployeesResult = employeeFlightService.GetAvailableEmployeesGroupedByRole(targetFlight);

        Assert.Single(availableEmployeesResult);
        Assert.Equal(AvailableEmployeeId, availableEmployeesResult[0].Id);
    }

    [Fact]
    public void GetAvailableEmployeesSortedByRole_SortsByRoleThenName()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var employeeRepository = new Mock<IEmployeeRepository>();
        var routeRepository = new Mock<IRouteRepository>();

        var targetFlight = new Flight { Id = TargetFlightId, Date = DateTime.Today, Route = new Route { Id = TargetRouteId } };

        var unsortedEmployeesList = new List<Employee>
        {
            new Employee { Id = CoPilotBobId,   Name = CoPilotBobName,   Role = EmployeeRole.CoPilot },
            new Employee { Id = PilotAliceId,   Name = PilotAliceName,   Role = EmployeeRole.Pilot },
            new Employee { Id = PilotCharlieId, Name = PilotCharlieName, Role = EmployeeRole.Pilot }
        };

        employeeRepository.Setup(getUnsortedEmployeeList => getUnsortedEmployeeList.GetAllEmployees()).Returns(unsortedEmployeesList);
        routeRepository.Setup(getDefaultRoute => getDefaultRoute.GetRouteById(It.IsAny<int>())).Returns(new Route());
        employeeFlightRepository.Setup(getNoFlightsForAnyEmployee => getNoFlightsForAnyEmployee.GetFlightsByEmployeeId(It.IsAny<int>())).Returns(new List<int>());

        var employeeFlightService = CreateTestService(
            employeeFlightRepository: employeeFlightRepository,
            employeeRepository: employeeRepository,
            routeRepository: routeRepository);

        List<Employee> sortedResult = employeeFlightService.GetAvailableEmployeesGroupedByRole(targetFlight);

        Assert.Equal(3, sortedResult.Count);

        Assert.Equal(PilotAliceId, sortedResult[0].Id);
        Assert.Equal(PilotCharlieId, sortedResult[1].Id);
        Assert.Equal(CoPilotBobId, sortedResult[2].Id);
    }

    [Fact]
    public void RemoveAllFlightsAssignmentsForEmployee_DoesNotCallRepository_WhenIdIsInvalid()
    {
        var employeeFlightRepository = new Mock<IEmployeeFlightRepository>();
        var employeeFlightService = CreateTestService(employeeFlightRepository: employeeFlightRepository);

        employeeFlightService.RemoveAllFlightsAssignmentsForEmployee(InvalidId);

        employeeFlightRepository.Verify(doesNotCallRepo => doesNotCallRepo.RemoveAllByEmployeeId(It.IsAny<int>()), Times.Never);
    }
}