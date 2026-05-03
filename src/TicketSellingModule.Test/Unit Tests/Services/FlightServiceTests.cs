using Moq;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class FlightServiceTests
{
    private const string FirstFlightNumber = "AA100";
    private const string SecondFlightNumber = "BB200";
    private const int NumberOfFlights = 2;
    private const int NegativeFlightId = -1;
    private const int ValidFlightId = 1;
    private const int InvalidFlightId = 99;
    private const int InvalidRouteId = 0;
    private const int ValidRouteId = 5;
    private const int ValidRunwayId = 3;
    private const int ValidGateId = 1;
    private static readonly DateTime FlightDate = new DateTime(2024, 5, 1);
    private static readonly DateTime NewFlightDate = new DateTime(2025, 6, 15);
    private static readonly DateTime NewFlightDate2 = new DateTime(2025, 12, 1);
    private const int NewRunwayId = 7;
    private const int NewGateId = 9;

    [Fact]
    public void GetAll_Should_Return_All_Flights_Always()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flights = new List<Flight>
        {
            new Flight { FlightNumber = FirstFlightNumber },
            new Flight { FlightNumber = SecondFlightNumber }
        };
        mockFlightRepo.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(flights);

        var flightService = new FlightService(mockFlightRepo.Object);
        var result = flightService.GetAllFlights();

        Assert.Equal(NumberOfFlights, result.Count);
        Assert.Equal(flights, result);
    }

    [Fact]
    public void GetById_Should_Return_Null_For_Invalid_Id()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);

        Assert.Null(flightService.GetFlightById(NegativeFlightId));
    }

    [Fact]
    public void GetById_Should_Return_Flight_When_Found()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flight = new Flight { FlightNumber = FirstFlightNumber };
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(ValidFlightId)).Returns(flight);

        var flightService = new FlightService(mockFlightRepo.Object);
        var result = flightService.GetFlightById(ValidFlightId);

        Assert.Equal(flight, result);
    }

    [Fact]
    public void GetByRoute_Should_Return_Empty_List_For_Invalid_RouteId()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);

        var result = flightService.GetFlightsByRouteId(InvalidRouteId);

        Assert.Empty(result);
        mockFlightRepo.Verify(doesNotCallRepository => doesNotCallRepository.GetFlightsByRouteId(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void GetByRoute_Should_Return_Flights_When_Found()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flights = new List<Flight> { new Flight { FlightNumber = FirstFlightNumber } };
        mockFlightRepo.Setup(getFlights => getFlights.GetFlightsByRouteId(ValidRouteId)).Returns(flights);

        var flightService = new FlightService(mockFlightRepo.Object);
        var result = flightService.GetFlightsByRouteId(ValidRouteId);

        Assert.Equal(flights, result);
    }

    [Fact]
    public void Add_Should_Throw_For_Null_FlightNumber()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);
        Assert.Throws<ArgumentException>(() => flightService.AddFlight(null, ValidRouteId,
            DateTime.Now, ValidRunwayId, ValidGateId));
    }

    [Fact]
    public void Add_Should_Throw_For_Empty_FlightNumber()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);
        Assert.Throws<ArgumentException>(() => flightService.AddFlight(string.Empty, ValidRouteId,
            DateTime.Now, ValidRunwayId, ValidGateId));
    }

    [Fact]
    public void Add_Should_Throw_For_Whitespace_FlightNumber()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);
        Assert.Throws<ArgumentException>(() => flightService.AddFlight(" ", ValidRouteId,
            DateTime.Now, ValidRunwayId, ValidGateId));
    }

    [Fact]
    public void Add_Should_Throw_For_Invalid_RouteId()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => flightService.AddFlight(FirstFlightNumber, InvalidRouteId, DateTime.Now, ValidRunwayId, ValidGateId));
    }

    [Fact]
    public void Add_Should_Work_For_Valid_Data()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockFlightRepo.Setup(addFlight => addFlight.AddFlight(It.IsAny<Flight>())).Returns(ValidFlightId);

        var flightService = new FlightService(mockFlightRepo.Object);
        var result = flightService.AddFlight(FirstFlightNumber, ValidRouteId, DateTime.Now, ValidRunwayId, ValidGateId);

        Assert.Equal(ValidFlightId, result);
        mockFlightRepo.Verify(addFlight => addFlight.AddFlight(It.IsAny<Flight>()), Times.Once);
    }

    [Fact]
    public void Update_Should_Throw_When_Flight_Not_Found()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(InvalidFlightId)).Returns((Flight)null);

        var flightService = new FlightService(mockFlightRepo.Object);

        Assert.Throws<InvalidOperationException>(() => flightService.UpdateFlight(InvalidFlightId));
    }

    [Fact]
    public void Update_Should_Update_Only_Date_When_FlightNumber_Is_Not_Provided()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flight = new Flight { FlightNumber = FirstFlightNumber, Date = FlightDate };
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(ValidFlightId)).Returns(flight);

        var flightService = new FlightService(mockFlightRepo.Object);
        flightService.UpdateFlight(ValidFlightId, date: NewFlightDate);
        Assert.Equal(NewFlightDate, flight.Date);
        Assert.Equal(FirstFlightNumber, flight.FlightNumber);
        mockFlightRepo.Verify(callsRepositoryToUpdateflight => callsRepositoryToUpdateflight.UpdateFlight(flight), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_Only_FlightNumber_When_Date_Is_Not_Provided()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flight = new Flight { FlightNumber = FirstFlightNumber, Date = FlightDate };
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(ValidFlightId)).Returns(flight);
        var flightService = new FlightService(mockFlightRepo.Object);
        flightService.UpdateFlight(ValidFlightId, flightNumber: SecondFlightNumber);

        Assert.Equal(SecondFlightNumber, flight.FlightNumber);
        Assert.Equal(FlightDate, flight.Date);
        mockFlightRepo.Verify(callsRepostirotyToUpdateFlight => callsRepostirotyToUpdateFlight.UpdateFlight(flight), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_Only_RunwayId_When_Other_Fields_Are_Not_Provided()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flight = new Flight { FlightNumber = FirstFlightNumber, Runway = new Runway { Id = ValidRunwayId } };
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(ValidFlightId)).Returns(flight);
        var flightService = new FlightService(mockFlightRepo.Object);
        flightService.UpdateFlight(ValidFlightId, runwayId: NewRunwayId);

        Assert.Equal(NewRunwayId, flight.Runway.Id);
        Assert.Equal(FirstFlightNumber, flight.FlightNumber);
        mockFlightRepo.Verify(callsRepositoryToUpdateflight => callsRepositoryToUpdateflight.UpdateFlight(flight), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_Only_GateId_When_Other_Fields_Are_Not_Provided()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flight = new Flight { FlightNumber = FirstFlightNumber, Gate = new Gate { Id = ValidGateId } };
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(ValidFlightId)).Returns(flight);
        var flightService = new FlightService(mockFlightRepo.Object);
        flightService.UpdateFlight(ValidFlightId, gateId: NewGateId);

        Assert.Equal(NewGateId, flight.Gate.Id);
        Assert.Equal(FirstFlightNumber, flight.FlightNumber);
        mockFlightRepo.Verify(callsRepositoryToUpdateFlight => callsRepositoryToUpdateFlight.UpdateFlight(flight), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_All_Fields_When_All_Fields_Are_Provided()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flight = new Flight
        {
            FlightNumber = FirstFlightNumber,
            Date = FlightDate,
            Runway = new Runway { Id = ValidRunwayId },
            Gate = new Gate { Id = ValidGateId }
        };
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(ValidFlightId)).Returns(flight);

        var flightService = new FlightService(mockFlightRepo.Object);
        var newDate = NewFlightDate2;
        flightService.UpdateFlight(ValidFlightId, newDate, SecondFlightNumber, NewRunwayId, NewGateId);
        Assert.Equal(newDate, flight.Date);
        Assert.Equal(SecondFlightNumber, flight.FlightNumber);
        Assert.Equal(NewRunwayId, flight.Runway.Id);
        Assert.Equal(NewGateId, flight.Gate.Id);
        mockFlightRepo.Verify(callsRepositoryToUpdateFlight => callsRepositoryToUpdateFlight.UpdateFlight(flight), Times.Once);
    }

    [Fact]
    public void Delete_Should_Throw_For_Invalid_Id()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);

        Assert.Throws<InvalidOperationException>(() => flightService.DeleteFlightUsingId(InvalidFlightId));
    }

    [Fact]
    public void Delete_Should_Throw_When_Flight_Not_Found()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockFlightRepo.Setup(getNoFlight => getNoFlight.GetFlightById(InvalidFlightId)).Returns((Flight)null);

        var flightService = new FlightService(mockFlightRepo.Object);

        Assert.Throws<InvalidOperationException>(() => flightService.DeleteFlightUsingId(InvalidFlightId));
    }

    [Fact]
    public void Delete_Should_Call_Repo_For_Valid_Id()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(ValidFlightId)).Returns(new Flight());

        var flightService = new FlightService(mockFlightRepo.Object);
        flightService.DeleteFlightUsingId(ValidFlightId);

        mockFlightRepo.Verify(callsRepositoryToDeleteFlight => callsRepositoryToDeleteFlight.DeleteFlightUsingId(ValidFlightId), Times.Once);
    }
}
