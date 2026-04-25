using System;
using System.Collections.Generic;
using System.Text;

using Moq;

using TicketSellingModule.Data.Repositories.Interfaces;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class FlightServiceTests
{
    [Fact]
    public void GetAll_Should_Return_All_Flights_Always()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flights = new List<Flight>
        {
            new Flight { FlightNumber = "AA100" },
            new Flight { FlightNumber = "BB200" }
        };
        mockFlightRepo.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(flights);

        var flightService = new FlightService(mockFlightRepo.Object);
        var result = flightService.GetAllFlights();

        Assert.Equal(2, result.Count);
        Assert.Equal(flights, result);
    }

    [Fact]
    public void GetById_Should_Return_Null_For_Invalid_Id()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);

        Assert.Null(flightService.GetFlightById(-1));
    }

    [Fact]
    public void GetById_Should_Return_Flight_When_Found()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flight = new Flight { FlightNumber = "AA100" };
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(1)).Returns(flight);

        var flightService = new FlightService(mockFlightRepo.Object);
        var result = flightService.GetFlightById(1);

        Assert.Equal(flight, result);
    }

    [Fact]
    public void GetByRoute_Should_Return_Empty_List_For_Invalid_RouteId()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);

        var result = flightService.GetFlightsByRouteId(0);

        Assert.Empty(result);
        mockFlightRepo.Verify(getFlights => getFlights.GetFlightsByRouteId(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void GetByRoute_Should_Return_Flights_When_Found()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flights = new List<Flight> { new Flight { FlightNumber = "AA100" } };
        mockFlightRepo.Setup(getFlights => getFlights.GetFlightsByRouteId(5)).Returns(flights);

        var flightService = new FlightService(mockFlightRepo.Object);
        var result = flightService.GetFlightsByRouteId(5);

        Assert.Equal(flights, result);
    }

    [Fact]
    public void Add_Should_Throw_For_Null_FlightNumber()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);
        Assert.Throws<ArgumentException>(() => flightService.AddFlight(null, 1, DateTime.Now, 1, 1));
    }

    [Fact]
    public void Add_Should_Throw_For_Empty_FlightNumber()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);
        Assert.Throws<ArgumentException>(() => flightService.AddFlight(string.Empty, 1, DateTime.Now, 1, 1));
    }

    [Fact]
    public void Add_Should_Throw_For_Whitespace_FlightNumber()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);
        Assert.Throws<ArgumentException>(() => flightService.AddFlight(" ", 1, DateTime.Now, 1, 1));
    }

    [Fact]
    public void Add_Should_Throw_For_Invalid_RouteId()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => flightService.AddFlight("AA100", -5, DateTime.Now, 1, 1));
    }

    [Fact]
    public void Add_Should_Work_For_Valid_Data()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockFlightRepo.Setup(addFlight => addFlight.AddFlight(It.IsAny<Flight>())).Returns(42);

        var flightService = new FlightService(mockFlightRepo.Object);
        var result = flightService.AddFlight("AA100", 1, DateTime.Now, 2, 3);

        Assert.Equal(42, result);
        mockFlightRepo.Verify(addFlight => addFlight.AddFlight(It.IsAny<Flight>()), Times.Once);
    }

    [Fact]
    public void Update_Should_Throw_When_Flight_Not_Found()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(99)).Returns((Flight)null);

        var flightService = new FlightService(mockFlightRepo.Object);

        Assert.Throws<InvalidOperationException>(() => flightService.UpdateFlight(99));
    }

    [Fact]
    public void Update_Should_Update_Only_Date_When_FlightNumber_Is_Not_Provided()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flight = new Flight { FlightNumber = "AA100", Date = new DateTime(2024, 1, 1) };
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(1)).Returns(flight);

        var flightService = new FlightService(mockFlightRepo.Object);
        var newDate = new DateTime(2025, 6, 15);
        flightService.UpdateFlight(1, date: newDate);
        Assert.Equal(newDate, flight.Date);
        Assert.Equal("AA100", flight.FlightNumber);
        mockFlightRepo.Verify(updateFlight => updateFlight.UpdateFlight(flight), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_Only_FlightNumber_When_Date_Is_Not_Provided()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flight = new Flight { FlightNumber = "OLD", Date = new DateTime(2024, 1, 1) };
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(1)).Returns(flight);
        var flightService = new FlightService(mockFlightRepo.Object);
        flightService.UpdateFlight(1, flightNumber: "NEW");

        Assert.Equal("NEW", flight.FlightNumber);
        Assert.Equal(new DateTime(2024, 1, 1), flight.Date);
        mockFlightRepo.Verify(updateFlight => updateFlight.UpdateFlight(flight), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_Only_RunwayId_When_Other_Fields_Are_Not_Provided()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flight = new Flight { FlightNumber = "AA100", Runway = new Runway { Id = 1 } };
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(1)).Returns(flight);
        var flightService = new FlightService(mockFlightRepo.Object);
        flightService.UpdateFlight(1, runwayId: 7);

        Assert.Equal(7, flight.Runway.Id);
        Assert.Equal("AA100", flight.FlightNumber);
        mockFlightRepo.Verify(updateFlight => updateFlight.UpdateFlight(flight), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_Only_GateId_When_Other_Fields_Are_Not_Provided()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flight = new Flight { FlightNumber = "AA100", Gate = new Gate { Id = 1 } };
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(1)).Returns(flight);
        var flightService = new FlightService(mockFlightRepo.Object);
        flightService.UpdateFlight(1, gateId: 9);

        Assert.Equal(9, flight.Gate.Id);
        Assert.Equal("AA100", flight.FlightNumber);
        mockFlightRepo.Verify(updateFlight => updateFlight.UpdateFlight(flight), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_All_Fields_When_All_Fields_Are_Provided()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flight = new Flight
        {
            FlightNumber = "OLD",
            Date = new DateTime(2024, 1, 1),
            Runway = new Runway { Id = 1 },
            Gate = new Gate { Id = 1 }
        };
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(1)).Returns(flight);

        var flightService = new FlightService(mockFlightRepo.Object);
        var newDate = new DateTime(2025, 12, 1);
        flightService.UpdateFlight(1, newDate, "NEW", 5, 6);
        Assert.Equal(newDate, flight.Date);
        Assert.Equal("NEW", flight.FlightNumber);
        Assert.Equal(5, flight.Runway.Id);
        Assert.Equal(6, flight.Gate.Id);
        mockFlightRepo.Verify(updateFlight => updateFlight.UpdateFlight(flight), Times.Once);
    }

    [Fact]
    public void Delete_Should_Throw_For_Invalid_Id()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flightService = new FlightService(mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => flightService.DeleteFlightUsingId(-1));
    }

    [Fact]
    public void Delete_Should_Throw_When_Flight_Not_Found()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockFlightRepo.Setup(getNoFlight => getNoFlight.GetFlightById(5)).Returns((Flight)null);

        var flightService = new FlightService(mockFlightRepo.Object);

        Assert.Throws<InvalidOperationException>(() => flightService.DeleteFlightUsingId(5));
    }

    [Fact]
    public void Delete_Should_Call_Repo_For_Valid_Id()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(5)).Returns(new Flight());

        var flightService = new FlightService(mockFlightRepo.Object);
        flightService.DeleteFlightUsingId(5);

        mockFlightRepo.Verify(deleteFlight => deleteFlight.DeleteFlightUsingId(5), Times.Once);
    }
}
