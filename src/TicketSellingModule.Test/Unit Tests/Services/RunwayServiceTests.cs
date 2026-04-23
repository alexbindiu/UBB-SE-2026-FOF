using System;
using System.Collections.Generic;
using System.Text;

using Moq;

using TicketSellingModule.Data.Repositories.Interfaces;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class RunwayServiceTests
{
    [Fact]
    public void GetAll_Should_Return_All_Runways()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        var runways = new List<Runway>
        {
            new Runway { Name = "R1", HandleTime = 10 },
            new Runway { Name = "R2", HandleTime = 15 }
        };
        mockRunwayRepo.Setup(r => r.GetAllRunways()).Returns(runways);

        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        var result = service.GetAll();

        Assert.Equal(2, result.Count);
        Assert.Equal(runways, result);
    }

    [Fact]
    public void GetById_Should_Throw_For_Invalid_Id()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => service.GetById(0));
        Assert.Throws<ArgumentException>(() => service.GetById(-1));
    }

    [Fact]
    public void GetById_Should_Throw_When_Runway_Not_Found()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        mockRunwayRepo.Setup(r => r.GetRunwayById(5)).Returns((Runway)null);

        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<InvalidOperationException>(() => service.GetById(5));
    }

    [Fact]
    public void GetById_Should_Return_Runway()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        var runway = new Runway { Name = "R1", HandleTime = 10 };
        mockRunwayRepo.Setup(r => r.GetRunwayById(1)).Returns(runway);

        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        var result = service.GetById(1);

        Assert.Equal(runway, result);
    }

    [Fact]
    public void GetByIdSafe_Should_Return_Null_For_Invalid_Id()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Null(service.GetByIdSafe(0));
        Assert.Null(service.GetByIdSafe(-1));
    }

    [Fact]
    public void GetByIdSafe_Should_Return_Runway()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        var runway = new Runway { Name = "R1" };
        mockRunwayRepo.Setup(r => r.GetRunwayById(1)).Returns(runway);

        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Equal(runway, service.GetByIdSafe(1));
    }

    [Fact]
    public void GetByIdSafe_Should_Return_Null_When_Repo_Throws()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        mockRunwayRepo.Setup(r => r.GetRunwayById(It.IsAny<int>())).Throws<Exception>();

        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Null(service.GetByIdSafe(1));
    }

    [Fact]
    public void Add_Should_Throw_For_Null_Or_Empty_Name()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => service.Add(null, 10));
        Assert.Throws<ArgumentException>(() => service.Add(string.Empty, 10));
        Assert.Throws<ArgumentException>(() => service.Add(" ", 10));
    }

    [Fact]
    public void Add_Should_Throw_For_Invalid_HandleTime()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => service.Add("R1", 0));
        Assert.Throws<ArgumentException>(() => service.Add("R1", -5));
    }

    [Fact]
    public void Add_Should_Work()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        mockRunwayRepo.Setup(r => r.AddRunway(It.IsAny<Runway>())).Returns(7);

        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        var result = service.Add("R1", 10);

        Assert.Equal(7, result);
        mockRunwayRepo.Verify(r => r.AddRunway(It.IsAny<Runway>()), Times.Once);
    }

    [Fact]
    public void Update_Should_Throw_When_Runway_Not_Found()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        mockRunwayRepo.Setup(r => r.GetRunwayById(1)).Returns((Runway)null);

        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<InvalidOperationException>(() => service.Update(1, "NewName"));
    }

    [Fact]
    public void Update_Should_Throw_For_Whitespace_New_Name()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        mockRunwayRepo.Setup(r => r.GetRunwayById(1)).Returns(new Runway { Name = "Old", HandleTime = 10 });

        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => service.Update(1, " "));
        Assert.Throws<ArgumentException>(() => service.Update(1, string.Empty));
    }

    [Fact]
    public void Update_Should_Throw_For_Invalid_HandleTime()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        mockRunwayRepo.Setup(r => r.GetRunwayById(1)).Returns(new Runway { Name = "Old", HandleTime = 10 });

        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => service.Update(1, newHandleTime: 0));
        Assert.Throws<ArgumentException>(() => service.Update(1, newHandleTime: -1));
    }

    [Fact]
    public void Update_Should_Update_Only_Name()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        var runway = new Runway { Name = "OldName", HandleTime = 10 };
        mockRunwayRepo.Setup(r => r.GetRunwayById(1)).Returns(runway);

        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        service.Update(1, newName: "NewName");

        Assert.Equal("NewName", runway.Name);
        Assert.Equal(10, runway.HandleTime);
        mockRunwayRepo.Verify(r => r.UpdateRunway(runway), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_Only_HandleTime()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        var runway = new Runway { Name = "OldName", HandleTime = 10 };
        mockRunwayRepo.Setup(r => r.GetRunwayById(1)).Returns(runway);

        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        service.Update(1, newHandleTime: 25);

        Assert.Equal("OldName", runway.Name);
        Assert.Equal(25, runway.HandleTime);
        mockRunwayRepo.Verify(r => r.UpdateRunway(runway), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_All_Fields()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        var runway = new Runway { Name = "OldName", HandleTime = 10 };
        mockRunwayRepo.Setup(r => r.GetRunwayById(1)).Returns(runway);

        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        service.Update(1, "NewName", 30);

        Assert.Equal("NewName", runway.Name);
        Assert.Equal(30, runway.HandleTime);
        mockRunwayRepo.Verify(r => r.UpdateRunway(runway), Times.Once);
    }

    [Fact]
    public void Delete_Should_Throw_When_Runway_Not_Found()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        mockRunwayRepo.Setup(r => r.GetRunwayById(1)).Returns((Runway)null);

        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<InvalidOperationException>(() => service.Delete(1));
    }

    [Fact]
    public void Delete_Should_Call_Repo_For_Valid_Id()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        mockRunwayRepo.Setup(r => r.GetRunwayById(3)).Returns(new Runway { Name = "R3" });

        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        service.Delete(3);

        mockRunwayRepo.Verify(r => r.DeleteRunway(3), Times.Once);
    }

    [Fact]
    public void SaveRunway_Should_Throw_For_Invalid_HandleTime_Text()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => service.SaveRunway(0, "R1", "abc"));
        Assert.Throws<ArgumentException>(() => service.SaveRunway(0, "R1", "0"));
        Assert.Throws<ArgumentException>(() => service.SaveRunway(0, "R1", "-5"));
    }

    [Fact]
    public void SaveRunway_Should_Call_Add_When_Id_Is_Zero()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        mockRunwayRepo.Setup(r => r.AddRunway(It.IsAny<Runway>())).Returns(1);

        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        service.SaveRunway(0, "R1", "20");

        mockRunwayRepo.Verify(r => r.AddRunway(It.IsAny<Runway>()), Times.Once);
        mockRunwayRepo.Verify(r => r.UpdateRunway(It.IsAny<Runway>()), Times.Never);
    }

    [Fact]
    public void SaveRunway_Should_Call_Update_When_Id_Is_NonZero()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        mockRunwayRepo.Setup(r => r.GetRunwayById(2)).Returns(new Runway { Name = "Old", HandleTime = 5 });

        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        service.SaveRunway(2, "Updated", "20");

        mockRunwayRepo.Verify(r => r.UpdateRunway(It.IsAny<Runway>()), Times.Once);
        mockRunwayRepo.Verify(r => r.AddRunway(It.IsAny<Runway>()), Times.Never);
    }

    [Fact]
    public void HasFlights_Should_Return_True_When_Flights_Exist()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        mockFlightRepo.Setup(r => r.GetFlightsByRunway(1)).Returns(new List<Flight> { new Flight() });

        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.True(service.HasFlights(1));
    }

    [Fact]
    public void HasFlights_Should_Return_False_When_No_Flights_Exist()
    {
        var mockRunwayRepo = new Mock<IRunwayRepo>();
        var mockFlightRepo = new Mock<IFlightRepo>();
        mockFlightRepo.Setup(r => r.GetFlightsByRunway(1)).Returns(new List<Flight>());

        var service = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.False(service.HasFlights(1));
    }
}
