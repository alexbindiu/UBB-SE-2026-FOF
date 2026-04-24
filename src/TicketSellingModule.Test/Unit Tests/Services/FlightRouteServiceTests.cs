using System;
using System.Collections.Generic;
using System.Text;

using Moq;

using TicketSellingModule.Data.Repositories.Interfaces;
using TicketSellingModule.Data.Services.Interfaces;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class FlightRouteServiceTests
{
    private static FlightRouteService BuildService(
        Mock<IFlightRepository> flightRepo,
        Mock<IRouteRepository> routeRepo,
        Mock<ICompanyRepository> companyRepo = null,
        Mock<IAirportRepository> airportRepo = null,
        Mock<IRunwayService> runwayService = null,
        Mock<IGateService> gateService = null,
        Mock<IAirportService> airportService = null)
    {
        return new FlightRouteService(
            flightRepo.Object,
            routeRepo.Object,
            (companyRepo ?? new Mock<ICompanyRepository>()).Object,
            (airportRepo ?? new Mock<IAirportRepository>()).Object,
            (runwayService ?? new Mock<IRunwayService>()).Object,
            (gateService ?? new Mock<IGateService>()).Object,
            (airportService ?? new Mock<IAirportService>()).Object);
    }

    [Fact]
    public void Add_Should_Throw_When_StartDate_After_EndDate()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight>());

        var service = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Throws<ArgumentException>(() =>
            service.Add(1, 1, "DEP", 1,
                new DateTime(2025, 6, 10), new DateTime(2025, 6, 1),
                new TimeOnly(10, 0), new TimeOnly(12, 0), 100, "FL001", 1, 1));
    }

    [Fact]
    public void Add_Should_Throw_For_Zero_Capacity()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight>());
        var service = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Throws<ArgumentException>(() =>
            service.Add(1, 1, "DEP", 1,
                new DateTime(2025, 1, 1), new DateTime(2025, 1, 10),
                new TimeOnly(10, 0), new TimeOnly(12, 0), 0, "FL001", 1, 1));
    }

    [Fact]
    public void Add_Should_Throw_On_Gate_Conflict()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();

        var start = new DateTime(2025, 1, 1);
        var existingFlight = new Flight
        {
            Gate = new Gate { Id = 1 },
            Runway = new Runway { Id = 99 },
            Route = new Route { Id = 5 },
            FlightNumber = "EX001",
            Date = start
        };
        var existingRoute = new Route
        {
            DepartureTime = new TimeOnly(10, 30),
            ArrivalTime = new TimeOnly(11, 30)
        };

        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(r => r.GetRouteById(5)).Returns(existingRoute);

        var service = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Throws<InvalidOperationException>(() =>
            service.Add(1, 1, "DEP", 1, start, start,
                new TimeOnly(10, 0), new TimeOnly(12, 0), 100, "FL001", 2, 1));
    }

    [Fact]
    public void Add_Should_Throw_On_Runway_Conflict()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();

        var start = new DateTime(2025, 1, 1);
        var existingFlight = new Flight
        {
            Gate = new Gate { Id = 99 },
            Runway = new Runway { Id = 2 },
            Route = new Route { Id = 5 },
            FlightNumber = "EX001",
            Date = start
        };
        var existingRoute = new Route
        {
            DepartureTime = new TimeOnly(10, 30),
            ArrivalTime = new TimeOnly(11, 30)
        };

        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(r => r.GetRouteById(5)).Returns(existingRoute);

        var service = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Throws<InvalidOperationException>(() =>
            service.Add(1, 1, "DEP", 1, start, start,
                new TimeOnly(10, 0), new TimeOnly(12, 0), 100, "FL001", 2, 1));
    }

    [Fact]
    public void Add_Should_Succeed_With_No_Conflicts()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockCompanyRepo = new Mock<ICompanyRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight>());
        mockCompanyRepo.Setup(r => r.GetCompanyById(It.IsAny<int>())).Returns(new Company());
        mockAirportRepo.Setup(r => r.GetAirportById(It.IsAny<int>())).Returns(new Airport());
        mockRouteRepo.Setup(r => r.AddRoute(It.IsAny<Route>())).Returns(20);

        var service = BuildService(mockFlightRepo, mockRouteRepo, mockCompanyRepo, mockAirportRepo);

        var start = new DateTime(2025, 1, 1);
        var result = service.Add(1, 1, "DEP", 1, start, start,
            new TimeOnly(10, 0), new TimeOnly(12, 0), 100, "FL001", 1, 1);

        Assert.Equal(20, result);
        mockRouteRepo.Verify(r => r.AddRoute(It.IsAny<Route>()), Times.Once);
        mockFlightRepo.Verify(r => r.AddFlight(It.IsAny<Flight>()), Times.Once);
    }

    [Fact]
    public void GetRouteById_Should_Return_Route()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var route = new Route { RouteType = "ARR" };
        mockRouteRepo.Setup(r => r.GetRouteById(1)).Returns(route);

        var service = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Equal(route, service.GetRouteById(1));
    }

    [Fact]
    public void GetFlightById_Should_Return_Flight()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var flight = new Flight { FlightNumber = "FL001" };
        mockFlightRepo.Setup(r => r.GetById(1)).Returns(flight);

        var service = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Equal(flight, service.GetFlightById(1));
    }

    // ── GetAllRoutes / GetAllFlights ─────────────────────────
    [Fact]
    public void GetAllRoutes_Should_Return_All_Routes()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var routes = new List<Route> { new Route(), new Route() };
        mockRouteRepo.Setup(r => r.GetAllRoutes()).Returns(routes);

        var service = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Equal(2, service.GetAllRoutes().Count);
    }

    [Fact]
    public void GetAllFlights_Should_Return_All_Flights()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var flights = new List<Flight> { new Flight(), new Flight() };
        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(flights);

        var service = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Equal(2, service.GetAllFlights().Count);
    }

    [Fact]
    public void DeleteFlight_Should_Throw_For_Invalid_Id()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var service = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Throws<ArgumentException>(() => service.DeleteFlight(0));
        Assert.Throws<ArgumentException>(() => service.DeleteFlight(-1));
    }

    [Fact]
    public void DeleteFlight_Should_Throw_When_Flight_Not_Found()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        mockFlightRepo.Setup(r => r.GetById(5)).Returns((Flight)null);

        var service = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Throws<ArgumentException>(() => service.DeleteFlight(5));
    }

    [Fact]
    public void DeleteFlight_Should_Call_Repo_For_Valid_Id()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        mockFlightRepo.Setup(r => r.GetById(5)).Returns(new Flight());

        var service = BuildService(mockFlightRepo, mockRouteRepo);
        service.DeleteFlight(5);

        mockFlightRepo.Verify(r => r.DeleteFlightUsingId(5), Times.Once);
    }

    [Fact]
    public void GetFlightsByCompany_Should_Return_Flights_For_Company()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();

        var routes = new List<Route>
        {
            new Route { Id = 1, Company = new Company { Id = 10 } },
            new Route { Id = 2, Company = new Company { Id = 99 } }
        };
        var flights = new List<Flight>
        {
            new Flight { FlightNumber = "FL001", Route = new Route { Id = 1 } },
            new Flight { FlightNumber = "FL002", Route = new Route { Id = 2 } }
        };

        mockRouteRepo.Setup(r => r.GetAllRoutes()).Returns(routes);
        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(flights);

        var service = BuildService(mockFlightRepo, mockRouteRepo);
        var result = service.GetFlightsByCompany(10);

        Assert.Single(result);
        Assert.Equal("FL001", result[0].FlightNumber);
    }

    [Fact]
    public void GetFlightsByCompany_Should_Return_Empty_When_No_Matching_Routes()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();

        mockRouteRepo.Setup(r => r.GetAllRoutes()).Returns(new List<Route>());
        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight> { new Flight { Route = new Route { Id = 1 } } });

        var service = BuildService(mockFlightRepo, mockRouteRepo);
        var result = service.GetFlightsByCompany(10);

        Assert.Empty(result);
    }

    [Fact]
    public void GetDestinationText_Should_Return_Dash_When_Route_Is_Null()
    {
        var service = BuildService(new Mock<IFlightRepository>(), new Mock<IRouteRepository>());
        var flight = new Flight { Route = null };

        Assert.Equal("-", service.GetDestinationText(flight));
    }

    [Fact]
    public void GetDestinationText_Should_Return_Dash_When_Airport_Is_Null()
    {
        var service = BuildService(new Mock<IFlightRepository>(), new Mock<IRouteRepository>());
        var flight = new Flight { Route = new Route { Airport = null } };

        Assert.Equal("-", service.GetDestinationText(flight));
    }

    [Fact]
    public void GetDestinationText_Should_Return_Formatted_String()
    {
        var service = BuildService(new Mock<IFlightRepository>(), new Mock<IRouteRepository>());
        var flight = new Flight
        {
            Route = new Route
            {
                Airport = new Airport { AirportCode = "JFK", AirportName = "John F. Kennedy" }
            }
        };

        Assert.Equal("JFK - John F. Kennedy", service.GetDestinationText(flight));
    }

    [Fact]
    public void CreateFlightWithSchedule_Should_Throw_When_RecurrentEndBeforeStart()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight>());

        var service = BuildService(mockFlightRepo, mockRouteRepo);
        var start = new DateTime(2025, 6, 10);
        var end = new DateTime(2025, 6, 1);

        Assert.Throws<InvalidOperationException>(() =>
            service.CreateFlightWithSchedule(1, "DEP", 1, 100,
                TimeSpan.FromHours(10), TimeSpan.FromHours(12),
                true, start, end, null, "Daily", null, 1, 1, _ => "FL001"));
    }

    [Fact]
    public void CreateFlightWithSchedule_Should_Throw_For_Equal_Dep_And_Arr_Times()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight>());

        var service = BuildService(mockFlightRepo, mockRouteRepo);
        var date = new DateTime(2025, 6, 10);

        Assert.Throws<InvalidOperationException>(() =>
            service.CreateFlightWithSchedule(1, "DEP", 1, 100,
                TimeSpan.FromHours(10), TimeSpan.FromHours(10),
                false, null, null, date, "Daily", null, 1, 1, _ => "FL001"));
    }

    [Fact]
    public void CreateFlightWithSchedule_Should_Throw_For_Invalid_Recurrence_Type()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight>());

        var service = BuildService(mockFlightRepo, mockRouteRepo);
        var date = new DateTime(2025, 6, 10);

        Assert.Throws<InvalidOperationException>(() =>
            service.CreateFlightWithSchedule(1, "DEP", 1, 100,
                TimeSpan.FromHours(10), TimeSpan.FromHours(12),
                true, date, date, null, "Biweekly", null, 1, 1, _ => "FL001"));
    }

    [Fact]
    public void CreateFlightWithSchedule_Should_Throw_For_Invalid_Custom_Interval()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight>());

        var service = BuildService(mockFlightRepo, mockRouteRepo);
        var date = new DateTime(2025, 6, 10);

        Assert.Throws<InvalidOperationException>(() =>
            service.CreateFlightWithSchedule(1, "DEP", 1, 100,
                TimeSpan.FromHours(10), TimeSpan.FromHours(12),
                true, date, date, null, "Custom", "abc", 1, 1, _ => "FL001"));
    }

    [Fact]
    public void CreateFlightWithSchedule_Should_Succeed_For_Non_Recurrent_Flight()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockCompanyRepo = new Mock<ICompanyRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight>());
        mockCompanyRepo.Setup(r => r.GetCompanyById(It.IsAny<int>())).Returns(new Company());
        mockAirportRepo.Setup(r => r.GetAirportById(It.IsAny<int>())).Returns(new Airport());
        mockRouteRepo.Setup(r => r.AddRoute(It.IsAny<Route>())).Returns(1);

        var service = BuildService(mockFlightRepo, mockRouteRepo, mockCompanyRepo, mockAirportRepo);
        var date = new DateTime(2025, 6, 10);

        service.CreateFlightWithSchedule(1, "DEP", 1, 100,
            TimeSpan.FromHours(10), TimeSpan.FromHours(12),
            false, null, null, date, string.Empty, null, 1, 1, _ => "FL001");

        mockRouteRepo.Verify(r => r.AddRoute(It.IsAny<Route>()), Times.Once);
        mockFlightRepo.Verify(r => r.AddFlight(It.IsAny<Flight>()), Times.Once);
    }
}
