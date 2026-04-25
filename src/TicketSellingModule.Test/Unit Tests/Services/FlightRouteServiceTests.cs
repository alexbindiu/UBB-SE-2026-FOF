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
        mockFlightRepo.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(new List<Flight>());

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Throws<ArgumentException>(() =>
            flightRouteService.AddFlightToRoute(1, 1, "DEP", 1,
                new DateTime(2025, 6, 10), new DateTime(2025, 6, 1),
                new TimeOnly(10, 0), new TimeOnly(12, 0), 100, "FL001", 1, 1));
    }

    [Fact]
    public void Add_Should_Throw_For_Zero_Capacity()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        mockFlightRepo.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(new List<Flight>());
        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Throws<ArgumentException>(() =>
            flightRouteService.AddFlightToRoute(1, 1, "DEP", 1,
                new DateTime(2025, 1, 1), new DateTime(2025, 1, 10),
                new TimeOnly(10, 0), new TimeOnly(12, 0), 0, "FL001", 1, 1));
    }

    [Fact]
    public void AddFlightToRoute_Should_Throw_For_Invalid_Capacity()
    {
        var flightRouteService = BuildService(new Mock<IFlightRepository>(), new Mock<IRouteRepository>());

        var ex = Assert.Throws<ArgumentException>(() =>
            flightRouteService.AddFlightToRoute(1, 1, "DEP", 0, DateTime.Today, DateTime.Today.AddDays(1),
                new TimeOnly(10, 0), new TimeOnly(12, 0), 0, "FL123", 1, 1));

        Assert.Equal("Capacity must be a positive number greater than 0.", ex.Message);
    }

    [Fact]
    public void Add_Should_Throw_On_Gate_Conflict()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();

        var startDate = new DateTime(2025, 1, 1);
        var existingFlight = new Flight
        {
            Gate = new Gate { Id = 1 },
            Runway = new Runway { Id = 99 },
            Route = new Route { Id = 5 },
            FlightNumber = "EX001",
            Date = startDate
        };
        var existingRoute = new Route
        {
            DepartureTime = new TimeOnly(10, 30),
            ArrivalTime = new TimeOnly(11, 30)
        };

        mockFlightRepo.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(getRoute => getRoute.GetRouteById(5)).Returns(existingRoute);

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Throws<InvalidOperationException>(() =>
            flightRouteService.AddFlightToRoute(1, 1, "DEP", 1, startDate, startDate,
                new TimeOnly(10, 0), new TimeOnly(12, 0), 100, "FL001", 2, 1));
    }

    [Fact]
    public void Add_Should_Throw_On_Runway_Conflict()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();

        var startDate = new DateTime(2025, 1, 1);
        var existingFlight = new Flight
        {
            Gate = new Gate { Id = 99 },
            Runway = new Runway { Id = 2 },
            Route = new Route { Id = 5 },
            FlightNumber = "EX001",
            Date = startDate
        };
        var existingRoute = new Route
        {
            DepartureTime = new TimeOnly(10, 30),
            ArrivalTime = new TimeOnly(11, 30)
        };

        mockFlightRepo.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(getRoute => getRoute.GetRouteById(5)).Returns(existingRoute);

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Throws<InvalidOperationException>(() =>
            flightRouteService.AddFlightToRoute(1, 1, "DEP", 1, startDate, startDate,
                new TimeOnly(10, 0), new TimeOnly(12, 0), 100, "FL001", 2, 1));
    }

    [Fact]
    public void AddFlightToRoute_Should_Throw_For_Gate_And_Runway_Conflicts()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();

        var existingFlight = new Flight
        {
            Date = DateTime.Today,
            FlightNumber = "EXISTING",
            Gate = new Gate { Id = 10 },
            Runway = new Runway { Id = 20 },
            Route = new Route { Id = 500 }
        };
        var existingRoute = new Route
        {
            Id = 500,
            DepartureTime = new TimeOnly(10, 0),
            ArrivalTime = new TimeOnly(12, 0)
        };

        mockFlightRepo.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(getRoute => getRoute.GetRouteById(500)).Returns(existingRoute);

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Throws<InvalidOperationException>(() =>
            flightRouteService.AddFlightToRoute(1, 1, "DEP", 0, DateTime.Today, DateTime.Today,
                new TimeOnly(11, 0), new TimeOnly(13, 0), 100, "NEW", 99, 10));

        Assert.Throws<InvalidOperationException>(() =>
            flightRouteService.AddFlightToRoute(1, 1, "DEP", 0, DateTime.Today, DateTime.Today,
                new TimeOnly(11, 0), new TimeOnly(13, 0), 100, "NEW", 20, 99));
    }

    [Fact]
    public void Add_Should_Succeed_With_No_Conflicts_When_Valid_Data()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockCompanyRepo = new Mock<ICompanyRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        mockFlightRepo.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(new List<Flight>());
        mockCompanyRepo.Setup(getCompany => getCompany.GetCompanyById(It.IsAny<int>())).Returns(new Company());
        mockAirportRepo.Setup(getAirport => getAirport.GetAirportById(It.IsAny<int>())).Returns(new Airport());
        mockRouteRepo.Setup(addRoute => addRoute.AddRoute(It.IsAny<Route>())).Returns(20);

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo, mockCompanyRepo, mockAirportRepo);

        var start = new DateTime(2025, 1, 1);
        var result = flightRouteService.AddFlightToRoute(1, 1, "DEP", 1, start, start,
            new TimeOnly(10, 0), new TimeOnly(12, 0), 100, "FL001", 1, 1);

        Assert.Equal(20, result);
        mockRouteRepo.Verify(addRoute => addRoute.AddRoute(It.IsAny<Route>()), Times.Once);
        mockFlightRepo.Verify(addFlight => addFlight.AddFlight(It.IsAny<Flight>()), Times.Once);
    }

    [Fact]
    public void AddFlightToRoute_Should_Skip_Flight_When_Is_On_Different_Date()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockCompanyRepo = new Mock<ICompanyRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        var targetDate = new DateTime(2025, 1, 1);

        var otherDayFlight = new Flight
        {
            Date = new DateTime(2025, 2, 1),
            Gate = new Gate { Id = 1 },
            Runway = new Runway { Id = 1 },
            Route = new Route { Id = 5 },
            FlightNumber = "OTHER"
        };

        mockFlightRepo.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(new List<Flight> { otherDayFlight });
        mockCompanyRepo.Setup(getCompany => getCompany.GetCompanyById(It.IsAny<int>())).Returns(new Company());
        mockAirportRepo.Setup(getAirport => getAirport.GetAirportById(It.IsAny<int>())).Returns(new Airport());
        mockRouteRepo.Setup(addRoute => addRoute.AddRoute(It.IsAny<Route>())).Returns(7);

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo, mockCompanyRepo, mockAirportRepo);

        var result = flightRouteService.AddFlightToRoute(1, 1, "DEP", 0, targetDate, targetDate,
            new TimeOnly(10, 0), new TimeOnly(12, 0), 100, "FL999", 1, 1);

        Assert.Equal(7, result);
        mockRouteRepo.Verify(getRoute => getRoute.GetRouteById(5), Times.Never);
    }

    [Fact]
    public void AddFlightToRoute_Should_Skip_Flight_When_Existing_Route_Is_Null()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockCompanyRepo = new Mock<ICompanyRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();

        var date = new DateTime(2025, 1, 1);

        var existingFlight = new Flight
        {
            Date = date,
            Gate = new Gate { Id = 1 },
            Runway = new Runway { Id = 1 },
            Route = new Route { Id = 999 },
            FlightNumber = "NULLROUTE"
        };

        mockFlightRepo.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(getRoute => getRoute.GetRouteById(999)).Returns((Route)null);
        mockCompanyRepo.Setup(getCompany => getCompany.GetCompanyById(It.IsAny<int>())).Returns(new Company());
        mockAirportRepo.Setup(getAirport => getAirport.GetAirportById(It.IsAny<int>())).Returns(new Airport());
        mockRouteRepo.Setup(addRoute => addRoute.AddRoute(It.IsAny<Route>())).Returns(8);
        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo, mockCompanyRepo, mockAirportRepo);

        var result = flightRouteService.AddFlightToRoute(1, 1, "DEP", 0, date, date,
            new TimeOnly(10, 0), new TimeOnly(12, 0), 100, "VALID", 1, 1);

        Assert.Equal(8, result);
    }

    [Fact]
    public void GetRouteById_Should_Return_Route_When_Valid_Data()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var route = new Route { RouteType = "ARR" };
        mockRouteRepo.Setup(getRoute => getRoute.GetRouteById(1)).Returns(route);

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Equal(route, flightRouteService.GetRouteById(1));
    }

    [Fact]
    public void GetFlightById_Should_Return_Flight_When_Valid_Data()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var flight = new Flight { FlightNumber = "FL001" };
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(1)).Returns(flight);

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Equal(flight, flightRouteService.GetFlightById(1));
    }

    [Fact]
    public void GetAllRoutes_Should_Return_All_Routes_Always()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var routes = new List<Route> { new Route(), new Route() };
        mockRouteRepo.Setup(getRoute => getRoute.GetAllRoutes()).Returns(routes);

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Equal(2, flightRouteService.GetAllRoutes().Count);
    }

    [Fact]
    public void GetAllFlights_Should_Return_All_Flights_Always()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var flights = new List<Flight> { new Flight(), new Flight() };
        mockFlightRepo.Setup(getFlight => getFlight.GetAllFlights()).Returns(flights);

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Equal(2, flightRouteService.GetAllFlights().Count);
    }

    [Fact]
    public void DeleteFlight_Should_Throw_For_Invalid_Id()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Throws<ArgumentException>(() => flightRouteService.DeleteFlightUsingId(-1));
    }

    [Fact]
    public void DeleteFlight_Should_Throw_When_Flight_Not_Found()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        mockFlightRepo.Setup(getNoFlight => getNoFlight.GetFlightById(5)).Returns((Flight)null);

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Throws<ArgumentException>(() => flightRouteService.DeleteFlightUsingId(5));
    }

    [Fact]
    public void DeleteFlight_Should_Call_Repo_For_Valid_Id()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        mockFlightRepo.Setup(getFlight => getFlight.GetFlightById(5)).Returns(new Flight());

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo);
        flightRouteService.DeleteFlightUsingId(5);

        mockFlightRepo.Verify(deleteFlight => deleteFlight.DeleteFlightUsingId(5), Times.Once);
    }

    [Fact]
    public void GetFlightsByCompany_Should_Return_Flights_For_Company_When_Valid_Data()
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

        mockRouteRepo.Setup(getRoutes => getRoutes.GetAllRoutes()).Returns(routes);
        mockFlightRepo.Setup(getFlights => getFlights.GetAllFlights()).Returns(flights);

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo);
        var result = flightRouteService.GetFlightsByCompanyId(10);

        Assert.Single(result);
        Assert.Equal("FL001", result[0].FlightNumber);
    }

    [Fact]
    public void GetFlightsByCompany_Should_Return_Empty_When_No_Matching_Routes()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();

        mockRouteRepo.Setup(getRoutes => getRoutes.GetAllRoutes()).Returns(new List<Route>());
        mockFlightRepo.Setup(getFlights => getFlights.GetAllFlights()).Returns(new List<Flight> { new Flight { Route = new Route { Id = 1 } } });

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo);
        var result = flightRouteService.GetFlightsByCompanyId(10);
        Assert.Empty(result);
    }

    [Fact]
    public void GetDestinationText_Should_Return_Dash_When_Route_Is_Null()
    {
        var flightRouteService = BuildService(new Mock<IFlightRepository>(), new Mock<IRouteRepository>());
        Assert.Equal("-", flightRouteService.GetDestinationText(new Flight { Route = null }));
    }

    [Fact]
    public void GetDestinationText_Should_Return_Dash_When_Airport_Is_Null()
    {
        var flightRouteService = BuildService(new Mock<IFlightRepository>(), new Mock<IRouteRepository>());
        Assert.Equal("-", flightRouteService.GetDestinationText(new Flight { Route = new Route { Airport = null } }));
    }

    [Fact]
    public void GetDestinationText_Should_Return_Formatted_String()
    {
        var flightRouteService = BuildService(new Mock<IFlightRepository>(), new Mock<IRouteRepository>());
        var flight = new Flight
        {
            Route = new Route
            {
                Airport = new Airport { AirportCode = "JFK", AirportName = "John F. Kennedy" }
            }
        };

        Assert.Equal("JFK - John F. Kennedy", flightRouteService.GetDestinationText(flight));
    }

    [Fact]
    public void GetAllFlightsWithDetails_Should_Skip_Null_Or_Invalid_Ids_Always()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var flight = new Flight
        {
            Runway = new Runway { Id = 0 },
            Gate = null,
            Route = new Route { Id = 1, Airport = new Airport { Id = 0 } }
        };

        mockFlightRepo.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(new List<Flight> { flight });
        var flightRouteService = BuildService(mockFlightRepo, new Mock<IRouteRepository>());

        var result = flightRouteService.GetAllFlightsWithDetails();

        Assert.Single(result);
    }

    [Fact]
    public void GetAllFlightsWithDetails_Should_Hydrate_Runway_Gate_Route_And_Airport()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockRunwayService = new Mock<IRunwayService>();
        var mockGateService = new Mock<IGateService>();
        var mockAirportService = new Mock<IAirportService>();

        var airport = new Airport { Id = 5, AirportCode = "LHR", AirportName = "Heathrow" };
        var route = new Route { Id = 10, Airport = new Airport { Id = 5 } };
        var runway = new Runway { Id = 2, Name = "Runway 2" };
        var gate = new Gate { Id = 3, Name = "Gate 3" };

        var flight = new Flight
        {
            Runway = new Runway { Id = 2 },
            Gate = new Gate { Id = 3 },
            Route = new Route { Id = 10 }
        };

        mockFlightRepo.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(new List<Flight> { flight });
        mockRouteRepo.Setup(getRouteById => getRouteById.GetRouteById(10)).Returns(route);
        mockRunwayService.Setup(getRunway => getRunway.GetRunwayById(2)).Returns(runway);
        mockGateService.Setup(getGate => getGate.GetGateById(3)).Returns(gate);
        mockAirportService.Setup(getAirport => getAirport.GetAirportById(5)).Returns(airport);

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo,
            runwayService: mockRunwayService, gateService: mockGateService,
            airportService: mockAirportService);

        var result = flightRouteService.GetAllFlightsWithDetails();

        Assert.Single(result);
        Assert.Equal("Runway 2", result[0].Runway.Name);
        Assert.Equal("Gate 3", result[0].Gate.Name);
        Assert.Equal("LHR", result[0].Route.Airport.AirportCode);
    }

    [Fact]
    public void CheckOverlappingTimes_Should_Handle_Midnight_Wrap_When_It_Exists()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();

        var existingFlight = new Flight
        {
            Date = DateTime.Today,
            Route = new Route { Id = 1 },
            Gate = new Gate { Id = 1 },
            Runway = new Runway { Id = 99 }
        };

        mockFlightRepo.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(getRouteById => getRouteById.GetRouteById(1)).Returns(
            new Route { DepartureTime = new TimeOnly(23, 0), ArrivalTime = new TimeOnly(1, 0) });

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo);
        Assert.Throws<InvalidOperationException>(() =>
            flightRouteService.AddFlightToRoute(1, 1, "DEP", 0, DateTime.Today, DateTime.Today,
                new TimeOnly(22, 30), new TimeOnly(0, 30), 100, "FN", 999, 1));
    }

    [Fact]
    public void CreateFlightWithSchedule_Should_Throw_When_RecurrentEndBeforeStart()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        mockFlightRepo.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(new List<Flight>());

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo);

        Assert.Throws<InvalidOperationException>(() =>
            flightRouteService.CreateFlightWithSchedule(1, "DEP", 1, 100,
                TimeSpan.FromHours(10), TimeSpan.FromHours(12),
                true, new DateTime(2025, 6, 10), new DateTime(2025, 6, 1),
                null, "Daily", null, 1, 1, _ => "FL001"));
    }

    [Fact]
    public void CreateFlightWithSchedule_Should_Throw_For_Equal_Dep_And_Arr_Times()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        mockFlightRepo.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(new List<Flight>());

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo);
        var date = new DateTime(2025, 6, 10);

        Assert.Throws<InvalidOperationException>(() =>
            flightRouteService.CreateFlightWithSchedule(1, "DEP", 1, 100,
                TimeSpan.FromHours(10), TimeSpan.FromHours(10),
                false, null, null, date, "Daily", null, 1, 1, _ => "FL001"));
    }

    [Fact]
    public void CreateFlightWithSchedule_Should_Throw_For_Invalid_Recurrence_Type()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        mockFlightRepo.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(new List<Flight>());

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo);
        var date = new DateTime(2025, 6, 10);

        Assert.Throws<InvalidOperationException>(() =>
            flightRouteService.CreateFlightWithSchedule(1, "DEP", 1, 100,
                TimeSpan.FromHours(10), TimeSpan.FromHours(12),
                true, date, date, null, "Biweekly", null, 1, 1, _ => "FL001"));
    }

    [Fact]
    public void CreateFlightWithSchedule_Should_Throw_For_Invalid_Custom_Interval()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        mockFlightRepo.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(new List<Flight>());

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo);
        var date = new DateTime(2025, 6, 10);

        Assert.Throws<InvalidOperationException>(() =>
            flightRouteService.CreateFlightWithSchedule(1, "DEP", 1, 100,
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

        mockFlightRepo.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(new List<Flight>());
        mockCompanyRepo.Setup(getCompany => getCompany.GetCompanyById(It.IsAny<int>())).Returns(new Company());
        mockAirportRepo.Setup(getAirport => getAirport.GetAirportById(It.IsAny<int>())).Returns(new Airport());
        mockRouteRepo.Setup(addRoute => addRoute.AddRoute(It.IsAny<Route>())).Returns(1);

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo, mockCompanyRepo, mockAirportRepo);
        var date = new DateTime(2025, 6, 10);

        flightRouteService.CreateFlightWithSchedule(1, "DEP", 1, 100,
            TimeSpan.FromHours(10), TimeSpan.FromHours(12),
            false, null, null, date, string.Empty, null, 1, 1, _ => "FL001");

        mockRouteRepo.Verify(r => r.AddRoute(It.IsAny<Route>()), Times.Once);
        mockFlightRepo.Verify(r => r.AddFlight(It.IsAny<Flight>()), Times.Once);
    }

    [Fact]
    public void CreateFlightWithSchedule_Should_Succeed_For_Daily_Recurrence()
    {
        var (flightRepo, routeRepo, companyRepo, airportRepo) = SetupSuccessRepos();
        var flightRouteService = BuildService(flightRepo, routeRepo, companyRepo, airportRepo);
        var date = new DateTime(2025, 6, 10);

        flightRouteService.CreateFlightWithSchedule(1, "DEP", 1, 100,
            TimeSpan.FromHours(8), TimeSpan.FromHours(10),
            true, date, date.AddDays(7), null, "Daily", null, 1, 1, _ => "FL-D");

        routeRepo.Verify(addRoute => addRoute.AddRoute(It.IsAny<Route>()), Times.Once);
    }

    [Fact]
    public void CreateFlightWithSchedule_Should_Succeed_For_Weekly_Recurrence()
    {
        var (flightRepo, routeRepo, companyRepo, airportRepo) = SetupSuccessRepos();
        var flightRouteService = BuildService(flightRepo, routeRepo, companyRepo, airportRepo);
        var date = new DateTime(2025, 6, 10);

        flightRouteService.CreateFlightWithSchedule(1, "DEP", 1, 100,
            TimeSpan.FromHours(8), TimeSpan.FromHours(10),
            true, date, date.AddDays(30), null, "Weekly", null, 1, 1, _ => "FL-W");

        routeRepo.Verify(addRoute => addRoute.AddRoute(It.IsAny<Route>()), Times.Once);
    }

    [Fact]
    public void CreateFlightWithSchedule_Should_Succeed_For_Monthly_Recurrence()
    {
        var (flightRepo, routeRepo, companyRepo, airportRepo) = SetupSuccessRepos();
        var flightRouteService = BuildService(flightRepo, routeRepo, companyRepo, airportRepo);
        var date = new DateTime(2025, 6, 10);

        flightRouteService.CreateFlightWithSchedule(1, "DEP", 1, 100,
            TimeSpan.FromHours(8), TimeSpan.FromHours(10),
            true, date, date.AddMonths(3), null, "Monthly", null, 1, 1, _ => "FL-M");

        routeRepo.Verify(addRoute => addRoute.AddRoute(It.IsAny<Route>()), Times.Once);
    }

    [Fact]
    public void CreateFlightWithSchedule_Should_Succeed_For_Custom_Recurrence()
    {
        var (flightRepo, routeRepo, companyRepo, airportRepo) = SetupSuccessRepos();
        var flightRouteService = BuildService(flightRepo, routeRepo, companyRepo, airportRepo);
        var date = new DateTime(2025, 6, 10);

        flightRouteService.CreateFlightWithSchedule(1, "DEP", 1, 100,
            TimeSpan.FromHours(8), TimeSpan.FromHours(10),
            true, date, date.AddDays(14), null, "Custom", "5", 1, 1, _ => "FL-C");

        routeRepo.Verify(addRoute => addRoute.AddRoute(It.IsAny<Route>()), Times.Once);
    }

    [Fact]
    public void CreateFlightWithSchedule_Should_Throw_For_Custom_Interval_Zero()
    {
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        mockFlightRepo.Setup(r => r.GetAllFlights()).Returns(new List<Flight>());

        var flightRouteService = BuildService(mockFlightRepo, mockRouteRepo);
        var date = new DateTime(2025, 6, 10);

        Assert.Throws<InvalidOperationException>(() =>
            flightRouteService.CreateFlightWithSchedule(1, "DEP", 1, 100,
                TimeSpan.FromHours(10), TimeSpan.FromHours(12),
                true, date, date, null, "Custom", "0", 1, 1, _ => "FL001"));
    }

    private static (Mock<IFlightRepository> flightRepo,
                    Mock<IRouteRepository> routeRepo,
                    Mock<ICompanyRepository> companyRepo,
                    Mock<IAirportRepository> airportRepo) SetupSuccessRepos()
    {
        var flightRepo = new Mock<IFlightRepository>();
        var routeRepo = new Mock<IRouteRepository>();
        var companyRepo = new Mock<ICompanyRepository>();
        var airportRepo = new Mock<IAirportRepository>();

        flightRepo.Setup(getAllFlights => getAllFlights.GetAllFlights()).Returns(new List<Flight>());
        companyRepo.Setup(getCompany => getCompany.GetCompanyById(It.IsAny<int>())).Returns(new Company());
        airportRepo.Setup(getAirport => getAirport.GetAirportById(It.IsAny<int>())).Returns(new Airport());
        routeRepo.Setup(addRoute => addRoute.AddRoute(It.IsAny<Route>())).Returns(1);

        return (flightRepo, routeRepo, companyRepo, airportRepo);
    }
}