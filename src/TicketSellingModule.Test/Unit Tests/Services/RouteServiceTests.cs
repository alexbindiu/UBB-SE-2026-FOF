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
    public void GetById_Should_Return_Route_When_Route_Exists()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var route = new Route { RouteType = "DEP" };
        mockRouteRepo.Setup(getRoute => getRoute.GetRouteById(1)).Returns(route);

        var routeService = BuildService(mockRouteRepo, mockFlightRepo);

        Assert.Equal(route, routeService.GetRouteById(1));
    }

    [Fact]
    public void GetById_Should_Return_Null_When_Route_Not_Found()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockRouteRepo.Setup(getNoRoute => getNoRoute.GetRouteById(99)).Returns((Route)null);
        var routeService = BuildService(mockRouteRepo, mockFlightRepo);

        Assert.Null(routeService.GetRouteById(99));
    }

    [Fact]
    public void GetAll_Should_Return_All_Routes_Always()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var routes = new List<Route> { new Route(), new Route() };
        mockRouteRepo.Setup(getAllRoutes => getAllRoutes.GetAllRoutes()).Returns(routes);

        var routeService = BuildService(mockRouteRepo, mockFlightRepo);

        Assert.Equal(2, routeService.GetAllRoutes().Count);
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_Dash_For_Null()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal("-", routeService.NormalizeFlightType(null));
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_Dash_For_Empty_String()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal("-", routeService.NormalizeFlightType(string.Empty));
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_Dash_For_Whitespace()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal("-", routeService.NormalizeFlightType("  "));
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_ARR_For_Arrival_Variants()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal("ARR", routeService.NormalizeFlightType("arr"));
        Assert.Equal("ARR", routeService.NormalizeFlightType("ARR"));
        Assert.Equal("ARR", routeService.NormalizeFlightType("arrival"));
        Assert.Equal("ARR", routeService.NormalizeFlightType("ARRIVAL"));
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_DEP_For_Departure_Variants()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal("DEP", routeService.NormalizeFlightType("dep"));
        Assert.Equal("DEP", routeService.NormalizeFlightType("DEP"));
        Assert.Equal("DEP", routeService.NormalizeFlightType("departure"));
        Assert.Equal("DEP", routeService.NormalizeFlightType("DEPARTURE"));
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_Uppercased_Value_For_Unknown_Type()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal("TRANSIT", routeService.NormalizeFlightType("transit"));
    }

    [Fact]
    public void GetRelevantTime_Should_Return_Dash_For_Null_Route()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal("-", routeService.GetRelevantTime(null));
    }

    [Fact]
    public void GetRelevantTime_Should_Return_ArrivalTime_For_ARR_Route()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());
        var route = new Route
        {
            RouteType = "ARR",
            ArrivalTime = new TimeOnly(14, 30),
            DepartureTime = new TimeOnly(10, 0)
        };

        Assert.Equal("14:30", routeService.GetRelevantTime(route));
    }

    [Fact]
    public void GetRelevantTime_Should_Return_DepartureTime_For_DEP_Route()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());
        var route = new Route
        {
            RouteType = "DEP",
            ArrivalTime = new TimeOnly(14, 30),
            DepartureTime = new TimeOnly(10, 0)
        };

        Assert.Equal("10:00", routeService.GetRelevantTime(route));
    }

    [Fact]
    public void AddWithInitialFlight_Should_Succeed_When_No_Conflicts()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockCompanyRepo = new Mock<ICompanyRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        mockFlightRepo.Setup(getFlights => getFlights.GetAllFlights()).Returns(new List<Flight>());
        mockCompanyRepo.Setup(getCompany => getCompany.GetCompanyById(It.IsAny<int>())).Returns(new Company());
        mockAirportRepo.Setup(getAirport => getAirport.GetAirportById(It.IsAny<int>())).Returns(new Airport());
        mockRouteRepo.Setup(addRoute => addRoute.AddRoute(It.IsAny<Route>())).Returns(10);

        var routeService = new RouteService(mockRouteRepo.Object, mockFlightRepo.Object,
            mockCompanyRepo.Object, mockAirportRepo.Object);

        var startDate = new DateTime(2025, 1, 1);
        var result = routeService.AddWithInitialFlight(1, 1, "DEP", 1, startDate, startDate,
            new TimeOnly(10, 0), new TimeOnly(12, 0), 100, "FL001", 1, 1);

        Assert.Equal(10, result);
        mockRouteRepo.Verify(addRoute => addRoute.AddRoute(It.IsAny<Route>()), Times.Once);
        mockFlightRepo.Verify(addFlight => addFlight.AddFlight(It.IsAny<Flight>()), Times.Once);
    }

    [Fact]
    public void AddWithInitialFlight_Should_Not_Conflict_When_Times_Do_Not_Overlap()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockCompanyRepo = new Mock<ICompanyRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        var startDate = new DateTime(2025, 1, 1);
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

        mockFlightRepo.Setup(getFlights => getFlights.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(getRoute => getRoute.GetRouteById(5)).Returns(existingRoute);
        mockCompanyRepo.Setup(getCompany => getCompany.GetCompanyById(It.IsAny<int>())).Returns(new Company());
        mockAirportRepo.Setup(getAirport => getAirport.GetAirportById(It.IsAny<int>())).Returns(new Airport());
        mockRouteRepo.Setup(addRoute => addRoute.AddRoute(It.IsAny<Route>())).Returns(11);

        var routeService = new RouteService(mockRouteRepo.Object, mockFlightRepo.Object,
            mockCompanyRepo.Object, mockAirportRepo.Object);

        var result = routeService.AddWithInitialFlight(1, 1, "DEP", 1, startDate, startDate,
            new TimeOnly(10, 0), new TimeOnly(12, 0), 100, "FL002", 1, 1);

        Assert.Equal(11, result);
    }

    [Fact]
    public void AddWithInitialFlight_Should_Throw_When_Gate_Conflict()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var date = new DateTime(2025, 1, 1);

        var existingFlight = new Flight { Date = date, Gate = new Gate { Id = 10 }, Runway = new Runway { Id = 20 }, Route = new Route { Id = 5 }, FlightNumber = "EX123" };
        var existingRoute = new Route { DepartureTime = new TimeOnly(10, 0), ArrivalTime = new TimeOnly(12, 0) };

        mockFlightRepo.Setup(getFlights => getFlights.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(getRoute => getRoute.GetRouteById(5)).Returns(existingRoute);

        var routeService = BuildService(mockRouteRepo, mockFlightRepo);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            routeService.AddWithInitialFlight(1, 1, "DEP", 0, date, date, new TimeOnly(11, 0), new TimeOnly(13, 0), 100, "NEW123", 99, 10));

        Assert.Contains("Conflict", ex.Message);
    }

    [Fact]
    public void AddWithInitialFlight_Should_Throw_When_Runway_Conflict()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var date = new DateTime(2025, 1, 1);

        var existingFlight = new Flight { Date = date, Gate = new Gate { Id = 10 }, Runway = new Runway { Id = 20 }, Route = new Route { Id = 5 }, FlightNumber = "EX123" };
        var existingRoute = new Route { DepartureTime = new TimeOnly(10, 0), ArrivalTime = new TimeOnly(12, 0) };

        mockFlightRepo.Setup(getFlights => getFlights.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(getRoute => getRoute.GetRouteById(5)).Returns(existingRoute);

        var routeService = BuildService(mockRouteRepo, mockFlightRepo);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            routeService.AddWithInitialFlight(1, 1, "DEP", 0, date, date, new TimeOnly(11, 0), new TimeOnly(13, 0), 100, "NEW123", 20, 99));

        Assert.Contains("Conflict", ex.Message);
    }

    [Fact]
    public void AddWithInitialFlight_Should_Handle_Midnight_Wrap_Overlaps_When_Exist()
    {
        var date = new DateTime(2025, 1, 1);
        var existingRoute = new Route { DepartureTime = new TimeOnly(23, 30), ArrivalTime = new TimeOnly(1, 30) };
        {
            var mockRouteRepo = new Mock<IRouteRepository>();
            var mockFlightRepo = new Mock<IFlightRepository>();
            var existingFlight = new Flight { Date = date, Gate = new Gate { Id = 1 }, Runway = new Runway { Id = 1 }, Route = new Route { Id = 5 } };
            mockFlightRepo.Setup(getFlight => getFlight.GetAllFlights()).Returns(new List<Flight> { existingFlight });
            mockRouteRepo.Setup(getRoute => getRoute.GetRouteById(5)).Returns(existingRoute);
            var routeService = BuildService(mockRouteRepo, mockFlightRepo);

            Assert.Throws<InvalidOperationException>(() =>
                routeService.AddWithInitialFlight(1, 1, "DEP", 0, date, date, new TimeOnly(23, 0), new TimeOnly(1, 0), 100, "WRAP", 1, 1));
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

        mockFlightRepo.Setup(getFlight => getFlight.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(getRoute => getRoute.GetRouteById(999)).Returns((Route)null);
        mockRouteRepo.Setup(addRoute => addRoute.AddRoute(It.IsAny<Route>())).Returns(1);

        var routeService = BuildService(mockRouteRepo, mockFlightRepo);

        int resultId = routeService.AddWithInitialFlight(1, 1, "DEP", 0, date, date, new TimeOnly(10, 0), new TimeOnly(12, 0), 100, "VALID", 1, 1);

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

        mockFlightRepo.Setup(getFlight => getFlight.GetAllFlights()).Returns(new List<Flight> { otherDayFlight });
        mockCompanyRepo.Setup(getCompany => getCompany.GetCompanyById(It.IsAny<int>())).Returns(new Company());
        mockAirportRepo.Setup(getAirport => getAirport.GetAirportById(It.IsAny<int>())).Returns(new Airport());
        mockRouteRepo.Setup(addRoute => addRoute.AddRoute(It.IsAny<Route>())).Returns(5);

        var routeService = new RouteService(mockRouteRepo.Object, mockFlightRepo.Object,
            mockCompanyRepo.Object, mockAirportRepo.Object);

        var result = routeService.AddWithInitialFlight(1, 1, "DEP", 0, targetDate, targetDate,
            new TimeOnly(10, 0), new TimeOnly(12, 0), 100, "FL999", 1, 1);

        Assert.Equal(5, result);
        mockRouteRepo.Verify(r => r.GetRouteById(It.IsAny<int>()), Times.Never);
    }
}