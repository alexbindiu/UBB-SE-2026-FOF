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

        Assert.Equal(route, service.GetRouteById(1));
    }

    [Fact]
    public void GetById_Should_Return_Null_When_Not_Found()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockRouteRepo.Setup(r => r.GetRouteById(99)).Returns((Route)null);

        var service = BuildService(mockRouteRepo, mockFlightRepo);

        Assert.Null(service.GetRouteById(99));
    }

    [Fact]
    public void GetAll_Should_Return_All_Routes()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var routes = new List<Route> { new Route(), new Route() };
        mockRouteRepo.Setup(r => r.GetAllRoutes()).Returns(routes);

        var service = BuildService(mockRouteRepo, mockFlightRepo);

        Assert.Equal(2, service.GetAllRoutes().Count);
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

    [Fact]
    public void AddWithInitialFlight_Should_Throw_On_Gate_Conflict()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var date = new DateTime(2025, 1, 1);

        var existingFlight = new Flight { Date = date, Gate = new Gate { Id = 10 }, Runway = new Runway { Id = 20 }, Route = new Route { Id = 5 }, FlightNumber = "EX123" };
        var existingRoute = new Route { DepartureTime = new TimeOnly(10, 0), ArrivalTime = new TimeOnly(12, 0) };

        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(r => r.GetRouteById(5)).Returns(existingRoute);

        var service = BuildService(mockRouteRepo, mockFlightRepo);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.AddWithInitialFlight(1, 1, "DEP", 0, date, date, new TimeOnly(11, 0), new TimeOnly(13, 0), 100, "NEW123", 99, 10));

        Assert.Contains("Conflict", ex.Message);
    }

    [Fact]
    public void AddWithInitialFlight_Should_Throw_On_Runway_Conflict()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var date = new DateTime(2025, 1, 1);

        var existingFlight = new Flight { Date = date, Gate = new Gate { Id = 10 }, Runway = new Runway { Id = 20 }, Route = new Route { Id = 5 }, FlightNumber = "EX123" };
        var existingRoute = new Route { DepartureTime = new TimeOnly(10, 0), ArrivalTime = new TimeOnly(12, 0) };

        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(r => r.GetRouteById(5)).Returns(existingRoute);

        var service = BuildService(mockRouteRepo, mockFlightRepo);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.AddWithInitialFlight(1, 1, "DEP", 0, date, date, new TimeOnly(11, 0), new TimeOnly(13, 0), 100, "NEW123", 20, 99));

        Assert.Contains("Conflict", ex.Message);
    }

    [Fact]
    public void AddWithInitialFlight_Should_Handle_Midnight_Wrap_Overlaps()
    {
        var date = new DateTime(2025, 1, 1);
        var existingRoute = new Route { DepartureTime = new TimeOnly(23, 30), ArrivalTime = new TimeOnly(1, 30) };
        {
            var mockRouteRepo = new Mock<IRouteRepository>();
            var mockFlightRepo = new Mock<IFlightRepository>();
            var existingFlight = new Flight { Date = date, Gate = new Gate { Id = 1 }, Runway = new Runway { Id = 1 }, Route = new Route { Id = 5 } };
            mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight> { existingFlight });
            mockRouteRepo.Setup(r => r.GetRouteById(5)).Returns(existingRoute);
            var service = BuildService(mockRouteRepo, mockFlightRepo);

            Assert.Throws<InvalidOperationException>(() =>
                service.AddWithInitialFlight(1, 1, "DEP", 0, date, date, new TimeOnly(23, 0), new TimeOnly(1, 0), 100, "WRAP", 1, 1));
        }

        {
            var mockRouteRepo = new Mock<IRouteRepository>();
            var mockFlightRepo = new Mock<IFlightRepository>();
            var existingFlight = new Flight { Date = date, Gate = new Gate { Id = 1 }, Runway = new Runway { Id = 1 }, Route = new Route { Id = 5 } };
            mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight> { existingFlight });
            mockRouteRepo.Setup(r => r.GetRouteById(5)).Returns(existingRoute);
            var service = BuildService(mockRouteRepo, mockFlightRepo);

            Assert.Throws<InvalidOperationException>(() =>
                service.AddWithInitialFlight(1, 1, "DEP", 0, date, date, new TimeOnly(22, 0), new TimeOnly(0, 30), 100, "WRAP2", 1, 1));
        }
    }

    [Fact]
    public void AddWithInitialFlight_Should_Continue_If_Existing_Route_Is_Null()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var date = new DateTime(2025, 1, 1);

        var existingFlight = new Flight { Date = date, Gate = new Gate { Id = 1 }, Runway = new Runway { Id = 1 }, Route = new Route { Id = 999 } };

        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(r => r.GetRouteById(999)).Returns((Route)null);
        mockRouteRepo.Setup(r => r.AddRoute(It.IsAny<Route>())).Returns(1);

        var service = BuildService(mockRouteRepo, mockFlightRepo);

        int resultId = service.AddWithInitialFlight(1, 1, "DEP", 0, date, date, new TimeOnly(10, 0), new TimeOnly(12, 0), 100, "VALID", 1, 1);

        Assert.Equal(1, resultId);
    }

    [Fact]
    public void AddWithInitialFlight_Should_Skip_Flights_On_Different_Date()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockCompanyRepo = new Mock<ICompanyRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        var targetDate = new DateTime(2025, 1, 1);
        var differentDate = new DateTime(2025, 2, 1);

        var otherDayFlight = new Flight
        {
            Date = differentDate,
            Gate = new Gate { Id = 1 },
            Runway = new Runway { Id = 1 },
            Route = new Route { Id = 5 }
        };

        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight> { otherDayFlight });
        mockCompanyRepo.Setup(r => r.GetCompanyById(It.IsAny<int>())).Returns(new Company());
        mockAirportRepo.Setup(r => r.GetAirportById(It.IsAny<int>())).Returns(new Airport());
        mockRouteRepo.Setup(r => r.AddRoute(It.IsAny<Route>())).Returns(5);

        var service = new RouteService(mockRouteRepo.Object, mockFlightRepo.Object,
            mockCompanyRepo.Object, mockAirportRepo.Object);

        var result = service.AddWithInitialFlight(1, 1, "DEP", 0, targetDate, targetDate,
            new TimeOnly(10, 0), new TimeOnly(12, 0), 100, "FL999", 1, 1);

        Assert.Equal(5, result);
        mockRouteRepo.Verify(r => r.GetRouteById(It.IsAny<int>()), Times.Never);
    }
}