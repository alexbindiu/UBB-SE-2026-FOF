using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using Moq;

using TicketSellingModule.Data.Repositories.Interfaces;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class RouteServiceTests
{
    private int validRouteId = 1;
    private int invalidRouteId = 99;
    private int negativeRouteId = -1;
    private string depRouteType = "DEP";
    private string arrRouteType = "ARR";
    private int numberOfRoutes = 2;
    private string dash = "-";
    private string unknownRouteType = "transit";
    private TimeOnly arrivalTime = new TimeOnly(14, 30);
    private TimeOnly departureTime = new TimeOnly(10, 00);
    private string arrivalTimeString = "14:30";
    private string departureTimeString = "10:00";
    private int validGateId = 1;
    private int validRunwayId = 1;

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
        var route = new Route { RouteType = depRouteType };
        mockRouteRepo.Setup(getRoute => getRoute.GetRouteById(validRouteId)).Returns(route);

        var routeService = BuildService(mockRouteRepo, mockFlightRepo);

        Assert.Equal(route, routeService.GetRouteById(validRouteId));
    }

    [Fact]
    public void GetById_Should_Return_Null_When_Route_Not_Found()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        mockRouteRepo.Setup(getNoRoute => getNoRoute.GetRouteById(invalidRouteId)).Returns((Route)null);
        var routeService = BuildService(mockRouteRepo, mockFlightRepo);

        Assert.Null(routeService.GetRouteById(invalidRouteId));
    }

    [Fact]
    public void GetAll_Should_Return_All_Routes_Always()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var routes = new List<Route> { new Route(), new Route() };
        mockRouteRepo.Setup(getAllRoutes => getAllRoutes.GetAllRoutes()).Returns(routes);

        var routeService = BuildService(mockRouteRepo, mockFlightRepo);

        Assert.Equal(numberOfRoutes, routeService.GetAllRoutes().Count);
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_Dash_For_Null()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal(dash, routeService.NormalizeFlightType(null));
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_Dash_For_Empty_String()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal(dash, routeService.NormalizeFlightType(string.Empty));
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_Dash_For_Whitespace()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal(dash, routeService.NormalizeFlightType("  "));
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_ARR_For_Arrival_Variants()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal(arrRouteType, routeService.NormalizeFlightType("arr"));
        Assert.Equal(arrRouteType, routeService.NormalizeFlightType("ARR"));
        Assert.Equal(arrRouteType, routeService.NormalizeFlightType("arrival"));
        Assert.Equal(arrRouteType, routeService.NormalizeFlightType("ARRIVAL"));
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_DEP_For_Departure_Variants()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal(depRouteType, routeService.NormalizeFlightType("dep"));
        Assert.Equal(depRouteType, routeService.NormalizeFlightType("DEP"));
        Assert.Equal(depRouteType, routeService.NormalizeFlightType("departure"));
        Assert.Equal(depRouteType, routeService.NormalizeFlightType("DEPARTURE"));
    }

    [Fact]
    public void NormalizeFlightType_Should_Return_Uppercased_Value_For_Unknown_Type()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal(unknownRouteType.ToUpper(), routeService.NormalizeFlightType(unknownRouteType));
    }

    [Fact]
    public void GetRelevantTime_Should_Return_Dash_For_Null_Route()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());

        Assert.Equal(dash, routeService.GetRelevantTime(null));
    }

    [Fact]
    public void GetRelevantTime_Should_Return_ArrivalTime_For_ARR_Route()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());
        var route = new Route
        {
            RouteType = arrRouteType,
            ArrivalTime = arrivalTime,
            DepartureTime = departureTime
        };

        Assert.Equal(arrivalTimeString, routeService.GetRelevantTime(route));
    }

    [Fact]
    public void GetRelevantTime_Should_Return_DepartureTime_For_DEP_Route()
    {
        var routeService = BuildService(new Mock<IRouteRepository>(), new Mock<IFlightRepository>());
        var route = new Route
        {
            RouteType = depRouteType,
            ArrivalTime = arrivalTime,
            DepartureTime = departureTime
        };

        Assert.Equal(departureTimeString, routeService.GetRelevantTime(route));
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
        mockRouteRepo.Setup(addRoute => addRoute.AddRoute(It.IsAny<Route>())).Returns(validRouteId);

        var routeService = new RouteService(mockRouteRepo.Object, mockFlightRepo.Object,
            mockCompanyRepo.Object, mockAirportRepo.Object);

        var startDate = new DateTime(2025, 1, 1);
        int capacity = 100;
        int companyId = 1;
        int airportId = 1;
        int runwayId = 1;
        int gateId = 1;
        int recurrenceInterval = 1;
        string flightNumber = "FL001";

        var result = routeService.AddWithInitialFlight(companyId, airportId, depRouteType, recurrenceInterval, startDate, startDate,
            departureTime, arrivalTime, capacity, flightNumber, runwayId, gateId);

        Assert.Equal(validRouteId, result);
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
            Gate = new Gate { Id = validGateId },
            Runway = new Runway { Id = validRunwayId },
            Route = new Route { Id = validRouteId }
        };
        var existingRoute = new Route
        {
            DepartureTime = departureTime,
            ArrivalTime = arrivalTime
        };

        mockFlightRepo.Setup(getFlights => getFlights.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(getRoute => getRoute.GetRouteById(5)).Returns(existingRoute);
        mockCompanyRepo.Setup(getCompany => getCompany.GetCompanyById(It.IsAny<int>())).Returns(new Company());
        mockAirportRepo.Setup(getAirport => getAirport.GetAirportById(It.IsAny<int>())).Returns(new Airport());
        mockRouteRepo.Setup(addRoute => addRoute.AddRoute(It.IsAny<Route>())).Returns(validRouteId);

        var routeService = new RouteService(mockRouteRepo.Object, mockFlightRepo.Object,
            mockCompanyRepo.Object, mockAirportRepo.Object);

        int capacity = 100;
        int companyId = 1;
        int airportId = 1;
        int runwayId = 1;
        int gateId = 1;
        int recurrenceInterval = 1;
        string flightNumber = "FL001";

        var result = routeService.AddWithInitialFlight(companyId, airportId, depRouteType, recurrenceInterval, startDate, startDate,
            departureTime, arrivalTime, capacity, flightNumber, runwayId, gateId);

        Assert.Equal(validRouteId, result);
    }

    [Fact]
    public void AddWithInitialFlight_Should_Handle_Midnight_Wrap_Overlaps_When_Exist()
    {
        var date = new DateTime(2025, 1, 1);
        var existingRoute = new Route { DepartureTime = new TimeOnly(23, 30), ArrivalTime = new TimeOnly(1, 30) };
        {
            var mockRouteRepo = new Mock<IRouteRepository>();
            var mockFlightRepo = new Mock<IFlightRepository>();
            var existingFlight = new Flight { Date = date, Gate = new Gate { Id = validGateId }, Runway = new Runway { Id = validRunwayId }, Route = new Route { Id = validRouteId } };
            mockFlightRepo.Setup(getFlight => getFlight.GetAllFlights()).Returns(new List<Flight> { existingFlight });
            mockRouteRepo.Setup(getRoute => getRoute.GetRouteById(validRouteId)).Returns(existingRoute);
            var routeService = BuildService(mockRouteRepo, mockFlightRepo);

            var companyId = 1;
            var airportId = 1;
            var recurrenceInterval = 0;
            var departureTime = new TimeOnly(23, 0);
            var arrivalTime = new TimeOnly(1, 0);
            var capacity = 100;
            var flightName = "WRAP";
            var gateId = 1;
            var routeId = 1;

            Assert.Throws<InvalidOperationException>(() =>
                routeService.AddWithInitialFlight(companyId, airportId, "DEP", recurrenceInterval, date, date, departureTime, arrivalTime, capacity, flightName, routeId, gateId));
        }

        {
            var mockRouteRepo = new Mock<IRouteRepository>();
            var mockFlightRepo = new Mock<IFlightRepository>();
            var existingFlight = new Flight { Date = date, Gate = new Gate { Id = validGateId }, Runway = new Runway { Id = validRunwayId }, Route = new Route { Id = validRouteId } };
            mockFlightRepo.Setup(getFlight => getFlight.GetAllFlights()).Returns(new List<Flight> { existingFlight });
            mockRouteRepo.Setup(getRoute => getRoute.GetRouteById(validRouteId)).Returns(existingRoute);
            var service = BuildService(mockRouteRepo, mockFlightRepo);

            var companyId = 1;
            var airportId = 1;
            var recurrenceInterval = 0;
            var departureTime = new TimeOnly(23, 0);
            var arrivalTime = new TimeOnly(1, 0);
            var capacity = 100;
            var flightName = "WRAP2";
            var gateId = 1;
            var routeId = 1;

            Assert.Throws<InvalidOperationException>(() =>
                service.AddWithInitialFlight(companyId, airportId, "DEP", recurrenceInterval, date, date, departureTime, arrivalTime, capacity, flightName, routeId, gateId));
        }
    }

    [Fact]
    public void AddWithInitialFlight_Should_Continue_If_Existing_Route_Is_Null()
    {
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockFlightRepo = new Mock<IFlightRepository>();
        var date = new DateTime(2025, 1, 1);

        var existingFlight = new Flight { Date = date, Gate = new Gate { Id = validGateId }, Runway = new Runway { Id = validRunwayId }, Route = new Route { Id = validRouteId } };

        mockFlightRepo.Setup(getFlight => getFlight.GetAllFlights()).Returns(new List<Flight> { existingFlight });
        mockRouteRepo.Setup(getRoute => getRoute.GetRouteById(invalidRouteId)).Returns((Route)null);
        mockRouteRepo.Setup(addRoute => addRoute.AddRoute(It.IsAny<Route>())).Returns(validRouteId);

        var routeService = BuildService(mockRouteRepo, mockFlightRepo);

        var companyId = 1;
        var airportId = 1;
        var recurrenceInterval = 0;
        var departureTime = new TimeOnly(10, 0);
        var arrivalTime = new TimeOnly(12, 0);
        var capacity = 100;
        var flightName = "VALID";
        var gateId = 1;
        var routeId = 1;

        int resultId = routeService.AddWithInitialFlight(companyId, airportId, depRouteType, recurrenceInterval, date, date, departureTime, arrivalTime, capacity, flightName, gateId, routeId);

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
            Gate = new Gate { Id = validGateId },
            Runway = new Runway { Id = validRunwayId },
            Route = new Route { Id = validRouteId }
        };

        mockFlightRepo.Setup(getFlight => getFlight.GetAllFlights()).Returns(new List<Flight> { otherDayFlight });
        mockCompanyRepo.Setup(getCompany => getCompany.GetCompanyById(It.IsAny<int>())).Returns(new Company());
        mockAirportRepo.Setup(getAirport => getAirport.GetAirportById(It.IsAny<int>())).Returns(new Airport());
        mockRouteRepo.Setup(addRoute => addRoute.AddRoute(It.IsAny<Route>())).Returns(validRouteId);

        var routeService = new RouteService(mockRouteRepo.Object, mockFlightRepo.Object,
            mockCompanyRepo.Object, mockAirportRepo.Object);

        var companyId = 1;
        var airportId = 1;
        var recurrenceInterval = 0;
        var departureTime = new TimeOnly(10, 0);
        var arrivalTime = new TimeOnly(12, 0);
        var capacity = 100;
        var flightName = "FL999";
        var gateId = 1;
        var routeId = 1;

        var result = routeService.AddWithInitialFlight(companyId, airportId, depRouteType, recurrenceInterval, targetDate, targetDate,
            departureTime, arrivalTime, capacity, flightName, gateId, routeId);

        Assert.Equal(validRouteId, result);
        mockRouteRepo.Verify(r => r.GetRouteById(It.IsAny<int>()), Times.Never);
    }
}