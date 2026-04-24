using System;
using System.Collections.Generic;
using System.Text;

using Moq;

using TicketSellingModule.Data.Repositories.Interfaces;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class AirportServiceTests
{
    [Fact]
    public void Add_Should_Throw_Exception_For_Null_Code()
    {
        Mock<IFlightRepository> mockFlightRepo = new Mock<IFlightRepository>();
        Mock<IAirportRepository> mockAirportRepo = new Mock<IAirportRepository>();

        AirportService airportService = new AirportService(mockAirportRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => airportService.Add(null, "Test", "Test"));
        Assert.Throws<ArgumentException>(() => airportService.Add(string.Empty, "Test", "Test"));
        Assert.Throws<ArgumentException>(() => airportService.Add(" ", "Test", "Test"));
    }

    [Fact]
    public void Add_Should_Throw_Exception_For_Null_Name()
    {
        Mock<IFlightRepository> mockFlightRepo = new Mock<IFlightRepository>();
        Mock<IAirportRepository> mockAirportRepo = new Mock<IAirportRepository>();

        AirportService airportService = new AirportService(mockAirportRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => airportService.Add("Test", null, "Test"));
        Assert.Throws<ArgumentException>(() => airportService.Add("Test", string.Empty, "Test"));
        Assert.Throws<ArgumentException>(() => airportService.Add("Test", " ", "Test"));
    }

    [Fact]
    public void Add_Should_Throw_Exception_For_Null_City()
    {
        Mock<IFlightRepository> mockFlightRepo = new Mock<IFlightRepository>();
        Mock<IAirportRepository> mockAirportRepo = new Mock<IAirportRepository>();

        AirportService airportService = new AirportService(mockAirportRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => airportService.Add("Test", "Test", null));
        Assert.Throws<ArgumentException>(() => airportService.Add("Test", "Test", string.Empty));
        Assert.Throws<ArgumentException>(() => airportService.Add("Test", "Test", " "));
    }

    [Fact]
    public void Add_Should_Work()
    {
        Mock<IFlightRepository> mockFlightRepo = new Mock<IFlightRepository>();
        Mock<IAirportRepository> mockAirportRepo = new Mock<IAirportRepository>();

        AirportService airportService = new AirportService(mockAirportRepo.Object, mockFlightRepo.Object);

        mockAirportRepo.Setup(repo => repo.AddAirport(It.IsAny<Airport>())).Returns(1);

        Assert.Equal(1, airportService.Add("Test", "Test", "Test"));
    }

    [Fact]
    public void GetAll_Should_Return_All_Airports()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        var airports = new List<Airport>
        {
            new Airport { AirportCode = "A", AirportName = "A1", City = "C1" },
            new Airport { AirportCode = "B", AirportName = "B1", City = "C2" }
        };

        mockAirportRepo.Setup(repo => repo.GetAllAirports()).Returns(airports);

        var service = new AirportService(mockAirportRepo.Object, mockFlightRepo.Object);

        var result = service.GetAll();

        Assert.Equal(2, result.Count);
        Assert.Equal(airports, result);
    }

    [Fact]
    public void GetById_Should_Return_Null_For_Invalid_Id()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        var service = new AirportService(mockAirportRepo.Object, mockFlightRepo.Object);

        var result = service.GetById(0);

        Assert.Null(result);
    }

    [Fact]
    public void GetById_Should_Return_Airport()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        var airport = new Airport { AirportCode = "A", AirportName = "Test", City = "City" };

        mockAirportRepo.Setup(repo => repo.GetAirportById(1)).Returns(airport);

        var service = new AirportService(mockAirportRepo.Object, mockFlightRepo.Object);

        var result = service.GetById(1);

        Assert.Equal(airport, result);
    }

    [Fact]
    public void Update_Should_Do_Nothing_If_Airport_Not_Found()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        mockAirportRepo.Setup(repo => repo.GetAirportById(1)).Returns((Airport)null);

        var service = new AirportService(mockAirportRepo.Object, mockFlightRepo.Object);

        service.Update(1, "NewCity");

        mockAirportRepo.Verify(repo => repo.UpdateAirport(It.IsAny<Airport>()), Times.Never);
    }

    [Fact]
    public void Update_Should_Update_Only_Name()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        var airport = new Airport
        {
            AirportCode = "OLD",
            AirportName = "OldName",
            City = "OldCity"
        };

        mockAirportRepo.Setup(r => r.GetAirportById(1)).Returns(airport);

        var service = new AirportService(mockAirportRepo.Object, mockFlightRepo.Object);

        service.Update(1, newName: "NewName");

        Assert.Equal("NewName", airport.AirportName);
        Assert.Equal("OldCity", airport.City);
        Assert.Equal("OLD", airport.AirportCode);

        mockAirportRepo.Verify(r => r.UpdateAirport(airport), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_Only_City()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        var airport = new Airport
        {
            AirportCode = "OLD",
            AirportName = "OldName",
            City = "OldCity"
        };

        mockAirportRepo.Setup(r => r.GetAirportById(1)).Returns(airport);

        var service = new AirportService(mockAirportRepo.Object, mockFlightRepo.Object);

        service.Update(1, newCity: "NewCity");

        Assert.Equal("NewCity", airport.City);
        Assert.Equal("OldName", airport.AirportName);
        Assert.Equal("OLD", airport.AirportCode);

        mockAirportRepo.Verify(r => r.UpdateAirport(airport), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_Only_Code()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        var airport = new Airport
        {
            AirportCode = "OLD",
            AirportName = "OldName",
            City = "OldCity"
        };

        mockAirportRepo.Setup(r => r.GetAirportById(1)).Returns(airport);

        var service = new AirportService(mockAirportRepo.Object, mockFlightRepo.Object);

        service.Update(1, newCode: "NEW");

        Assert.Equal("NEW", airport.AirportCode);
        Assert.Equal("OldName", airport.AirportName);
        Assert.Equal("OldCity", airport.City);

        mockAirportRepo.Verify(r => r.UpdateAirport(airport), Times.Once);
    }

    [Fact]
    public void Update_Should_Call_Repo_Even_When_No_Changes()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        var airport = new Airport
        {
            AirportCode = "OLD",
            AirportName = "OldName",
            City = "OldCity"
        };

        mockAirportRepo.Setup(r => r.GetAirportById(1)).Returns(airport);

        var service = new AirportService(mockAirportRepo.Object, mockFlightRepo.Object);

        service.Update(1);

        Assert.Equal("OldName", airport.AirportName);
        Assert.Equal("OldCity", airport.City);
        Assert.Equal("OLD", airport.AirportCode);

        mockAirportRepo.Verify(r => r.UpdateAirport(airport), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_All_Fields()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        var airport = new Airport
        {
            AirportCode = "OLD",
            AirportName = "OldName",
            City = "OldCity"
        };

        mockAirportRepo.Setup(repo => repo.GetAirportById(1)).Returns(airport);

        var service = new AirportService(mockAirportRepo.Object, mockFlightRepo.Object);

        service.Update(1, "NewCity", "NewName", "NEW");

        Assert.Equal("NewCity", airport.City);
        Assert.Equal("NewName", airport.AirportName);
        Assert.Equal("NEW", airport.AirportCode);

        mockAirportRepo.Verify(repo => repo.UpdateAirport(airport), Times.Once);
    }

    [Fact]
    public void Delete_Should_Not_Call_Repo_For_Invalid_Id()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        var service = new AirportService(mockAirportRepo.Object, mockFlightRepo.Object);

        service.Delete(0);

        mockAirportRepo.Verify(repo => repo.DeleteAirportUsingId(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void Delete_Should_Call_Repo_For_Valid_Id()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        var service = new AirportService(mockAirportRepo.Object, mockFlightRepo.Object);

        service.Delete(5);

        mockAirportRepo.Verify(repo => repo.DeleteAirportUsingId(5), Times.Once);
    }

    [Fact]
    public void HasFlights_Should_Return_True_When_Flights_Exist()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        var flights = new List<Flight> { new Flight() };

        mockFlightRepo.Setup(repo => repo.GetFlightsByAirportId(1)).Returns(flights);

        var service = new AirportService(mockAirportRepo.Object, mockFlightRepo.Object);

        var result = service.HasFlights(1);

        Assert.True(result);
    }

    [Fact]
    public void HasFlights_Should_Return_False_When_No_Flights_Exist()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        mockFlightRepo.Setup(repo => repo.GetFlightsByAirportId(1)).Returns(new List<Flight>());

        var service = new AirportService(mockAirportRepo.Object, mockFlightRepo.Object);

        var result = service.HasFlights(1);

        Assert.False(result);
    }
}
