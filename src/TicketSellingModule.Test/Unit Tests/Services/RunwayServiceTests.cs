using System;
using System.Collections.Generic;

using Moq;

using TicketSellingModule.Data.Repositories.Interfaces;

using Xunit;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class RunwayServiceTests
{
    private const int DefaultId = 1;
    private const int ValidHandleTime = 10;
    private const int UpdatedHandleTime = 30;
    private const string DefaultRunwayName = "R1";
    private const string UpdatedRunwayName = "UpdatedName";
    private const int CreatedId = 7;

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

        mockRunwayRepo.Setup(repo => repo.GetAllRunways()).Returns(runways);

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        var result = runwayService.GetAllRunways();

        Assert.Equal(runways.Count, result.Count);
        Assert.Equal(runways, result);
    }

    [Fact]
    public void GetById_Should_Be_Null_For_Invalid_Id()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        const int invalidId = -1;
        Assert.Null(runwayService.GetRunwayById(invalidId));
    }

    [Fact]
    public void GetById_Should_Be_Null_When_Runway_Not_Found()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        const int nonExistentId = 999;

        mockRunwayRepo.Setup(repo => repo.GetRunwayById(nonExistentId)).Returns((Runway)null);

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Null(runwayService.GetRunwayById(nonExistentId));
    }

    [Fact]
    public void GetById_Should_Return_Runway_When_Runway_Exists()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var runway = new Runway { Name = DefaultRunwayName, HandleTime = ValidHandleTime };

        mockRunwayRepo.Setup(repo => repo.GetRunwayById(DefaultId)).Returns(runway);

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        var result = runwayService.GetRunwayById(DefaultId);

        Assert.Equal(runway, result);
    }

    [Fact]
    public void GetById_Should_Throw_When_Repo_Throws()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockRunwayRepo.Setup(repo => repo.GetRunwayById(It.IsAny<int>())).Throws<Exception>();

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<Exception>(() => runwayService.GetRunwayById(DefaultId));
    }

    [Fact]
    public void Add_Should_Throw_For_Null_Name()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => runwayService.AddRunway(null, ValidHandleTime));
    }

    [Fact]
    public void Add_Should_Throw_For_Empty_Name()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => runwayService.AddRunway(string.Empty, ValidHandleTime));
    }

    [Fact]
    public void Add_Should_Throw_For_Invalid_HandleTime()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        const int negativeHandleTime = -5;
        Assert.Throws<ArgumentException>(() => runwayService.AddRunway(DefaultRunwayName, negativeHandleTime));
    }

    [Fact]
    public void Add_Should_Work_When_Valid_Input()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();

        mockRunwayRepo.Setup(repo => repo.AddRunway(It.IsAny<Runway>())).Returns(CreatedId);

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        var result = runwayService.AddRunway(DefaultRunwayName, ValidHandleTime);

        Assert.Equal(CreatedId, result);
        mockRunwayRepo.Verify(r => r.AddRunway(It.Is<Runway>(runway =>
            runway.Name == DefaultRunwayName &&
            runway.HandleTime == ValidHandleTime)), Times.Once);
    }

    [Fact]
    public void Update_Should_Throw_When_Runway_Not_Found()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockRunwayRepo.Setup(repo => repo.GetRunwayById(DefaultId)).Returns((Runway)null);

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.Throws<InvalidOperationException>(() => runwayService.UpdateRunway(DefaultId, UpdatedRunwayName));
    }

    [Fact]
    public void Update_Should_Update_All_Fields_When_Both_Are_Provided()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var runway = new Runway { Name = "OldName", HandleTime = 5 };

        mockRunwayRepo.Setup(repo => repo.GetRunwayById(DefaultId)).Returns(runway);

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        runwayService.UpdateRunway(DefaultId, UpdatedRunwayName, UpdatedHandleTime);

        Assert.Equal(UpdatedRunwayName, runway.Name);
        Assert.Equal(UpdatedHandleTime, runway.HandleTime);
        mockRunwayRepo.Verify(r => r.UpdateRunway(runway), Times.Once);
    }

    [Fact]
    public void Delete_Should_Call_Repo_For_Valid_Id()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockRunwayRepo.Setup(repo => repo.GetRunwayById(DefaultId)).Returns(new Runway { Name = DefaultRunwayName });

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        runwayService.DeleteRunwayUsingId(DefaultId);

        mockRunwayRepo.Verify(repo => repo.DeleteRunwayUsingId(DefaultId), Times.Once);
    }

    [Fact]
    public void SaveRunway_Should_Throw_For_Invalid_HandleTime_Text()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        const string nonNumericValue = "abc";
        Assert.Throws<ArgumentException>(() => runwayService.SaveRunway(0, DefaultRunwayName, nonNumericValue));
    }

    [Fact]
    public void SaveRunway_Should_Call_Add_When_Id_Is_Zero()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        const int newRunwayId = 0;
        string handleTimeStr = ValidHandleTime.ToString();

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);
        runwayService.SaveRunway(newRunwayId, DefaultRunwayName, handleTimeStr);

        mockRunwayRepo.Verify(repo => repo.AddRunway(It.IsAny<Runway>()), Times.Once);
    }

    [Fact]
    public void HasFlights_Should_Return_True_When_Flights_Exist()
    {
        var mockRunwayRepo = new Mock<IRunwayRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();

        mockFlightRepo.Setup(repo => repo.GetFlightsByRunwayId(DefaultId))
                      .Returns(new List<Flight> { new Flight() });

        var runwayService = new RunwayService(mockRunwayRepo.Object, mockFlightRepo.Object);

        Assert.True(runwayService.HasFlights(DefaultId));
    }
}