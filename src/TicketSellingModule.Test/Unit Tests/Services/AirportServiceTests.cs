using Moq;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class AirportServiceTests
{
    private const int TargetAirportId = 1;
    private const int InvalidAirportId = 0;
    private const string DefaultTestCode = "DTC";
    private const string DefaultTestName = "DefaultTestName";
    private const string DefaultTestCity = "DefaultTestCity";
    private const string SecondTestCode = "STC";
    private const string SecondTestName = "SecondTestName";
    private const string SecondTestCity = "SecondTestCity";
    private const string UpdatedName = "New Name";
    private const string UpdatedCity = "New City";
    private const string UpdatedCode = "NEW";

    [Fact]
    public void AddAirport_ThrowsArgumentException_WhenCodeIsNull()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();
        var airportService = new AirportService(airportDataSource.Object, flightDataSource.Object);

        Assert.Throws<ArgumentException>(() => airportService.AddAirport(null, DefaultTestName, DefaultTestCity));
    }

    [Fact]
    public void AddAirport_ThrowsArgumentException_WhenCodeIsEmpty()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();
        var airportService = new AirportService(airportDataSource.Object, flightDataSource.Object);

        Assert.Throws<ArgumentException>(() => airportService.AddAirport(string.Empty, DefaultTestName, DefaultTestCity));
    }

    [Fact]
    public void AddAirport_ThrowsArgumentException_WhenCodeIsWhitespace()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();
        var airportService = new AirportService(airportDataSource.Object, flightDataSource.Object);

        Assert.Throws<ArgumentException>(() => airportService.AddAirport(" ", DefaultTestName, DefaultTestCity));
    }

    [Fact]
    public void AddAirport_ThrowsArgumentException_WhenNameIsNull()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();
        var airportService = new AirportService(airportDataSource.Object, flightDataSource.Object);

        Assert.Throws<ArgumentException>(() => airportService.AddAirport(DefaultTestCode, null, DefaultTestCity));
    }

    [Fact]
    public void AddAirport_ThrowsArgumentException_WhenNameIsEmpty()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();
        var airportService = new AirportService(airportDataSource.Object, flightDataSource.Object);

        Assert.Throws<ArgumentException>(() => airportService.AddAirport(DefaultTestCode, string.Empty, DefaultTestCity));
    }

    [Fact]
    public void AddAirport_ThrowsArgumentException_WhenNameIsWhiteSpace()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();
        var airportService = new AirportService(airportDataSource.Object, flightDataSource.Object);

        Assert.Throws<ArgumentException>(() => airportService.AddAirport(DefaultTestCode, " ", DefaultTestCity));
    }

    [Fact]
    public void AddAirport_ThrowsArgumentException_WhenCityIsNull()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();
        var airportService = new AirportService(airportDataSource.Object, flightDataSource.Object);

        Assert.Throws<ArgumentException>(() => airportService.AddAirport(DefaultTestCode, DefaultTestName, null));
    }

    [Fact]
    public void AddAirport_ThrowsArgumentException_WhenCityIsEmpty()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();
        var airportService = new AirportService(airportDataSource.Object, flightDataSource.Object);

        Assert.Throws<ArgumentException>(() => airportService.AddAirport(DefaultTestCode, DefaultTestName, string.Empty));
    }

    [Fact]
    public void AddAirport_ThrowsArgumentException_WhenCityIsWhitespace()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();
        var airportService = new AirportService(airportDataSource.Object, flightDataSource.Object);

        Assert.Throws<ArgumentException>(() => airportService.AddAirport(DefaultTestCode, DefaultTestName, " "));
    }

    [Fact]
    public void AddAirport_ReturnsGeneratedId_WhenArgumentsAreValid()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();

        airportDataSource.Setup(addValidAirport => addValidAirport.AddAirport(It.IsAny<Airport>()))
            .Returns(TargetAirportId);

        var airportService = new AirportService(airportDataSource.Object, flightDataSource.Object);

        int resultId = airportService.AddAirport(DefaultTestCode, DefaultTestName, DefaultTestCity);

        Assert.Equal(TargetAirportId, resultId);
    }

    [Fact]
    public void GetAllAirports_ReturnsAllRecords_WhenRecordsExist()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();

        var existingAirportsList = new List<Airport>
        {
            new Airport { AirportCode = DefaultTestCode, AirportName = DefaultTestName, City = DefaultTestCity },
            new Airport { AirportCode = SecondTestCode, AirportName = SecondTestName, City = SecondTestCity }
        };

        airportDataSource.Setup(getAllAirportsFromRepository => getAllAirportsFromRepository.GetAllAirports())
            .Returns(existingAirportsList);

        var airportService = new AirportService(airportDataSource.Object, flightDataSource.Object);

        List<Airport> resultList = airportService.GetAllAirports();

        Assert.Equal(2, resultList.Count);
        Assert.Equal(existingAirportsList, resultList);
    }

    [Fact]
    public void GetAirportById_ReturnsNull_WhenIdIsInvalid()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();
        var airportService = new AirportService(airportDataSource.Object, flightDataSource.Object);

        Airport? result = airportService.GetAirportById(InvalidAirportId);

        Assert.Null(result);
    }

    [Fact]
    public void GetAirportById_ReturnsAirportObject_WhenIdIsValid()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();
        var existingAirport = new Airport { Id = TargetAirportId, AirportCode = DefaultTestCode };

        airportDataSource.Setup(getAirportCorrespondingToId => getAirportCorrespondingToId.GetAirportById(TargetAirportId))
            .Returns(existingAirport);

        var airportService = new AirportService(airportDataSource.Object, flightDataSource.Object);

        Airport? result = airportService.GetAirportById(TargetAirportId);

        Assert.Equal(existingAirport, result);
    }

    [Fact]
    public void UpdateAirport_DoesNotCallRepository_WhenAirportNotFound()
    {
        var airportDataSourceThatReturnsNull = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();

        airportDataSourceThatReturnsNull.Setup(noAriportsWithProvidedId => noAriportsWithProvidedId.GetAirportById(TargetAirportId))
            .Returns((Airport?)null);

        var airportService = new AirportService(airportDataSourceThatReturnsNull.Object, flightDataSource.Object);

        airportService.UpdateAirport(TargetAirportId, DefaultTestCity);

        airportDataSourceThatReturnsNull.Verify(airportDataSource => airportDataSource.UpdateAirport(It.IsAny<Airport>()), Times.Never);
    }

    [Fact]
    public void UpdateAirport_UpdatesOnlyProvidedName_WhenOtherFieldsAreNull()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();
        var airportToUpdate = new Airport { AirportCode = DefaultTestCode, AirportName = DefaultTestName, City = DefaultTestCity };

        airportDataSource.Setup(getAirportToUpdate => getAirportToUpdate.GetAirportById(TargetAirportId))
            .Returns(airportToUpdate);

        var airportService = new AirportService(airportDataSource.Object, flightDataSource.Object);

        airportService.UpdateAirport(TargetAirportId, newName: UpdatedName);

        Assert.Equal(DefaultTestCode, airportToUpdate.AirportCode);
        Assert.Equal(UpdatedName, airportToUpdate.AirportName);
        Assert.Equal(DefaultTestCity, airportToUpdate.City);

        airportDataSource.Verify(ifChangesWereSentToRepository => ifChangesWereSentToRepository.UpdateAirport(airportToUpdate), Times.Once);
    }

    [Fact]
    public void UpdateAirport_UpdatesOnlyProvidedCity_WhenOtherFieldsAreNull()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();
        var airportToUpdate = new Airport { AirportCode = DefaultTestCode, AirportName = DefaultTestName, City = DefaultTestCity };

        airportDataSource.Setup(getAirportToUpdate => getAirportToUpdate.GetAirportById(TargetAirportId))
            .Returns(airportToUpdate);

        var airportService = new AirportService(airportDataSource.Object, flightDataSource.Object);

        airportService.UpdateAirport(TargetAirportId, newCity: UpdatedCity);

        Assert.Equal(DefaultTestCode, airportToUpdate.AirportCode);
        Assert.Equal(DefaultTestName, airportToUpdate.AirportName);
        Assert.Equal(UpdatedCity, airportToUpdate.City);

        airportDataSource.Verify(ifChangesWereSentToRepository => ifChangesWereSentToRepository.UpdateAirport(airportToUpdate), Times.Once);
    }

    [Fact]
    public void UpdateAirport_UpdatesOnlyProvidedCode_WhenOtherFieldsAreNull()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();
        var airportToUpdate = new Airport { AirportCode = DefaultTestCode, AirportName = DefaultTestName, City = DefaultTestCity };

        airportDataSource.Setup(getAirportToUpdate => getAirportToUpdate.GetAirportById(TargetAirportId))
            .Returns(airportToUpdate);

        var airportService = new AirportService(airportDataSource.Object, flightDataSource.Object);

        airportService.UpdateAirport(TargetAirportId, newCode: UpdatedCode);

        Assert.Equal(UpdatedCode, airportToUpdate.AirportCode);
        Assert.Equal(DefaultTestName, airportToUpdate.AirportName);
        Assert.Equal(DefaultTestCity, airportToUpdate.City);

        airportDataSource.Verify(ifChangesWereSentToRepository => ifChangesWereSentToRepository.UpdateAirport(airportToUpdate), Times.Once);
    }

    [Fact]
    public void UpdateAirport_ShouldCallRepo_EvenWhenNoChanges()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();
        var airportToUpdate = new Airport { AirportCode = DefaultTestCode, AirportName = DefaultTestName, City = DefaultTestCity };

        airportDataSource.Setup(getAirportToUpdate => getAirportToUpdate.GetAirportById(TargetAirportId))
            .Returns(airportToUpdate);

        var airportService = new AirportService(airportDataSource.Object, flightDataSource.Object);

        Assert.Equal(DefaultTestCode, airportToUpdate.AirportCode);
        Assert.Equal(DefaultTestName, airportToUpdate.AirportName);
        Assert.Equal(DefaultTestCity, airportToUpdate.City);

        airportDataSource.Verify(ifChangesWereSentToRepository => ifChangesWereSentToRepository.UpdateAirport(airportToUpdate), Times.Once);
    }

    [Fact]
    public void UpdateAirport_UpdatesAllFields_WhenAllFieldsAreProvided()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();
        var airportToUpdate = new Airport { AirportCode = DefaultTestCode, AirportName = DefaultTestName, City = DefaultTestCity };

        airportDataSource.Setup(getAirportToUpdate => getAirportToUpdate.GetAirportById(TargetAirportId))
            .Returns(airportToUpdate);

        var airportService = new AirportService(airportDataSource.Object, flightDataSource.Object);

        airportService.UpdateAirport(TargetAirportId, newCode: UpdatedCode, newName: UpdatedName, newCity: UpdatedCity);

        Assert.Equal(UpdatedCode, airportToUpdate.AirportCode);
        Assert.Equal(UpdatedName, airportToUpdate.AirportName);
        Assert.Equal(UpdatedCity, airportToUpdate.City);

        airportDataSource.Verify(ifChangesWereSentToRepository => ifChangesWereSentToRepository.UpdateAirport(airportToUpdate), Times.Once);
    }

    [Fact]
    public void DeleteAirport_DoesNotCallRepository_WhenIdIsInvalid()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();
        var airportService = new AirportService(airportDataSource.Object, flightDataSource.Object);

        airportService.DeleteAirportUsingId(InvalidAirportId);

        airportDataSource.Verify(doesNotCallRepository => doesNotCallRepository.DeleteAirportUsingId(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void DeleteAirport_CallsRepositoryDelete_WhenIdIsValid()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSource = new Mock<IFlightRepository>();
        var airportService = new AirportService(airportDataSource.Object, flightDataSource.Object);

        airportService.DeleteAirportUsingId(TargetAirportId);

        airportDataSource.Verify(doesCallRepository => doesCallRepository.DeleteAirportUsingId(TargetAirportId), Times.Once);
    }

    [Fact]
    public void HasFlights_ReturnsTrue_WhenAssociatedFlightsExist()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSourceWithFlights = new Mock<IFlightRepository>();
        var associatedFlightsList = new List<Flight> { new Flight() };

        flightDataSourceWithFlights.Setup(getFlights => getFlights.GetFlightsByAirportId(TargetAirportId))
            .Returns(associatedFlightsList);

        var airportService = new AirportService(airportDataSource.Object, flightDataSourceWithFlights.Object);

        bool result = airportService.HasFlights(TargetAirportId);

        Assert.True(result);
    }

    [Fact]
    public void HasFlights_ReturnsFalse_WhenNoAssociatedFlightsFound()
    {
        var airportDataSource = new Mock<IAirportRepository>();
        var flightDataSourceWithNoFlights = new Mock<IFlightRepository>();

        flightDataSourceWithNoFlights.Setup(getNoFlights => getNoFlights.GetFlightsByAirportId(TargetAirportId))
            .Returns(new List<Flight>());

        var airportService = new AirportService(airportDataSource.Object, flightDataSourceWithNoFlights.Object);

        bool result = airportService.HasFlights(TargetAirportId);

        Assert.False(result);
    }
}
