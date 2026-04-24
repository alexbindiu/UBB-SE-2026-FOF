using System;
using System.Collections.Generic;
using System.Text;

using Moq;

using TicketSellingModule.Data.Repositories.Interfaces;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class RouteServiceTests
{
    private static RouteService BuildService(
        Mock<IRouteRepository> routeRepo,
        Mock<IFlightRepository> flightRepo,
        Mock<ICompanyRepository> companyRepo = null,
        Mock<IAirportRepository> airportRepo = null)
    {
        return new RouteService(
            routeRepo.Object,
            flightRepo.Object,
            (companyRepo ?? new Mock<ICompanyRepository>()).Object,
            (airportRepo ?? new Mock<IAirportRepository>()).Object);
    }

    [Fact]
    public void GetById_Should_Return_Route()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var route = new Route { RouteType = "DEP" };
        mockRouteRepo.Setup(r => r.GetRouteById(1)).Returns(route);

        var service = BuildService(mockRouteRepo, mockFlightRepo);

        Assert.Equal(route, service.GetById(1));
    }

    [Fact]
    public void GetById_Should_Return_Null_When_Not_Found()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockRouteRepo.Setup(r => r.GetRouteById(99)).Returns((Route)null);

        var service = BuildService(mockRouteRepo, mockFlightRepo);

        Assert.Null(service.GetById(99));
    }

    [Fact]
    public void GetAll_Should_Return_All_Routes()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var routes = new List<Route> { new Route(), new Route() };
        mockRouteRepo.Setup(r => r.GetAllRoutes()).Returns(routes);

        var service = BuildService(mockRouteRepo, mockFlightRepo);

        Assert.Equal(2, service.GetAll().Count);
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_Dash_For_Null_Or_Whitespace()
    {
        var service = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal("-", service.NormalizeFlightType(null));
        Assert.Equal("-", service.NormalizeFlightType(string.Empty));
        Assert.Equal("-", service.NormalizeFlightType("  "));
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_ARR_For_Arrival_Variants()
    {
        var service = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal("ARR", service.NormalizeFlightType("arr"));
        Assert.Equal("ARR", service.NormalizeFlightType("ARR"));
        Assert.Equal("ARR", service.NormalizeFlightType("arrival"));
        Assert.Equal("ARR", service.NormalizeFlightType("ARRIVAL"));
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_DEP_For_Departure_Variants()
    {
        var service = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal("DEP", service.NormalizeFlightType("dep"));
        Assert.Equal("DEP", service.NormalizeFlightType("DEP"));
        Assert.Equal("DEP", service.NormalizeFlightType("departure"));
        Assert.Equal("DEP", service.NormalizeFlightType("DEPARTURE"));
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_Uppercased_Value_For_Unknown_Type()
    {
        var service = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal("TRANSIT", service.NormalizeFlightType("transit"));
    }

    [Fact]
    public void GetRelevantTime_Should_Return_Dash_For_Null_Route()
    {
        var service = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal("-", service.GetRelevantTime(null));
    }

    [Fact]
    public void GetRelevantTime_Should_Return_ArrivalTime_For_ARR_Route()
    {
        var service = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());
        var route = new Route
        {
            RouteType = "ARR",
            ArrivalTime = new TimeOnly(14, 30),
            DepartureTime = new TimeOnly(10, 0)
        };

        Assert.Equal("14:30", service.GetRelevantTime(route));
    }

    [Fact]
    public void GetRelevantTime_Should_Return_DepartureTime_For_DEP_Route()
    {
        var service = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());
        var route = new Route
        {
            RouteType = "DEP",
            ArrivalTime = new TimeOnly(14, 30),
            DepartureTime = new TimeOnly(10, 0)
        };

        Assert.Equal("10:00", service.GetRelevantTime(route));
    }

    [Fact]
    public void AddWithInitialFlight_Should_Throw_On_Gate_Conflict()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockCompanyRepo = new Mock<ICompanyRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        var start = new DateTime(2025, 1, 1);
        var dep = new TimeOnly(10, 0);
        var arr = new TimeOnly(12, 0);

        var existingFlight = new Flight
        {
            Gate = new Gate { Id = 1 },
            Runway = new Runway { Id = 99 },
            Route = new Route { Id = 5 },
            Date = start
        };
        var existingRoute = new Route
        {
            DepartureTime = new TimeOnly(10, 30),
            ArrivalTime = new TimeOnly(11, 30)
        };

        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(r => r.GetRouteById(5)).Returns(existingRoute);

        var service = new RouteService(mockRouteRepo.Object, mockFlightRepo.Object,
            mockCompanyRepo.Object, mockAirportRepo.Object);

        Assert.Throws<InvalidOperationException>(() =>
            service.AddWithInitialFlight(1, 1, "DEP", 1, start, start, dep, arr, 100, "FL001", 2, 1));
    }

    [Fact]
    public void AddWithInitialFlight_Should_Throw_On_Runway_Conflict()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockCompanyRepo = new Mock<ICompanyRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        var start = new DateTime(2025, 1, 1);
        var dep = new TimeOnly(10, 0);
        var arr = new TimeOnly(12, 0);

        var existingFlight = new Flight
        {
            Gate = new Gate { Id = 99 },
            Runway = new Runway { Id = 2 },
            Route = new Route { Id = 5 },
            Date = start
        };
        var existingRoute = new Route
        {
            DepartureTime = new TimeOnly(10, 30),
            ArrivalTime = new TimeOnly(11, 30)
        };

        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(r => r.GetRouteById(5)).Returns(existingRoute);

        var service = new RouteService(mockRouteRepo.Object, mockFlightRepo.Object,
            mockCompanyRepo.Object, mockAirportRepo.Object);

        Assert.Throws<InvalidOperationException>(() =>
            service.AddWithInitialFlight(1, 1, "DEP", 1, start, start, dep, arr, 100, "FL001", 2, 1));
    }

    [Fact]
    public void AddWithInitialFlight_Should_Succeed_With_No_Conflicts()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockCompanyRepo = new Mock<ICompanyRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight>());
        mockCompanyRepo.Setup(r => r.GetCompanyById(It.IsAny<int>())).Returns(new Company());
        mockAirportRepo.Setup(r => r.GetAirportById(It.IsAny<int>())).Returns(new Airport());
        mockRouteRepo.Setup(r => r.AddRoute(It.IsAny<Route>())).Returns(10);

        var service = new RouteService(mockRouteRepo.Object, mockFlightRepo.Object,
            mockCompanyRepo.Object, mockAirportRepo.Object);

        var start = new DateTime(2025, 1, 1);
        var result = service.AddWithInitialFlight(1, 1, "DEP", 1, start, start,
            new TimeOnly(10, 0), new TimeOnly(12, 0), 100, "FL001", 1, 1);

        Assert.Equal(10, result);
        mockRouteRepo.Verify(r => r.AddRoute(It.IsAny<Route>()), Times.Once);
        mockFlightRepo.Verify(r => r.AddFlight(It.IsAny<Flight>()), Times.Once);
    }

    [Fact]
    public void AddWithInitialFlight_Should_Not_Conflict_When_Times_Do_Not_Overlap()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockCompanyRepo = new Mock<ICompanyRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        var start = new DateTime(2025, 1, 1);
        var existingFlight = new Flight
        {
            Gate = new Gate { Id = 1 },
            Runway = new Runway { Id = 1 },
            Route = new Route { Id = 5 }
        };
        var existingRoute = new Route
        {
            DepartureTime = new TimeOnly(8, 0),
            ArrivalTime = new TimeOnly(9, 0)
        };

        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(r => r.GetRouteById(5)).Returns(existingRoute);
        mockCompanyRepo.Setup(r => r.GetCompanyById(It.IsAny<int>())).Returns(new Company());
        mockAirportRepo.Setup(r => r.GetAirportById(It.IsAny<int>())).Returns(new Airport());
        mockRouteRepo.Setup(r => r.AddRoute(It.IsAny<Route>())).Returns(11);

        var service = new RouteService(mockRouteRepo.Object, mockFlightRepo.Object,
            mockCompanyRepo.Object, mockAirportRepo.Object);

        var result = service.AddWithInitialFlight(1, 1, "DEP", 1, start, start,
            new TimeOnly(10, 0), new TimeOnly(12, 0), 100, "FL002", 1, 1);

        Assert.Equal(11, result);
    }
}
