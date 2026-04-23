using System;
using System.Collections.Generic;
using System.Text;

using Moq;

using TicketSellingModule.Data.Repositories.Interfaces;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class FlightServiceTests
{
    [Fact]
    public void GetAll_Should_Return_All_Flights()
    {
        var mockFlightRepo = new Mock<IFlightRepo>();
        var flights = new List<Flight>
        {
            new Flight { FlightNumber = "AA100" },
            new Flight { FlightNumber = "BB200" }
        };
        mockFlightRepo.Setup(r => r.GetAll()).Returns(flights);

        var service = new FlightService(mockFlightRepo.Object);
        var result = service.GetAll();

        Assert.Equal(2, result.Count);
        Assert.Equal(flights, result);
    }

    [Fact]
    public void GetById_Should_Return_Null_For_Invalid_Id()
    {
        var mockFlightRepo = new Mock<IFlightRepo>();
        var service = new FlightService(mockFlightRepo.Object);

        Assert.Null(service.GetById(0));
        Assert.Null(service.GetById(-1));
    }

    [Fact]
    public void GetById_Should_Return_Flight()
    {
        var mockFlightRepo = new Mock<IFlightRepo>();
        var flight = new Flight { FlightNumber = "AA100" };
        mockFlightRepo.Setup(r => r.GetById(1)).Returns(flight);

        var service = new FlightService(mockFlightRepo.Object);
        var result = service.GetById(1);

        Assert.Equal(flight, result);
    }

    [Fact]
    public void GetByRoute_Should_Return_Empty_List_For_Invalid_RouteId()
    {
        var mockFlightRepo = new Mock<IFlightRepo>();
        var service = new FlightService(mockFlightRepo.Object);

        var result = service.GetByRoute(0);

        Assert.Empty(result);
        mockFlightRepo.Verify(r => r.GetFlightsByRoute(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void GetByRoute_Should_Return_Flights()
    {
        var mockFlightRepo = new Mock<IFlightRepo>();
        var flights = new List<Flight> { new Flight { FlightNumber = "AA100" } };
        mockFlightRepo.Setup(r => r.GetFlightsByRoute(5)).Returns(flights);

        var service = new FlightService(mockFlightRepo.Object);
        var result = service.GetByRoute(5);

        Assert.Equal(flights, result);
    }

    [Fact]
    public void Add_Should_Throw_For_Null_Or_Empty_FlightNumber()
    {
        var mockFlightRepo = new Mock<IFlightRepo>();
        var service = new FlightService(mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => service.Add(null, 1, DateTime.Now, 1, 1));
        Assert.Throws<ArgumentException>(() => service.Add(string.Empty, 1, DateTime.Now, 1, 1));
        Assert.Throws<ArgumentException>(() => service.Add(" ", 1, DateTime.Now, 1, 1));
    }

    [Fact]
    public void Add_Should_Throw_For_Invalid_RouteId()
    {
        var mockFlightRepo = new Mock<IFlightRepo>();
        var service = new FlightService(mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => service.Add("AA100", 0, DateTime.Now, 1, 1));
        Assert.Throws<ArgumentException>(() => service.Add("AA100", -5, DateTime.Now, 1, 1));
    }

    [Fact]
    public void Add_Should_Work()
    {
        var mockFlightRepo = new Mock<IFlightRepo>();
        mockFlightRepo.Setup(r => r.Add(It.IsAny<Flight>())).Returns(42);

        var service = new FlightService(mockFlightRepo.Object);
        var result = service.Add("AA100", 1, DateTime.Now, 2, 3);

        Assert.Equal(42, result);
        mockFlightRepo.Verify(r => r.Add(It.IsAny<Flight>()), Times.Once);
    }

    [Fact]
    public void Update_Should_Throw_When_Flight_Not_Found()
    {
        var mockFlightRepo = new Mock<IFlightRepo>();
        mockFlightRepo.Setup(r => r.GetById(99)).Returns((Flight)null);

        var service = new FlightService(mockFlightRepo.Object);

        Assert.Throws<InvalidOperationException>(() => service.Update(99));
    }

    [Fact]
    public void Update_Should_Update_Only_Date()
    {
        var mockFlightRepo = new Mock<IFlightRepo>();
        var flight = new Flight { FlightNumber = "AA100", Date = new DateTime(2024, 1, 1) };
        mockFlightRepo.Setup(r => r.GetById(1)).Returns(flight);

        var service = new FlightService(mockFlightRepo.Object);
        var newDate = new DateTime(2025, 6, 15);
        service.Update(1, date: newDate);

        Assert.Equal(newDate, flight.Date);
        Assert.Equal("AA100", flight.FlightNumber);
        mockFlightRepo.Verify(r => r.Update(flight), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_Only_FlightNumber()
    {
        var mockFlightRepo = new Mock<IFlightRepo>();
        var flight = new Flight { FlightNumber = "OLD", Date = new DateTime(2024, 1, 1) };
        mockFlightRepo.Setup(r => r.GetById(1)).Returns(flight);

        var service = new FlightService(mockFlightRepo.Object);
        service.Update(1, flightNumber: "NEW");

        Assert.Equal("NEW", flight.FlightNumber);
        Assert.Equal(new DateTime(2024, 1, 1), flight.Date);
        mockFlightRepo.Verify(r => r.Update(flight), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_Only_RunwayId()
    {
        var mockFlightRepo = new Mock<IFlightRepo>();
        var flight = new Flight { FlightNumber = "AA100", Runway = new Runway { Id = 1 } };
        mockFlightRepo.Setup(r => r.GetById(1)).Returns(flight);

        var service = new FlightService(mockFlightRepo.Object);
        service.Update(1, runwayId: 7);

        Assert.Equal(7, flight.Runway.Id);
        Assert.Equal("AA100", flight.FlightNumber);
        mockFlightRepo.Verify(r => r.Update(flight), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_Only_GateId()
    {
        var mockFlightRepo = new Mock<IFlightRepo>();
        var flight = new Flight { FlightNumber = "AA100", Gate = new Gate { Id = 1 } };
        mockFlightRepo.Setup(r => r.GetById(1)).Returns(flight);

        var service = new FlightService(mockFlightRepo.Object);
        service.Update(1, gateId: 9);

        Assert.Equal(9, flight.Gate.Id);
        Assert.Equal("AA100", flight.FlightNumber);
        mockFlightRepo.Verify(r => r.Update(flight), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_All_Fields()
    {
        var mockFlightRepo = new Mock<IFlightRepo>();
        var flight = new Flight
        {
            FlightNumber = "OLD",
            Date = new DateTime(2024, 1, 1),
            Runway = new Runway { Id = 1 },
            Gate = new Gate { Id = 1 }
        };
        mockFlightRepo.Setup(r => r.GetById(1)).Returns(flight);

        var service = new FlightService(mockFlightRepo.Object);
        var newDate = new DateTime(2025, 12, 1);
        service.Update(1, newDate, "NEW", 5, 6);

        Assert.Equal(newDate, flight.Date);
        Assert.Equal("NEW", flight.FlightNumber);
        Assert.Equal(5, flight.Runway.Id);
        Assert.Equal(6, flight.Gate.Id);
        mockFlightRepo.Verify(r => r.Update(flight), Times.Once);
    }

    [Fact]
    public void Delete_Should_Throw_For_Invalid_Id()
    {
        var mockFlightRepo = new Mock<IFlightRepo>();
        var service = new FlightService(mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => service.Delete(0));
        Assert.Throws<ArgumentException>(() => service.Delete(-1));
    }

    [Fact]
    public void Delete_Should_Throw_When_Flight_Not_Found()
    {
        var mockFlightRepo = new Mock<IFlightRepo>();
        mockFlightRepo.Setup(r => r.GetById(5)).Returns((Flight)null);

        var service = new FlightService(mockFlightRepo.Object);

        Assert.Throws<InvalidOperationException>(() => service.Delete(5));
    }

    [Fact]
    public void Delete_Should_Call_Repo_For_Valid_Id()
    {
        var mockFlightRepo = new Mock<IFlightRepo>();
        mockFlightRepo.Setup(r => r.GetById(5)).Returns(new Flight());

        var service = new FlightService(mockFlightRepo.Object);
        service.Delete(5);

        mockFlightRepo.Verify(r => r.Delete(5), Times.Once);
    }
}
