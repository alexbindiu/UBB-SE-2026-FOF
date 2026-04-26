using System;
using System.Collections.Generic;
using System.Text;

using Moq;

using TicketSellingModule.Data.Repositories.Interfaces;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class FlightServiceTests
{
    private string firstFlightNumber = "AA100";
    private string secondFlightNumber = "BB200";
    private int numberOfFlights = 2;
    private int negativeFlightId = -1;
    private int validFlightId = 1;
    private int invalidFlightId = 99;
    private int invalidRouteId = 0;
    private int validRouteId = 5;
    private int validRunwayId = 3;
    private int validGateId = 1;
    private DateTime flightDate = new DateTime(2024, 5, 1);
    private DateTime newFlightDate = new DateTime(2025, 6, 15);
    private int newRunwayId = 7;
    private int newGateId = 9;

    [Fact]
    public void GetAll_Should_Return_All_Flights_Always()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flights = new List<Flight>
        {
            new Flight { FlightNumber = firstFlightNumber },
            new Flight { FlightNumber = secondFlightNumber }
        };
        mockFlightRepo.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(flights);

        var flightService = new FlightService(mockFlightRepo.Object);
        var result = flightService.GetAllFlights();

        Assert.Equal(numberOfFlights, result.Count);
        Assert.Equal(flights, result);
    }

    [Fact]
    public void GetById_Should_Return_Null_For_Invalid_Id()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);

        Assert.Null(flightService.GetFlightById(negativeFlightId));
    }

    [Fact]
    public void GetById_Should_Return_Flight_When_Found()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flight = new Flight { FlightNumber = firstFlightNumber };
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(validFlightId)).Returns(flight);

        var flightService = new FlightService(mockFlightRepo.Object);
        var result = flightService.GetFlightById(validFlightId);

        Assert.Equal(flight, result);
    }

    [Fact]
    public void GetByRoute_Should_Return_Empty_List_For_Invalid_RouteId()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);

        var result = flightService.GetFlightsByRouteId(invalidRouteId);

        Assert.Empty(result);
        mockFlightRepo.Verify(getFlights => getFlights.GetFlightsByRouteId(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void GetByRoute_Should_Return_Flights_When_Found()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flights = new List<Flight> { new Flight { FlightNumber = firstFlightNumber } };
        mockFlightRepo.Setup(getFlights => getFlights.GetFlightsByRouteId(5)).Returns(flights);

        var flightService = new FlightService(mockFlightRepo.Object);
        var result = flightService.GetFlightsByRouteId(validRouteId);

        Assert.Equal(flights, result);
    }

    [Fact]
    public void Add_Should_Throw_For_Null_FlightNumber()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);
        Assert.Throws<ArgumentException>(() => flightService.AddFlight(null, validRouteId,
            DateTime.Now, validRunwayId, validGateId));
    }

    [Fact]
    public void Add_Should_Throw_For_Empty_FlightNumber()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);
        Assert.Throws<ArgumentException>(() => flightService.AddFlight(string.Empty, validRouteId,
            DateTime.Now, validRunwayId, validGateId));
    }

    [Fact]
    public void Add_Should_Throw_For_Whitespace_FlightNumber()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);
        Assert.Throws<ArgumentException>(() => flightService.AddFlight(" ", validRouteId,
            DateTime.Now, validRunwayId, validGateId));
    }

    [Fact]
    public void Add_Should_Throw_For_Invalid_RouteId()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => flightService.AddFlight(firstFlightNumber, invalidRouteId, DateTime.Now, validRunwayId, validGateId));
    }

    [Fact]
    public void Add_Should_Work_For_Valid_Data()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockFlightRepo.Setup(addFlight => addFlight.AddFlight(It.IsAny<Flight>())).Returns(validFlightId);

        var flightService = new FlightService(mockFlightRepo.Object);
        var result = flightService.AddFlight(firstFlightNumber, validRouteId, DateTime.Now, validRunwayId, validGateId);

        Assert.Equal(validFlightId, result);
        mockFlightRepo.Verify(addFlight => addFlight.AddFlight(It.IsAny<Flight>()), Times.Once);
    }

    [Fact]
    public void Update_Should_Throw_When_Flight_Not_Found()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(invalidFlightId)).Returns((Flight)null);

        var flightService = new FlightService(mockFlightRepo.Object);

        Assert.Throws<InvalidOperationException>(() => flightService.UpdateFlight(invalidFlightId));
    }

    [Fact]
    public void Update_Should_Update_Only_Date_When_FlightNumber_Is_Not_Provided()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flight = new Flight { FlightNumber = firstFlightNumber, Date = flightDate };
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(validFlightId)).Returns(flight);

        var flightService = new FlightService(mockFlightRepo.Object);
        flightService.UpdateFlight(validFlightId, date: newFlightDate);
        Assert.Equal(newFlightDate, flight.Date);
        Assert.Equal(firstFlightNumber, flight.FlightNumber);
        mockFlightRepo.Verify(updateFlight => updateFlight.UpdateFlight(flight), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_Only_FlightNumber_When_Date_Is_Not_Provided()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flight = new Flight { FlightNumber = firstFlightNumber, Date = flightDate };
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(validFlightId)).Returns(flight);
        var flightService = new FlightService(mockFlightRepo.Object);
        flightService.UpdateFlight(validFlightId, flightNumber: secondFlightNumber);

        Assert.Equal(secondFlightNumber, flight.FlightNumber);
        Assert.Equal(flightDate, flight.Date);
        mockFlightRepo.Verify(updateFlight => updateFlight.UpdateFlight(flight), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_Only_RunwayId_When_Other_Fields_Are_Not_Provided()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flight = new Flight { FlightNumber = firstFlightNumber, Runway = new Runway { Id = validRunwayId } };
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(validFlightId)).Returns(flight);
        var flightService = new FlightService(mockFlightRepo.Object);
        flightService.UpdateFlight(validFlightId, runwayId: newRunwayId);

        Assert.Equal(newRunwayId, flight.Runway.Id);
        Assert.Equal(firstFlightNumber, flight.FlightNumber);
        mockFlightRepo.Verify(updateFlight => updateFlight.UpdateFlight(flight), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_Only_GateId_When_Other_Fields_Are_Not_Provided()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flight = new Flight { FlightNumber = firstFlightNumber, Gate = new Gate { Id = validGateId } };
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(validFlightId)).Returns(flight);
        var flightService = new FlightService(mockFlightRepo.Object);
        flightService.UpdateFlight(validFlightId, gateId: newGateId);

        Assert.Equal(newGateId, flight.Gate.Id);
        Assert.Equal(firstFlightNumber, flight.FlightNumber);
        mockFlightRepo.Verify(updateFlight => updateFlight.UpdateFlight(flight), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_All_Fields_When_All_Fields_Are_Provided()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flight = new Flight
        {
            FlightNumber = firstFlightNumber,
            Date = flightDate,
            Runway = new Runway { Id = validRunwayId },
            Gate = new Gate { Id = validGateId }
        };
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(validFlightId)).Returns(flight);

        var flightService = new FlightService(mockFlightRepo.Object);
        var newDate = new DateTime(2025, 12, 1);
        flightService.UpdateFlight(validFlightId, newDate, secondFlightNumber, newRunwayId, newGateId);
        Assert.Equal(newDate, flight.Date);
        Assert.Equal(secondFlightNumber, flight.FlightNumber);
        Assert.Equal(newRunwayId, flight.Runway.Id);
        Assert.Equal(newGateId, flight.Gate.Id);
        mockFlightRepo.Verify(updateFlight => updateFlight.UpdateFlight(flight), Times.Once);
    }

    [Fact]
    public void Delete_Should_Throw_For_Invalid_Id()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);

        Assert.Throws<InvalidOperationException>(() => flightService.DeleteFlightUsingId(invalidFlightId));
    }

    [Fact]
    public void Delete_Should_Throw_When_Flight_Not_Found()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockFlightRepo.Setup(getNoFlight => getNoFlight.GetFlightById(invalidFlightId)).Returns((Flight)null);

        var flightService = new FlightService(mockFlightRepo.Object);

        Assert.Throws<InvalidOperationException>(() => flightService.DeleteFlightUsingId(invalidFlightId));
    }

    [Fact]
    public void Delete_Should_Call_Repo_For_Valid_Id()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(validFlightId)).Returns(new Flight());

        var flightService = new FlightService(mockFlightRepo.Object);
        flightService.DeleteFlightUsingId(validFlightId);

        mockFlightRepo.Verify(deleteFlight => deleteFlight.DeleteFlightUsingId(validFlightId), Times.Once);
    }
}
