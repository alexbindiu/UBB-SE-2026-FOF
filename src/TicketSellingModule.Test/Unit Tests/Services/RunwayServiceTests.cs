using System;
using System.Collections.Generic;
using System.Text;

using Moq;

using TicketSellingModule.Data.Repositories.Interfaces;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class RunwayServiceTests
{
    [Fact]
    public void GetAll_Should_Return_All_Runways_Always()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var runways = new List<Runway>
        {
            new Runway { Name = "R1", HandleTime = 10 },
            new Runway { Name = "R2", HandleTime = 15 }
        };
        mockRunwayRepo.Setup(getAll => getAll.GetAllRunways()).Returns(runways);

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        var result = runwayService.GetAllRunways();

        Assert.Equal(2, result.Count);
        Assert.Equal(runways, result);
    }

    [Fact]
    public void GetById_Should_Be_Null_For_Invalid_Id()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Null(runwayService.GetRunwayById(-1));
    }

    [Fact]
    public void GetById_Should_Be_Null_When_Runway_Not_Found()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockRunwayRepo.Setup(getNoRunway => getNoRunway.GetRunwayById(5)).Returns((Runway)null);

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Null(runwayService.GetRunwayById(5));
    }

    [Fact]
    public void GetById_Should_Return_Runway_When_Runway_Exists()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var runway = new Runway { Name = "R1", HandleTime = 10 };
        mockRunwayRepo.Setup(getExistingRunway => getExistingRunway.GetRunwayById(1)).Returns(runway);

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        var result = runwayService.GetRunwayById(1);

        Assert.Equal(runway, result);
    }

    [Fact]
    public void GetById_Should_Throw_When_Repo_Throws()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockRunwayRepo.Setup(throwException => throwException.GetRunwayById(It.IsAny<int>())).Throws<Exception>();

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<Exception>(() => runwayService.GetRunwayById(1));
    }

    [Fact]
    public void Add_Should_Throw_For_Null_Name()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => runwayService.AddRunway(null, 10));
    }

    [Fact]
    public void Add_Should_Throw_For_Empty_Name()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => runwayService.AddRunway(string.Empty, 10));
    }

    [Fact]
    public void Add_Should_Throw_For_Whitespace_Name()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => runwayService.AddRunway(" ", 10));
    }

    [Fact]
    public void Add_Should_Throw_For_Invalid_HandleTime()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => runwayService.AddRunway("R1", -5));
    }

    [Fact]
    public void Add_Should_Work_When_Valid_Input()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockRunwayRepo.Setup(getCorrectRunway => getCorrectRunway.AddRunway(It.IsAny<Runway>())).Returns(7);

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        var result = runwayService.AddRunway("R1", 10);

        Assert.Equal(7, result);
        mockRunwayRepo.Verify(r => r.AddRunway(It.IsAny<Runway>()), Times.Once);
    }

    [Fact]
    public void Update_Should_Throw_When_Runway_Not_Found()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockRunwayRepo.Setup(getNoRunway => getNoRunway.GetRunwayById(1)).Returns((Runway)null);

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<InvalidOperationException>(() => runwayService.UpdateRunway(1, "NewName"));
    }

    [Fact]
    public void Update_Should_Throw_For_Whitespace_New_Name()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockRunwayRepo.Setup(getNoRunway => getNoRunway.GetRunwayById(1)).Returns(new Runway { Name = "Old", HandleTime = 10 });

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => runwayService.UpdateRunway(1, " "));
    }

    [Fact]
    public void Update_Should_Throw_For_Empty_New_Name()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockRunwayRepo.Setup(getNoRunway => getNoRunway.GetRunwayById(1)).Returns(new Runway { Name = "Old", HandleTime = 10 });

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => runwayService.UpdateRunway(1, string.Empty));
    }

    [Fact]
    public void Update_Should_Throw_For_Invalid_HandleTime()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockRunwayRepo.Setup(getNoRunway => getNoRunway.GetRunwayById(1)).Returns(new Runway { Name = "Old", HandleTime = 10 });

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => runwayService.UpdateRunway(1, newHandleTime: -1));
    }

    [Fact]
    public void Update_Should_Update_Only_Name_When_New_HandleTime_Is_Not_Provided()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var runway = new Runway { Name = "OldName", HandleTime = 10 };
        mockRunwayRepo.Setup(getCorrectRunway => getCorrectRunway.GetRunwayById(1)).Returns(runway);

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        runwayService.UpdateRunway(1, newName: "NewName");

        Assert.Equal("NewName", runway.Name);
        Assert.Equal(10, runway.HandleTime);
        mockRunwayRepo.Verify(r => r.UpdateRunway(runway), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_Only_HandleTime_When_New_Name_Is_Not_Provided()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var runway = new Runway { Name = "OldName", HandleTime = 10 };
        mockRunwayRepo.Setup(getCorrectRunway => getCorrectRunway.GetRunwayById(1)).Returns(runway);

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        runwayService.UpdateRunway(1, newHandleTime: 25);

        Assert.Equal("OldName", runway.Name);
        Assert.Equal(25, runway.HandleTime);
        mockRunwayRepo.Verify(r => r.UpdateRunway(runway), Times.Once);
    }

    [Fact]
    public void Update_Should_Update_All_Fields_When_Both_Are_Provided()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var runway = new Runway { Name = "OldName", HandleTime = 10 };
        mockRunwayRepo.Setup(getCorrectRunway => getCorrectRunway.GetRunwayById(1)).Returns(runway);

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        runwayService.UpdateRunway(1, "NewName", 30);

        Assert.Equal("NewName", runway.Name);
        Assert.Equal(30, runway.HandleTime);
        mockRunwayRepo.Verify(r => r.UpdateRunway(runway), Times.Once);
    }

    [Fact]
    public void Delete_Should_Throw_When_Runway_Not_Found()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockRunwayRepo.Setup(getNoRunway => getNoRunway.GetRunwayById(1)).Returns((Runway)null);

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<InvalidOperationException>(() => runwayService.DeleteRunwayUsingId(1));
    }

    [Fact]
    public void Delete_Should_Call_Repo_For_Valid_Id()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockRunwayRepo.Setup(getCorrectRunway => getCorrectRunway.GetRunwayById(3)).Returns(new Runway { Name = "R3" });

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        runwayService.DeleteRunwayUsingId(3);

        mockRunwayRepo.Verify(deleteCorrectRunway => deleteCorrectRunway.DeleteRunwayUsingId(3), Times.Once);
    }

    [Fact]
    public void SaveRunway_Should_Throw_For_Invalid_HandleTime_Text()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => runwayService.SaveRunway(0, "R1", "abc"));
    }

    [Fact]
    public void SaveRunway_Should_Call_Add_When_Id_Is_Zero()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockRunwayRepo.Setup(addCorrectRunway => addCorrectRunway.AddRunway(It.IsAny<Runway>())).Returns(1);

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        runwayService.SaveRunway(0, "R1", "20");

        mockRunwayRepo.Verify(addCorrectRunway => addCorrectRunway.AddRunway(It.IsAny<Runway>()), Times.Once);
        mockRunwayRepo.Verify(updateCorrectRunway => updateCorrectRunway.UpdateRunway(It.IsAny<Runway>()), Times.Never);
    }

    [Fact]
    public void SaveRunway_Should_Call_Update_When_Id_Is_NonZero()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockRunwayRepo.Setup(getCorrectRunway => getCorrectRunway.GetRunwayById(2)).Returns(new Runway { Name = "Old", HandleTime = 5 });

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        runwayService.SaveRunway(2, "Updated", "20");

        mockRunwayRepo.Verify(updateCorrectRunway => updateCorrectRunway.UpdateRunway(It.IsAny<Runway>()), Times.Once);
        mockRunwayRepo.Verify(addCorrectRunway => addCorrectRunway.AddRunway(It.IsAny<Runway>()), Times.Never);
    }

    [Fact]
    public void HasFlights_Should_Return_True_When_Flights_Exist()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockFlightRepo.Setup(getFlights => getFlights.GetFlightsByRunwayId(1)).Returns(new List<Flight> { new Flight() });

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.True(runwayService.HasFlights(1));
    }

    [Fact]
    public void HasFlights_Should_Return_False_When_No_Flights_Exist()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockFlightRepo.Setup(getNoFlights => getNoFlights.GetFlightsByRunwayId(1)).Returns(new List<Flight>());

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.False(runwayService.HasFlights(1));
    }
}
