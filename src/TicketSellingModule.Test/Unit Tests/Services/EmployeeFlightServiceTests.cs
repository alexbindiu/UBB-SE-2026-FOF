using System;
using System.Collections.Generic;
using System.Text;

using Moq;

using TicketSellingModule.Data.Repositories.Interfaces;
using TicketSellingModule.Data.Services.Interfaces;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class EmployeeFlightServiceTests
{
    [Fact]
    public void Constructor_Should_Throw_When_Dependencies_Are_Null()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        Assert.Throws<ArgumentNullException>(() =>
            new EmployeeFlightService(null, mockEmployeeRepo.Object, mockFlightRepo.Object, mockRouteRepo.Object, mockGateService.Object, mockRunwayService.Object, mockRouteService.Object));
    }

    [Fact]
    public void AssignCrewMember_Should_Throw_For_Invalid_Id()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        Assert.Throws<ArgumentException>(() => service.AssignCrewMember(0, 1));
        Assert.Throws<ArgumentException>(() => service.AssignCrewMember(1, 0));
    }

    [Fact]
    public void AssignCrewMember_Should_Throw_When_Employee_Or_Flight_Missing()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        mockEmployeeRepo.Setup(r => r.GetEmployeeById(1)).Returns((Employee)null);
        mockFlightRepo.Setup(r => r.GetById(1)).Returns(new Flight());

        Assert.Throws<InvalidOperationException>(() => service.AssignCrewMember(1, 1));
    }

    [Fact]
    public void AssignCrewMember_Should_Throw_When_Already_Assigned()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        mockEmployeeRepo.Setup(r => r.GetEmployeeById(1)).Returns(new Employee());
        mockFlightRepo.Setup(r => r.GetById(1)).Returns(new Flight { Id = 1, Route = new Route { Id = 1 }, Date = DateTime.Today });

        mockEmployeeFlightRepo.Setup(r => r.GetEmployeesByFlightId(1)).Returns(new List<int> { 1 });

        Assert.Throws<InvalidOperationException>(() => service.AssignCrewMember(1, 1));
    }

    [Fact]
    public void AssignCrewMember_Should_Throw_When_Not_Available()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        var flight = new Flight { Id = 1, Date = DateTime.Today, Route = new Route { Id = 1 } };

        mockEmployeeRepo.Setup(r => r.GetEmployeeById(1)).Returns(new Employee { Name = "Test" });
        mockFlightRepo.Setup(r => r.GetById(1)).Returns(flight);
        mockEmployeeFlightRepo.Setup(r => r.GetEmployeesByFlightId(1)).Returns(new List<int>());

        mockRouteRepo.Setup(r => r.GetRouteById(It.IsAny<int>())).Returns((Route)null);

        Assert.Throws<InvalidOperationException>(() => service.AssignCrewMember(1, 1));
    }

    [Fact]
    public void AssignCrewMember_Should_Work()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        var flight = new Flight { Id = 1, Date = DateTime.Today, Route = new Route { Id = 1 } };

        mockEmployeeRepo.Setup(r => r.GetEmployeeById(1)).Returns(new Employee());
        mockFlightRepo.Setup(r => r.GetById(1)).Returns(flight);
        mockEmployeeFlightRepo.Setup(r => r.GetEmployeesByFlightId(1)).Returns(new List<int>());
        mockRouteRepo.Setup(r => r.GetRouteById(It.IsAny<int>())).Returns(new Route
        {
            DepartureTime = TimeOnly.FromDateTime(DateTime.Today),
            ArrivalTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(1))
        });

        mockEmployeeFlightRepo.Setup(r => r.GetFlightsByEmployeeId(1)).Returns(new List<int>());

        service.AssignCrewMember(1, 1);

        mockEmployeeFlightRepo.Verify(r => r.AssignFlightToEmployeesUsingIds(1, 1), Times.Once);
    }

    [Fact]
    public void CleanUpFlightAssignments_Should_Call_When_Valid()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        service.CleanUpFlightAssignments(1);

        mockEmployeeFlightRepo.Verify(r => r.RemoveAllByFlightId(1), Times.Once);
    }

    [Fact]
    public void CleanUpEmployeeAssignments_Should_Not_Call_For_Invalid_Id()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        service.CleanUpEmployeeAssignments(0);

        mockEmployeeFlightRepo.Verify(r => r.RemoveAllByEmployeeId(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void GetFlightCrew_Should_Return_Only_Existing_Employees()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        mockEmployeeFlightRepo.Setup(r => r.GetEmployeesByFlightId(1)).Returns(new List<int> { 1, 2 });

        mockEmployeeRepo.Setup(r => r.GetEmployeeById(1)).Returns(new Employee { Id = 1 });
        mockEmployeeRepo.Setup(r => r.GetEmployeeById(2)).Returns((Employee)null);

        var result = service.GetFlightCrew(1);

        Assert.Single(result);
    }

    [Fact]
    public void GetEmployeeSchedule_Should_Return_Empty_For_Invalid_Id()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        var result = service.GetEmployeeSchedule(0);

        Assert.Empty(result);
    }

    [Fact]
    public void IsEmployeeAvailable_Should_Return_False_When_Route_Not_Found()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        mockRouteRepo.Setup(r => r.GetRouteById(1)).Returns((Route)null);

        var result = service.IsEmployeeAvailable(1, DateTime.Today, 1);

        Assert.False(result);
    }

    [Fact]
    public void IsEmployeeAvailable_Should_Return_False_When_Overlap()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        var targetRoute = new Route
        {
            Id = 1,
            DepartureTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(10)),
            ArrivalTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(12))
        };

        var existingRoute = new Route
        {
            Id = 2,
            DepartureTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(11)),
            ArrivalTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(13))
        };

        mockRouteRepo.Setup(r => r.GetRouteById(1)).Returns(targetRoute);
        mockRouteRepo.Setup(r => r.GetRouteById(2)).Returns(existingRoute);

        mockEmployeeFlightRepo.Setup(r => r.GetFlightsByEmployeeId(1)).Returns(new List<int> { 2 });
        mockFlightRepo.Setup(r => r.GetById(2)).Returns(new Flight
        {
            Id = 2,
            Date = DateTime.Today,
            Route = new Route { Id = 2 }
        });

        var result = service.IsEmployeeAvailable(1, DateTime.Today, 1);

        Assert.False(result);
    }

    [Fact]
    public void IsEmployeeAvailable_Should_Return_True_When_No_Overlap()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        var route = new Route
        {
            Id = 1,
            DepartureTime = TimeOnly.FromDateTime(DateTime.Today),
            ArrivalTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(1))
        };

        mockRouteRepo.Setup(r => r.GetRouteById(It.IsAny<int>())).Returns(route);
        mockEmployeeFlightRepo.Setup(r => r.GetFlightsByEmployeeId(1)).Returns(new List<int>());

        var result = service.IsEmployeeAvailable(1, DateTime.Today, 1);

        Assert.True(result);
    }

    [Fact]
    public void AssignCrewToFlight_Should_Continue_On_Exception()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        mockEmployeeRepo.Setup(r => r.GetEmployeeById(1)).Throws(new Exception());

        service.AssignCrewToFlight(1, new List<int> { 1, 2 });

        Assert.True(true);
    }

    [Fact]
    public void UpdateCrewForFlight_Should_Add_And_Remove_Correctly()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        mockEmployeeFlightRepo.Setup(r => r.GetEmployeesByFlightId(1))
            .Returns(new List<int> { 1, 2 });

        mockEmployeeRepo.Setup(r => r.GetEmployeeById(It.IsAny<int>())).Returns(new Employee());
        mockFlightRepo.Setup(r => r.GetById(It.IsAny<int>())).Returns(new Flight { Id = 1, Route = new Route { Id = 1 }, Date = DateTime.Today });
        mockRouteRepo.Setup(r => r.GetRouteById(It.IsAny<int>())).Returns(new Route());
        mockEmployeeFlightRepo.Setup(r => r.GetFlightsByEmployeeId(It.IsAny<int>())).Returns(new List<int>());

        service.UpdateCrewForFlight(1, new List<int> { 2, 3 });

        mockEmployeeFlightRepo.Verify(r => r.RemoveFlightFromEmployeeUsingIds(1, 1), Times.Once);
    }

    [Fact]
    public void RemoveCrewMember_Should_Call_Repo()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        service.RemoveCrewMember(10, 5);

        mockEmployeeFlightRepo.Verify(r => r.RemoveFlightFromEmployeeUsingIds(5, 10), Times.Once);
    }

    [Fact]
    public void GetFormattedEmployeeSchedule_Should_Return_Empty_For_Invalid_Id()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        var result = service.GetFormattedEmployeeSchedule(0);

        Assert.Empty(result);
    }

    [Fact]
    public void GetFormattedEmployeeSchedule_Should_Map_All_Fields_Correctly()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        var flight = new Flight
        {
            Id = 1,
            FlightNumber = "FL-1000",
            Date = new DateTime(2024, 1, 1),
            Route = new Route { Id = 1, RouteType = "INT" },
            Gate = new Gate { Id = 1 },
            Runway = new Runway { Id = 1 }
        };

        mockEmployeeFlightRepo.Setup(r => r.GetFlightsByEmployeeId(1)).Returns(new List<int> { 1 });
        mockFlightRepo.Setup(r => r.GetById(1)).Returns(flight);

        mockRouteRepo.Setup(r => r.GetRouteById(1)).Returns(flight.Route);
        mockGateService.Setup(r => r.GetById(1)).Returns(new Gate { Name = "G1" });
        mockRunwayService.Setup(r => r.GetByIdSafe(1)).Returns(new Runway { Name = "R1" });

        mockRouteService.Setup(r => r.NormalizeFlightType("INT")).Returns("International");
        mockRouteService.Setup(r => r.GetRelevantTime(It.IsAny<Route>())).Returns("10:00");

        var result = service.GetFormattedEmployeeSchedule(1);

        var item = result.First();

        Assert.Equal("1", item.Id);
        Assert.Equal("FL-1000", item.FlightNumber);
        Assert.Equal("International", item.FlightType);
        Assert.Equal(flight.Date.ToString("dd MMM yyyy"), item.Date);
        Assert.Equal("G1", item.GateName);
        Assert.Equal("R1", item.RunwayName);
        Assert.Equal("10:00", item.FlightTime);
    }

    [Fact]
    public void GetFormattedEmployeeSchedule_Should_Use_Default_For_Null_Gate_And_Runway()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        var flight = new Flight
        {
            Id = 1,
            FlightNumber = "FL-1000",
            Date = DateTime.Today,
            Route = new Route { Id = 1 },
            Gate = null,
            Runway = null
        };

        mockEmployeeFlightRepo.Setup(r => r.GetFlightsByEmployeeId(1)).Returns(new List<int> { 1 });
        mockFlightRepo.Setup(r => r.GetById(1)).Returns(flight);

        mockRouteRepo.Setup(r => r.GetRouteById(1)).Returns(flight.Route);
        mockRouteService.Setup(r => r.NormalizeFlightType(null)).Returns("Unknown");
        mockRouteService.Setup(r => r.GetRelevantTime(It.IsAny<Route>())).Returns("N/A");
        mockRunwayService.Setup(r => r.GetByIdSafe(0)).Returns((Runway)null);

        var result = service.GetFormattedEmployeeSchedule(1);

        var item = result.First();

        Assert.Equal("-", item.GateName);
        Assert.Equal("-", item.RunwayName);
    }

    [Fact]
    public void GetFormattedEmployeeSchedule_Should_Sort_By_Date()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        var f1 = new Flight { Id = 1, Date = DateTime.Today.AddDays(1), Route = new Route { Id = 1 } };
        var f2 = new Flight { Id = 2, Date = DateTime.Today, Route = new Route { Id = 1 } };

        mockEmployeeFlightRepo.Setup(r => r.GetFlightsByEmployeeId(1)).Returns(new List<int> { 1, 2 });
        mockFlightRepo.Setup(r => r.GetById(1)).Returns(f1);
        mockFlightRepo.Setup(r => r.GetById(2)).Returns(f2);

        mockRouteRepo.Setup(r => r.GetRouteById(It.IsAny<int>())).Returns(new Route());
        mockRouteService.Setup(r => r.NormalizeFlightType(It.IsAny<string>())).Returns("X");
        mockRouteService.Setup(r => r.GetRelevantTime(It.IsAny<Route>())).Returns("X");
        mockRunwayService.Setup(r => r.GetByIdSafe(It.IsAny<int>())).Returns((Runway)null);

        var result = service.GetFormattedEmployeeSchedule(1);

        Assert.Equal("2", result[0].Id);
    }

    [Fact]
    public void GetAvailableCrewGroupedByRole_Should_Filter_Unavailable()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        var flight = new Flight
        {
            Id = 1,
            Date = DateTime.Today,
            Route = new Route { Id = 1 }
        };

        var employees = new List<Employee>
        {
            new Employee { Id = 1 },
            new Employee { Id = 2 }
        };

        mockEmployeeRepo.Setup(r => r.GetAllEmployees()).Returns(employees);

        mockRouteRepo.Setup(r => r.GetRouteById(1)).Returns(new Route
        {
            Id = 1,
            DepartureTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(10)),
            ArrivalTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(12))
        });

        mockEmployeeFlightRepo.Setup(r => r.GetFlightsByEmployeeId(1)).Returns(new List<int>());

        mockEmployeeFlightRepo.Setup(r => r.GetFlightsByEmployeeId(2)).Returns(new List<int> { 99 });

        mockFlightRepo.Setup(r => r.GetById(99)).Returns(new Flight
        {
            Id = 99,
            Date = DateTime.Today,
            Route = new Route { Id = 2 }
        });

        mockRouteRepo.Setup(r => r.GetRouteById(2)).Returns(new Route
        {
            Id = 2,
            DepartureTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(11)),
            ArrivalTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(13))
        });

        var result = service.GetAvailableCrewGroupedByRole(flight);

        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public void GetAvailableCrewGroupedByRole_Should_Sort_By_Role_Then_Name()
    {
        var mockEmployeeFlightRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var service = new EmployeeFlightService(mockEmployeeFlightRepo.Object, mockEmployeeRepo.Object,
            mockFlightRepo.Object, mockRouteRepo.Object,
            mockGateService.Object, mockRunwayService.Object, mockRouteService.Object);

        var flight = new Flight
        {
            Id = 1,
            Date = DateTime.Today,
            Route = new Route { Id = 1 }
        };

        var employees = new List<Employee>
        {
            new Employee { Id = 1, Name = "Bob", Role = EmployeeRole.CoPilot },
            new Employee { Id = 2, Name = "Alice", Role = EmployeeRole.Pilot },
            new Employee { Id = 3, Name = "Charlie", Role = EmployeeRole.Pilot }
        };

        mockEmployeeRepo.Setup(r => r.GetAllEmployees()).Returns(employees);

        mockRouteRepo.Setup(r => r.GetRouteById(It.IsAny<int>())).Returns(new Route());
        mockEmployeeFlightRepo.Setup(r => r.GetFlightsByEmployeeId(It.IsAny<int>())).Returns(new List<int>());

        var result = service.GetAvailableCrewGroupedByRole(flight);

        Assert.Equal(new[] { 2, 3, 1 }, result.Select(e => e.Id));
    }
}
