using System;
using System.Collections.Generic;
using System.Text;

using Moq;

using TicketSellingModule.Data.Repositories.Interfaces;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class GateServiceTests
{
    [Fact]
    public void GetAll_Should_Return_All_Gates_Always()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gates = new List<Gate>
        {
            new Gate { Name = "A1" },
            new Gate { Name = "B2" }
        };

        mockGateRepo.Setup(getAllGates => getAllGates.GetAllGates()).Returns(gates);

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        var result = gateService.GetAllGates();

        Assert.Equal(2, result.Count);
        Assert.Equal(gates, result);
    }

    [Fact]
    public void GetById_Should_Return_Null_For_Invalid_Id()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.Null(gateService.GetGateById(-1));
    }

    [Fact]
    public void GetById_Should_Return_Gate_When_Found()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gate = new Gate { Name = "A1" };
        mockGateRepo.Setup(getGate => getGate.GetGateById(1)).Returns(gate);

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        var result = gateService.GetGateById(1);

        Assert.Equal(gate, result);
    }

    [Fact]
    public void Add_Should_Throw_For_Null_Name()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => gateService.Add(null));
    }

    [Fact]
    public void Add_Should_Throw_For_Empty_Name()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => gateService.Add(string.Empty));
    }

    [Fact]
    public void Add_Should_Throw_For_Whitespace_Name()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => gateService.Add(" "));
    }

    [Fact]
    public void Add_Should_Work_For_Valid_Data()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockGateRepo.Setup(addGate => addGate.AddGate(It.IsAny<Gate>())).Returns(3);

        var service = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        var result = service.Add("Gate A");

        Assert.Equal(3, result);
        mockGateRepo.Verify(addGate => addGate.AddGate(It.IsAny<Gate>()), Times.Once);
    }

    [Fact]
    public void Update_Should_Do_Nothing_If_Gate_Not_Found()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockGateRepo.Setup(getGate => getGate.GetGateById(1)).Returns((Gate)null);

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        gateService.Update(1, "NewName");

        mockGateRepo.Verify(updateGate => updateGate.UpdateGate(It.IsAny<Gate>()), Times.Never);
    }

    [Fact]
    public void Update_Should_Throw_For_Whitespace_New_Name()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockGateRepo.Setup(getGate => getGate.GetGateById(1)).Returns(new Gate { Name = "Old" });

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => gateService.Update(1, " "));
    }

    [Fact]
    public void Update_Should_Throw_For_Empty_New_Name()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockGateRepo.Setup(getGate => getGate.GetGateById(1)).Returns(new Gate { Name = "Old" });

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => gateService.Update(1, string.Empty));
    }

    [Fact]
    public void Update_Should_Update_Name_For_Valid_Data()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gate = new Gate { Name = "OldName" };
        mockGateRepo.Setup(getGate => getGate.GetGateById(1)).Returns(gate);

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        gateService.Update(1, "NewName");

        Assert.Equal("NewName", gate.Name);
        mockGateRepo.Verify(updateGate => updateGate.UpdateGate(gate), Times.Once);
    }

    [Fact]
    public void Update_Should_Call_Repo_Even_When_No_Changes()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gate = new Gate { Name = "OldName" };
        mockGateRepo.Setup(getGate => getGate.GetGateById(1)).Returns(gate);

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        gateService.Update(1);

        Assert.Equal("OldName", gate.Name);
        mockGateRepo.Verify(updateGate => updateGate.UpdateGate(gate), Times.Once);
    }

    [Fact]
    public void Delete_Should_Not_Call_Repo_For_Invalid_Id()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        gateService.DeleteGateUsingId(0);
        gateService.DeleteGateUsingId(-3);

        mockGateRepo.Verify(deleteGate => deleteGate.DeleteGateUsingId(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void Delete_Should_Call_Repo_For_Valid_Id()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        gateService.DeleteGateUsingId(4);

        mockGateRepo.Verify(deleteGate => deleteGate.DeleteGateUsingId(4), Times.Once);
    }

    [Fact]
    public void SaveGate_Should_Call_Add_When_Id_Is_Zero()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockGateRepo.Setup(addGate => addGate.AddGate(It.IsAny<Gate>())).Returns(1);

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        gateService.SaveGate(0, "NewGate");

        mockGateRepo.Verify(addGate => addGate.AddGate(It.Is<Gate>(g => g.Name == "NewGate")), Times.Once);
        mockGateRepo.Verify(updateGate => updateGate.UpdateGate(It.IsAny<Gate>()), Times.Never);
    }

    [Fact]
    public void SaveGate_Should_Call_Update_When_Id_Is_NonZero()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gate = new Gate { Name = "OldName" };
        mockGateRepo.Setup(getGate => getGate.GetGateById(2)).Returns(gate);

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        gateService.SaveGate(2, "UpdatedGate");

        mockGateRepo.Verify(updateGate => updateGate.UpdateGate(It.Is<Gate>(g => g.Name == "UpdatedGate")), Times.Once);
        mockGateRepo.Verify(addGate => addGate.AddGate(It.IsAny<Gate>()), Times.Never);
    }

    [Fact]
    public void HasFlights_Should_Return_True_When_Flights_Exist()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockFlightRepo.Setup(getFlights => getFlights.GetFlightsByGateId(1)).Returns(new List<Flight> { new Flight() });

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.True(gateService.HasFlights(1));
    }

    [Fact]
    public void HasFlights_Should_Return_False_When_No_Flights_Exist()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockFlightRepo.Setup(getFlights => getFlights.GetFlightsByGateId(1)).Returns(new List<Flight>());

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.False(gateService.HasFlights(1));
    }
}
