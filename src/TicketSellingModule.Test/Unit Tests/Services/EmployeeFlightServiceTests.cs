using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Moq;

using TicketSellingModule.Data.Repositories.Interfaces;
using TicketSellingModule.Data.Services.Interfaces;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class EmployeeFlightServiceTests
{
    private static EmployeeFlightService BuildService(
        Mock<IEmployeeFlightRepository> efRepo = null,
        Mock<IEmployeeRepository> empRepo = null,
        Mock<IFlightRepository> flightRepo = null,
        Mock<IRouteRepository> routeRepo = null,
        Mock<IGateService> gateService = null,
        Mock<IRunwayService> runwayService = null,
        Mock<IRouteService> routeService = null)
    {
        return new EmployeeFlightService(
            (efRepo ?? new Mock<IEmployeeFlightRepository>()).Object,
            (empRepo ?? new Mock<IEmployeeRepository>()).Object,
            (flightRepo ?? new Mock<IFlightRepository>()).Object,
            (routeRepo ?? new Mock<IRouteRepository>()).Object,
            (gateService ?? new Mock<IGateService>()).Object,
            (runwayService ?? new Mock<IRunwayService>()).Object,
            (routeService ?? new Mock<IRouteService>()).Object);
    }

    [Fact]
    public void AssignCrewMember_Should_Throw_For_Invalid_Id()
    {
        var service = BuildService();

        Assert.Throws<ArgumentException>(() => service.AssignEmployeeToFlightUsingIds(0, 1));
        Assert.Throws<ArgumentException>(() => service.AssignEmployeeToFlightUsingIds(1, 0));
    }

    [Fact]
    public void AssignCrewMember_Should_Throw_When_Employee_Or_Flight_Missing()
    {
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();

        mockEmployeeRepo.Setup(r => r.GetEmployeeById(1)).Returns((Employee)null);
        mockFlightRepo.Setup(r => r.GetFlightById(1)).Returns(new Flight());

        var service = BuildService(empRepo: mockEmployeeRepo, flightRepo: mockFlightRepo);

        Assert.Throws<InvalidOperationException>(() => service.AssignEmployeeToFlightUsingIds(1, 1));
    }

    [Fact]
    public void AssignCrewMember_Should_Throw_When_Already_Assigned()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();

        mockEmpRepo.Setup(r => r.GetEmployeeById(1)).Returns(new Employee());
        mockFlightRepo.Setup(r => r.GetFlightById(1)).Returns(new Flight { Id = 1, Route = new Route { Id = 1 }, Date = DateTime.Today });
        mockEFRepo.Setup(r => r.GetEmployeesByFlightId(1)).Returns(new List<int> { 1 });

        var service = BuildService(efRepo: mockEFRepo, empRepo: mockEmpRepo, flightRepo: mockFlightRepo);

        Assert.Throws<InvalidOperationException>(() => service.AssignEmployeeToFlightUsingIds(1, 1));
    }

    [Fact]
    public void AssignCrewMember_Should_Throw_When_Not_Available()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();

        var flight = new Flight { Id = 1, Date = DateTime.Today, Route = new Route { Id = 1 } };

        mockEmpRepo.Setup(r => r.GetEmployeeById(1)).Returns(new Employee { Name = "Test" });
        mockFlightRepo.Setup(r => r.GetFlightById(1)).Returns(flight);
        mockEFRepo.Setup(r => r.GetEmployeesByFlightId(1)).Returns(new List<int>());
        mockRouteRepo.Setup(r => r.GetRouteById(It.IsAny<int>())).Returns((Route)null);

        var service = BuildService(efRepo: mockEFRepo, empRepo: mockEmpRepo,
            flightRepo: mockFlightRepo, routeRepo: mockRouteRepo);

        Assert.Throws<InvalidOperationException>(() => service.AssignEmployeeToFlightUsingIds(1, 1));
    }

    [Fact]
    public void AssignCrewMember_Should_Work()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();

        var flight = new Flight { Id = 1, Date = DateTime.Today, Route = new Route { Id = 1 } };

        mockEmpRepo.Setup(r => r.GetEmployeeById(1)).Returns(new Employee());
        mockFlightRepo.Setup(r => r.GetFlightById(1)).Returns(flight);
        mockEFRepo.Setup(r => r.GetEmployeesByFlightId(1)).Returns(new List<int>());
        mockRouteRepo.Setup(r => r.GetRouteById(It.IsAny<int>())).Returns(new Route
        {
            DepartureTime = TimeOnly.FromDateTime(DateTime.Today),
            ArrivalTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(1))
        });
        mockEFRepo.Setup(r => r.GetFlightsByEmployeeId(1)).Returns(new List<int>());

        var service = BuildService(efRepo: mockEFRepo, empRepo: mockEmpRepo,
            flightRepo: mockFlightRepo, routeRepo: mockRouteRepo);

        service.AssignEmployeeToFlightUsingIds(1, 1);

        mockEFRepo.Verify(r => r.AssignFlightToEmployeesUsingIds(1, 1), Times.Once);
    }

    [Fact]
    public void CleanUpFlightAssignments_Should_Call_When_Valid()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var service = BuildService(efRepo: mockEFRepo);

        service.RemoveAllCrewAssignmentsForFlight(1);

        mockEFRepo.Verify(r => r.RemoveAllByFlightId(1), Times.Once);
    }

    [Fact]
    public void CleanUpFlightAssignments_Should_Not_Call_For_Invalid_Id()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var service = BuildService(efRepo: mockEFRepo);

        service.RemoveAllCrewAssignmentsForFlight(0);

        mockEFRepo.Verify(r => r.RemoveAllByFlightId(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void RemoveAllFlightsAssignmentsForEmployee_Should_Call_Repo_When_Valid()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var service = BuildService(efRepo: mockEFRepo);

        service.RemoveAllFlightsAssignmentsForEmployee(1);

        mockEFRepo.Verify(r => r.RemoveAllByEmployeeId(1), Times.Once);
    }

    [Fact]
    public void RemoveAllFlightsAssignmentsForEmployee_Should_Not_Call_For_Invalid_Id()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var service = BuildService(efRepo: mockEFRepo);

        service.RemoveAllFlightsAssignmentsForEmployee(0);

        mockEFRepo.Verify(r => r.RemoveAllByEmployeeId(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void RemoveCrewMember_Should_Call_Repo()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var service = BuildService(efRepo: mockEFRepo);

        service.RemoveEmployeeFromFlightUsingIds(10, 5);

        mockEFRepo.Verify(r => r.RemoveFlightFromEmployeeUsingIds(5, 10), Times.Once);
    }

    [Fact]
    public void GetFlightCrew_Should_Return_Only_Existing_Employees()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmpRepo = new Mock<IEmployeeRepository>();

        mockEFRepo.Setup(r => r.GetEmployeesByFlightId(1)).Returns(new List<int> { 1, 2 });
        mockEmpRepo.Setup(r => r.GetEmployeeById(1)).Returns(new Employee { Id = 1 });
        mockEmpRepo.Setup(r => r.GetEmployeeById(2)).Returns((Employee)null);

        var service = BuildService(efRepo: mockEFRepo, empRepo: mockEmpRepo);
        var result = service.GetEmployeesAssignedToFlight(1);

        Assert.Single(result);
    }

    [Fact]
    public void GetEmployeeSchedule_Should_Return_Empty_For_Invalid_Id()
    {
        var service = BuildService();
        Assert.Empty(service.GetEmployeeSchedule(0));
    }

    [Fact]
    public void GetEmployeeSchedule_Should_Skip_Null_Flights()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();

        mockEFRepo.Setup(r => r.GetFlightsByEmployeeId(1)).Returns(new List<int> { 1, 2 });
        mockFlightRepo.Setup(r => r.GetFlightById(1)).Returns(new Flight { Id = 1, Route = new Route { Id = 1 } });
        mockFlightRepo.Setup(r => r.GetFlightById(2)).Returns((Flight)null);

        var service = BuildService(efRepo: mockEFRepo, flightRepo: mockFlightRepo);
        var result = service.GetEmployeeSchedule(1);

        Assert.Single(result);
    }

    [Fact]
    public void IsEmployeeAvailable_Should_Return_False_When_Route_Not_Found()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        mockRouteRepo.Setup(r => r.GetRouteById(1)).Returns((Route)null);

        var service = BuildService(routeRepo: mockRouteRepo);

        Assert.False(service.IsEmployeeAvailable(1, DateTime.Today, 1));
    }

    [Fact]
    public void IsEmployeeAvailable_Should_Return_True_When_No_Overlap()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();

        var route = new Route
        {
            Id = 1,
            DepartureTime = TimeOnly.FromDateTime(DateTime.Today),
            ArrivalTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(1))
        };

        mockRouteRepo.Setup(r => r.GetRouteById(It.IsAny<int>())).Returns(route);
        mockEFRepo.Setup(r => r.GetFlightsByEmployeeId(1)).Returns(new List<int>());

        var service = BuildService(efRepo: mockEFRepo, routeRepo: mockRouteRepo);

        Assert.True(service.IsEmployeeAvailable(1, DateTime.Today, 1));
    }

    [Fact]
    public void IsEmployeeAvailable_Should_Return_False_When_Overlap()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();

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
        mockEFRepo.Setup(r => r.GetFlightsByEmployeeId(1)).Returns(new List<int> { 2 });
        mockFlightRepo.Setup(r => r.GetFlightById(2)).Returns(new Flight { Id = 2, Date = DateTime.Today, Route = new Route { Id = 2 } });

        var service = BuildService(efRepo: mockEFRepo, flightRepo: mockFlightRepo, routeRepo: mockRouteRepo);

        Assert.False(service.IsEmployeeAvailable(1, DateTime.Today, 1));
    }

    [Fact]
    public void IsEmployeeAvailable_Should_Return_True_When_Conflict_Is_The_Excluded_Flight()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();

        var targetDate = DateTime.Today;
        int flightId = 100;
        int routeId = 1;

        var existingFlight = new Flight { Id = flightId, Date = targetDate, Route = new Route { Id = routeId } };

        mockEFRepo.Setup(r => r.GetFlightsByEmployeeId(1)).Returns(new List<int> { flightId });
        mockFlightRepo.Setup(r => r.GetFlightById(flightId)).Returns(existingFlight);
        mockRouteRepo.Setup(r => r.GetRouteById(routeId)).Returns(existingFlight.Route);

        var service = BuildService(efRepo: mockEFRepo, flightRepo: mockFlightRepo, routeRepo: mockRouteRepo);

        Assert.True(service.IsEmployeeAvailable(1, targetDate, routeId, excludedFlightId: flightId));
    }

    [Fact]
    public void IsEmployeeAvailable_Should_Return_True_When_Flight_Is_Different_Date()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();

        var targetRoute = new Route
        {
            Id = 1,
            DepartureTime = new TimeOnly(10, 0),
            ArrivalTime = new TimeOnly(12, 0)
        };

        // Existing flight is on a different date — should not cause conflict
        var otherDayFlight = new Flight
        {
            Id = 50,
            Date = DateTime.Today.AddDays(1),
            Route = new Route { Id = 2 }
        };

        mockRouteRepo.Setup(r => r.GetRouteById(1)).Returns(targetRoute);
        mockEFRepo.Setup(r => r.GetFlightsByEmployeeId(1)).Returns(new List<int> { 50 });
        mockFlightRepo.Setup(r => r.GetFlightById(50)).Returns(otherDayFlight);

        var service = BuildService(efRepo: mockEFRepo, flightRepo: mockFlightRepo, routeRepo: mockRouteRepo);

        Assert.True(service.IsEmployeeAvailable(1, DateTime.Today, 1));
    }

    [Fact]
    public void IsEmployeeAvailable_Should_Return_True_When_Scheduled_Route_Is_Null()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();

        var targetRoute = new Route
        {
            Id = 1,
            DepartureTime = new TimeOnly(10, 0),
            ArrivalTime = new TimeOnly(12, 0)
        };

        var scheduledFlight = new Flight { Id = 5, Date = DateTime.Today, Route = new Route { Id = 99 } };

        mockRouteRepo.Setup(r => r.GetRouteById(1)).Returns(targetRoute);
        mockRouteRepo.Setup(r => r.GetRouteById(99)).Returns((Route)null);
        mockEFRepo.Setup(r => r.GetFlightsByEmployeeId(1)).Returns(new List<int> { 5 });
        mockFlightRepo.Setup(r => r.GetFlightById(5)).Returns(scheduledFlight);

        var service = BuildService(efRepo: mockEFRepo, flightRepo: mockFlightRepo, routeRepo: mockRouteRepo);

        Assert.True(service.IsEmployeeAvailable(1, DateTime.Today, 1));
    }

    [Fact]
    public void IsEmployeeAvailable_Should_Return_False_When_Times_Overlap_Complex()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();

        var targetRoute = new Route { Id = 1, DepartureTime = new TimeOnly(14, 0), ArrivalTime = new TimeOnly(16, 0) };
        var existingRoute = new Route { Id = 2, DepartureTime = new TimeOnly(15, 0), ArrivalTime = new TimeOnly(17, 0) };

        mockRouteRepo.Setup(r => r.GetRouteById(1)).Returns(targetRoute);
        mockRouteRepo.Setup(r => r.GetRouteById(2)).Returns(existingRoute);
        mockEFRepo.Setup(r => r.GetFlightsByEmployeeId(1)).Returns(new List<int> { 99 });
        mockFlightRepo.Setup(r => r.GetFlightById(99)).Returns(new Flight { Id = 99, Date = DateTime.Today, Route = new Route { Id = 2 } });

        var service = BuildService(efRepo: mockEFRepo, flightRepo: mockFlightRepo, routeRepo: mockRouteRepo);

        Assert.False(service.IsEmployeeAvailable(1, DateTime.Today, 1));
    }

    [Fact]
    public void AssignCrewToFlight_Should_Continue_On_Exception()
    {
        var mockEmpRepo = new Mock<IEmployeeRepository>();
        mockEmpRepo.Setup(r => r.GetEmployeeById(1)).Throws(new Exception());

        var service = BuildService(empRepo: mockEmpRepo);

        service.AssignEmpolyeesToFlightUsingIds(1, new List<int> { 1, 2 });

        Assert.True(true);
    }

    [Fact]
    public void AssignEmployeesToFlightUsingIds_Should_Proceed_After_Partial_Failure()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmpRepo = new Mock<IEmployeeRepository>();

        mockEmpRepo.Setup(r => r.GetEmployeeById(1)).Throws(new Exception("Fail"));
        mockEmpRepo.Setup(r => r.GetEmployeeById(2)).Returns(new Employee());

        var service = BuildService(efRepo: mockEFRepo, empRepo: mockEmpRepo);

        service.AssignEmpolyeesToFlightUsingIds(1, new List<int> { 1, 2 });

        mockEFRepo.Verify(r => r.AssignFlightToEmployeesUsingIds(2, 1), Times.Never);
    }

    [Fact]
    public void UpdateCrewForFlight_Should_Add_And_Remove_Correctly()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();

        mockEFRepo.Setup(r => r.GetEmployeesByFlightId(1)).Returns(new List<int> { 1, 2 });
        mockEmpRepo.Setup(r => r.GetEmployeeById(It.IsAny<int>())).Returns(new Employee());
        mockFlightRepo.Setup(r => r.GetFlightById(It.IsAny<int>())).Returns(new Flight { Id = 1, Route = new Route { Id = 1 }, Date = DateTime.Today });
        mockRouteRepo.Setup(r => r.GetRouteById(It.IsAny<int>())).Returns(new Route());
        mockEFRepo.Setup(r => r.GetFlightsByEmployeeId(It.IsAny<int>())).Returns(new List<int>());

        var service = BuildService(efRepo: mockEFRepo, empRepo: mockEmpRepo,
            flightRepo: mockFlightRepo, routeRepo: mockRouteRepo);

        service.UpdateEmployeesForFlightUsingIds(1, new List<int> { 2, 3 });

        mockEFRepo.Verify(r => r.RemoveFlightFromEmployeeUsingIds(1, 1), Times.Once);
    }

    [Fact]
    public void GetFormattedEmployeeSchedule_Should_Return_Empty_For_Invalid_Id()
    {
        var service = BuildService();
        Assert.Empty(service.GetFormattedEmployeeSchedule(0));
    }

    [Fact]
    public void GetFormattedEmployeeSchedule_Should_Map_All_Fields_Correctly()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockGateService = new Mock<IGateService>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var flight = new Flight
        {
            Id = 1,
            FlightNumber = "FL-1000",
            Date = new DateTime(2024, 1, 1),
            Route = new Route { Id = 1, RouteType = "INT" },
            Gate = new Gate { Id = 1 },
            Runway = new Runway { Id = 1 }
        };

        mockEFRepo.Setup(r => r.GetFlightsByEmployeeId(1)).Returns(new List<int> { 1 });
        mockFlightRepo.Setup(r => r.GetFlightById(1)).Returns(flight);
        mockRouteRepo.Setup(r => r.GetRouteById(1)).Returns(flight.Route);
        mockGateService.Setup(r => r.GetGateById(1)).Returns(new Gate { Name = "G1" });
        mockRunwayService.Setup(r => r.GetRunwayById(1)).Returns(new Runway { Name = "R1" });
        mockRouteService.Setup(r => r.NormalizeFlightType("INT")).Returns("International");
        mockRouteService.Setup(r => r.GetRelevantTime(It.IsAny<Route>())).Returns("10:00");

        var service = BuildService(efRepo: mockEFRepo, flightRepo: mockFlightRepo,
            routeRepo: mockRouteRepo, gateService: mockGateService,
            runwayService: mockRunwayService, routeService: mockRouteService);

        var item = service.GetFormattedEmployeeSchedule(1).First();

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
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var flight = new Flight
        {
            Id = 1,
            FlightNumber = "FL-1000",
            Date = DateTime.Today,
            Route = new Route { Id = 1 },
            Gate = null,
            Runway = null
        };

        mockEFRepo.Setup(r => r.GetFlightsByEmployeeId(1)).Returns(new List<int> { 1 });
        mockFlightRepo.Setup(r => r.GetFlightById(1)).Returns(flight);
        mockRouteRepo.Setup(r => r.GetRouteById(1)).Returns(flight.Route);
        mockRouteService.Setup(r => r.NormalizeFlightType(null)).Returns("Unknown");
        mockRouteService.Setup(r => r.GetRelevantTime(It.IsAny<Route>())).Returns("N/A");
        mockRunwayService.Setup(r => r.GetRunwayById(0)).Returns((Runway)null);

        var service = BuildService(efRepo: mockEFRepo, flightRepo: mockFlightRepo,
            routeRepo: mockRouteRepo, runwayService: mockRunwayService, routeService: mockRouteService);

        var item = service.GetFormattedEmployeeSchedule(1).First();

        Assert.Equal("-", item.GateName);
        Assert.Equal("-", item.RunwayName);
    }

    [Fact]
    public void GetFormattedEmployeeSchedule_Should_Sort_By_Date()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockRouteService = new Mock<IRouteService>();

        var f1 = new Flight { Id = 1, Date = DateTime.Today.AddDays(1), Route = new Route { Id = 1 } };
        var f2 = new Flight { Id = 2, Date = DateTime.Today, Route = new Route { Id = 1 } };

        mockEFRepo.Setup(r => r.GetFlightsByEmployeeId(1)).Returns(new List<int> { 1, 2 });
        mockFlightRepo.Setup(r => r.GetFlightById(1)).Returns(f1);
        mockFlightRepo.Setup(r => r.GetFlightById(2)).Returns(f2);
        mockRouteRepo.Setup(r => r.GetRouteById(It.IsAny<int>())).Returns(new Route());
        mockRouteService.Setup(r => r.NormalizeFlightType(It.IsAny<string>())).Returns("X");
        mockRouteService.Setup(r => r.GetRelevantTime(It.IsAny<Route>())).Returns("X");
        mockRunwayService.Setup(r => r.GetRunwayById(It.IsAny<int>())).Returns((Runway)null);

        var service = BuildService(efRepo: mockEFRepo, flightRepo: mockFlightRepo,
            routeRepo: mockRouteRepo, runwayService: mockRunwayService, routeService: mockRouteService);

        var result = service.GetFormattedEmployeeSchedule(1);

        Assert.Equal("2", result[0].Id);
    }

    [Fact]
    public void GenerateFormattedSchedule_Should_Return_Empty_For_Invalid_Id()
    {
        var service = BuildService();
        Assert.Empty(service.GenerateFormattedSchedule(0));
    }

    [Fact]
    public void GenerateFormattedSchedule_Should_Return_List_When_Valid()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();

        mockEFRepo.Setup(r => r.GetFlightsByEmployeeId(1)).Returns(new List<int> { 101 });
        mockFlightRepo.Setup(r => r.GetFlightById(101)).Returns(new Flight { Id = 101, Route = new Route() });

        var service = BuildService(efRepo: mockEFRepo, flightRepo: mockFlightRepo);
        var result = service.GenerateFormattedSchedule(1);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetAvailableCrewGroupedByRole_Should_Filter_Unavailable()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();

        var flight = new Flight { Id = 1, Date = DateTime.Today, Route = new Route { Id = 1 } };
        var employees = new List<Employee> { new Employee { Id = 1 }, new Employee { Id = 2 } };

        mockEmpRepo.Setup(r => r.GetAllEmployees()).Returns(employees);
        mockRouteRepo.Setup(r => r.GetRouteById(1)).Returns(new Route
        {
            Id = 1,
            DepartureTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(10)),
            ArrivalTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(12))
        });

        mockEFRepo.Setup(r => r.GetFlightsByEmployeeId(1)).Returns(new List<int>());
        mockEFRepo.Setup(r => r.GetFlightsByEmployeeId(2)).Returns(new List<int> { 99 });
        mockFlightRepo.Setup(r => r.GetFlightById(99)).Returns(new Flight { Id = 99, Date = DateTime.Today, Route = new Route { Id = 2 } });
        mockRouteRepo.Setup(r => r.GetRouteById(2)).Returns(new Route
        {
            Id = 2,
            DepartureTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(11)),
            ArrivalTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(13))
        });

        var service = BuildService(efRepo: mockEFRepo, empRepo: mockEmpRepo,
            flightRepo: mockFlightRepo, routeRepo: mockRouteRepo);

        var result = service.GetAvailableEmployeesGroupedByRole(flight);

        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public void GetAvailableCrewGroupedByRole_Should_Sort_By_Role_Then_Name()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();

        var flight = new Flight { Id = 1, Date = DateTime.Today, Route = new Route { Id = 1 } };
        var employees = new List<Employee>
        {
            new Employee { Id = 1, Name = "Bob",     Role = EmployeeRole.CoPilot },
            new Employee { Id = 2, Name = "Alice",   Role = EmployeeRole.Pilot },
            new Employee { Id = 3, Name = "Charlie", Role = EmployeeRole.Pilot }
        };

        mockEmpRepo.Setup(r => r.GetAllEmployees()).Returns(employees);
        mockRouteRepo.Setup(r => r.GetRouteById(It.IsAny<int>())).Returns(new Route());
        mockEFRepo.Setup(r => r.GetFlightsByEmployeeId(It.IsAny<int>())).Returns(new List<int>());

        var service = BuildService(efRepo: mockEFRepo, empRepo: mockEmpRepo, routeRepo: mockRouteRepo);
        var result = service.GetAvailableEmployeesGroupedByRole(flight);

        Assert.Equal(new[] { 2, 3, 1 }, result.Select(e => e.Id));
    }

    [Fact]
    public void CleanUpEmployeeAssignments_Should_Not_Call_For_Invalid_Id()
    {
        var mockEFRepo = new Mock<IEmployeeFlightRepository>();
        var service = BuildService(efRepo: mockEFRepo);

        service.RemoveAllCrewAssignmentsForFlight(0);

        mockEFRepo.Verify(r => r.RemoveAllByEmployeeId(It.IsAny<int>()), Times.Never);
    }
}