using System;
using System.Collections.Generic;
using System.Text;

using Moq;

using TicketSellingModule.Data.Repositories.Interfaces;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class GateServiceTests
{
    [Fact]
    public void GetAll_Should_Return_All_Gates()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gates = new List<Gate>
        {
            new Gate { Name = "A1" },
            new Gate { Name = "B2" }
        };
        mockGateRepo.Setup(r => r.GetAllGates()).Returns(gates);

        var service = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        var result = service.GetAll();

        Assert.Equal(2, result.Count);
        Assert.Equal(gates, result);
    }

    [Fact]
    public void GetById_Should_Return_Null_For_Invalid_Id()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var service = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.Null(service.GetById(0));
        Assert.Null(service.GetById(-1));
    }

    [Fact]
    public void GetById_Should_Return_Gate()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gate = new Gate { Name = "A1" };
        mockGateRepo.Setup(r => r.GetGateById(1)).Returns(gate);

        var service = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        var result = service.GetById(1);

        Assert.Equal(gate, result);
    }

    [Fact]
    public void Add_Should_Throw_For_Null_Or_Empty_Name()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var service = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => service.Add(null));
        Assert.Throws<ArgumentException>(() => service.Add(string.Empty));
        Assert.Throws<ArgumentException>(() => service.Add(" "));
    }

    [Fact]
    public void Add_Should_Work()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockGateRepo.Setup(r => r.AddGate(It.IsAny<Gate>())).Returns(3);

        var service = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        var result = service.Add("Gate A");

        Assert.Equal(3, result);
        mockGateRepo.Verify(r => r.AddGate(It.IsAny<Gate>()), Times.Once);
    }

    [Fact]
    public void Update_Should_Do_Nothing_If_Gate_Not_Found()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockGateRepo.Setup(r => r.GetGateById(1)).Returns((Gate)null);

        var service = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        service.Update(1, "NewName");

        mockGateRepo.Verify(r => r.UpdateGate(It.IsAny<Gate>()), Times.Never);
    }

    [Fact]
    public void Update_Should_Throw_For_Whitespace_New_Name()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockGateRepo.Setup(r => r.GetGateById(1)).Returns(new Gate { Name = "Old" });

        var service = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => service.Update(1, " "));
        Assert.Throws<ArgumentException>(() => service.Update(1, string.Empty));
    }

    [Fact]
    public void Update_Should_Update_Name()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gate = new Gate { Name = "OldName" };
        mockGateRepo.Setup(r => r.GetGateById(1)).Returns(gate);

        var service = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        service.Update(1, "NewName");

        Assert.Equal("NewName", gate.Name);
        mockGateRepo.Verify(r => r.UpdateGate(gate), Times.Once);
    }

    [Fact]
    public void Update_Should_Call_Repo_Even_When_No_Changes()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gate = new Gate { Name = "OldName" };
        mockGateRepo.Setup(r => r.GetGateById(1)).Returns(gate);

        var service = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        service.Update(1);

        Assert.Equal("OldName", gate.Name);
        mockGateRepo.Verify(r => r.UpdateGate(gate), Times.Once);
    }

    [Fact]
    public void Delete_Should_Not_Call_Repo_For_Invalid_Id()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var service = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        service.Delete(0);
        service.Delete(-3);

        mockGateRepo.Verify(r => r.DeleteGateUsingId(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void Delete_Should_Call_Repo_For_Valid_Id()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var service = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        service.Delete(4);

        mockGateRepo.Verify(r => r.DeleteGateUsingId(4), Times.Once);
    }

    [Fact]
    public void SaveGate_Should_Call_Add_When_Id_Is_Zero()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockGateRepo.Setup(r => r.AddGate(It.IsAny<Gate>())).Returns(1);

        var service = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        service.SaveGate(0, "NewGate");

        mockGateRepo.Verify(r => r.AddGate(It.Is<Gate>(g => g.Name == "NewGate")), Times.Once);
        mockGateRepo.Verify(r => r.UpdateGate(It.IsAny<Gate>()), Times.Never);
    }

    [Fact]
    public void SaveGate_Should_Call_Update_When_Id_Is_NonZero()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gate = new Gate { Name = "OldName" };
        mockGateRepo.Setup(r => r.GetGateById(2)).Returns(gate);

        var service = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        service.SaveGate(2, "UpdatedGate");

        mockGateRepo.Verify(r => r.UpdateGate(It.Is<Gate>(g => g.Name == "UpdatedGate")), Times.Once);
        mockGateRepo.Verify(r => r.AddGate(It.IsAny<Gate>()), Times.Never);
    }

    [Fact]
    public void HasFlights_Should_Return_True_When_Flights_Exist()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockFlightRepo.Setup(r => r.GetFlightsByGateId(1)).Returns(new List<Flight> { new Flight() });

        var service = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.True(service.HasFlights(1));
    }

    [Fact]
    public void HasFlights_Should_Return_False_When_No_Flights_Exist()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockFlightRepo.Setup(r => r.GetFlightsByGateId(1)).Returns(new List<Flight>());

        var service = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.False(service.HasFlights(1));
    }
}
