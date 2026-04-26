using System;
using System.Collections.Generic;
using System.Text;

using Moq;

using TicketSellingModule.Data.Repositories.Interfaces;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class GateServiceTests
{
    private string firstGateName = "A1";
    private string secondGateName = "B2";
    private int validGateId = 1;
    private int invalidGateId = 99;
    private int negativeGateId = -1;
    private int numberOfGates = 2;
    private int inexistentGateId = 0;

    [Fact]
    public void GetAll_Should_Return_All_Gates_Always()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gates = new List<Gate>
        {
            new Gate { Name = firstGateName },
            new Gate { Name = secondGateName }
        };

        mockGateRepo.Setup(getAllGates => getAllGates.GetAllGates()).Returns(gates);

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        var result = gateService.GetAllGates();

        Assert.Equal(numberOfGates, result.Count);
        Assert.Equal(gates, result);
    }

    [Fact]
    public void GetById_Should_Return_Null_For_Invalid_Id()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.Null(gateService.GetGateById(invalidGateId));
    }

    [Fact]
    public void GetById_Should_Return_Gate_When_Found()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gate = new Gate { Name = firstGateName };
        mockGateRepo.Setup(getGate => getGate.GetGateById(validGateId)).Returns(gate);

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        var result = gateService.GetGateById(validGateId);
        Assert.Equal(gate, result);
    }

    [Fact]
    public void Add_Should_Throw_For_Null_Name()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => gateService.AddGate(null));
    }

    [Fact]
    public void Add_Should_Throw_For_Empty_Name()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => gateService.AddGate(string.Empty));
    }

    [Fact]
    public void Add_Should_Throw_For_Whitespace_Name()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => gateService.AddGate(" "));
    }

    [Fact]
    public void Add_Should_Work_For_Valid_Data()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockGateRepo.Setup(addGate => addGate.AddGate(It.IsAny<Gate>())).Returns(validGateId);

        var service = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        var result = service.AddGate(firstGateName);

        Assert.Equal(validGateId, result);
        mockGateRepo.Verify(addGate => addGate.AddGate(It.IsAny<Gate>()), Times.Once);
    }

    [Fact]
    public void Update_Should_Do_Nothing_If_Gate_Not_Found()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockGateRepo.Setup(getGate => getGate.GetGateById(validGateId)).Returns((Gate)null);

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        gateService.UpdateGate(validGateId, secondGateName);

        mockGateRepo.Verify(updateGate => updateGate.UpdateGate(It.IsAny<Gate>()), Times.Never);
    }

    [Fact]
    public void Update_Should_Throw_For_Whitespace_New_Name()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockGateRepo.Setup(getGate => getGate.GetGateById(validGateId)).Returns(new Gate { Name = firstGateName });

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => gateService.UpdateGate(validGateId, " "));
    }

    [Fact]
    public void Update_Should_Throw_For_Empty_New_Name()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockGateRepo.Setup(getGate => getGate.GetGateById(1)).Returns(new Gate { Name = firstGateName });

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.Throws<ArgumentException>(() => gateService.UpdateGate(1, string.Empty));
    }

    [Fact]
    public void Update_Should_Update_Name_For_Valid_Data()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gate = new Gate { Name = firstGateName };
        mockGateRepo.Setup(getGate => getGate.GetGateById(validGateId)).Returns(gate);

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        gateService.UpdateGate(validGateId, secondGateName);
        Assert.Equal(secondGateName, gate.Name);
        mockGateRepo.Verify(updateGate => updateGate.UpdateGate(gate), Times.Once);
    }

    [Fact]
    public void Update_Should_Call_Repo_Even_When_No_Changes()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gate = new Gate { Name = firstGateName };
        mockGateRepo.Setup(getGate => getGate.GetGateById(validGateId)).Returns(gate);

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        gateService.UpdateGate(validGateId);
        Assert.Equal(firstGateName, gate.Name);
        mockGateRepo.Verify(updateGate => updateGate.UpdateGate(gate), Times.Once);
    }

    [Fact]
    public void Delete_Should_Not_Call_Repo_For_Invalid_Id()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        gateService.DeleteGateUsingId(negativeGateId);

        mockGateRepo.Verify(deleteGate => deleteGate.DeleteGateUsingId(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void Delete_Should_Call_Repo_For_Valid_Id()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        gateService.DeleteGateUsingId(validGateId);

        mockGateRepo.Verify(deleteGate => deleteGate.DeleteGateUsingId(validGateId), Times.Once);
    }

    [Fact]
    public void SaveGate_Should_Call_Add_When_Id_Is_Zero()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockGateRepo.Setup(addGate => addGate.AddGate(It.IsAny<Gate>())).Returns(validGateId);

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        gateService.SaveGate(inexistentGateId, secondGateName);

        mockGateRepo.Verify(addGate => addGate.AddGate(It.Is<Gate>(newGate => newGate.Name == secondGateName)), Times.Once);
        mockGateRepo.Verify(updateGate => updateGate.UpdateGate(It.IsAny<Gate>()), Times.Never);
    }

    [Fact]
    public void SaveGate_Should_Call_Update_When_Id_Is_NonZero()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var gate = new Gate { Name = firstGateName };
        mockGateRepo.Setup(getGate => getGate.GetGateById(validGateId)).Returns(gate);

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);
        gateService.SaveGate(validGateId, secondGateName);
        mockGateRepo.Verify(updateGate => updateGate.UpdateGate(It.Is<Gate>(newGate => newGate.Name == secondGateName)), Times.Once);
        mockGateRepo.Verify(addGate => addGate.AddGate(It.IsAny<Gate>()), Times.Never);
    }

    [Fact]
    public void HasFlights_Should_Return_True_When_Flights_Exist()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockFlightRepo.Setup(getFlights => getFlights.GetFlightsByGateId(validGateId)).Returns(new List<Flight> { new Flight() });

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.True(gateService.HasFlights(validGateId));
    }

    [Fact]
    public void HasFlights_Should_Return_False_When_No_Flights_Exist()
    {
        var mockGateRepo = new Mock<IGateRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockFlightRepo.Setup(getFlights => getFlights.GetFlightsByGateId(validGateId)).Returns(new List<Flight>());

        var gateService = new GateService(mockGateRepo.Object, mockFlightRepo.Object);

        Assert.False(gateService.HasFlights(validGateId));
    }
}
