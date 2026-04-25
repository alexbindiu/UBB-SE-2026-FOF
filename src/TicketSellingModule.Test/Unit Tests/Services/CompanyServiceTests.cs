using Moq;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class CompanyServiceTests
{
    private const int TargetCompanyId = 1;
    private const int InvalidCompanyId = 0;
    private const string DefaultCompanyTwoWordsName = "Test Company";
    private const string DefaultCompanyTwoWordsPrefix = "TC-";
    private const string DefaultCompanyOneWordName = "TestCompany;";
    private const string DefaultCompanyOneWordPrefix = "TE-";
    private const string UpdatedCompanyName = "NewName";
    private const string NoCompanyPrefix = "FL-";
    private const string StartingSequence = "-1000";
    private const string TestFlightNumber1000 = "FL-1000";
    private const string TestFlightNumber1005 = "FL-1005";
    private const string TestFlightNumber1006Suffix = "-1006";
    private const string FlightNumberInvalid = "INVALID";
    private const string FlightNumberwithBadSuffix = "TC-TEST";
    private const string SingleCharCompanyName = "X";
    private const string SingleCharCompanyCode = "X-";

    [Fact]
    public void AddCompany_ThrowsArgumentException_WhenNameIsNull()
    {
        var companyRepository = new Mock<ICompanyRepository>();
        var flightRouteRepository = new Mock<IFlightRouteService>();
        var companyService = new CompanyService(companyRepository.Object, flightRouteRepository.Object);

        Assert.Throws<ArgumentException>(() => companyService.AddCompany(null!));
    }

    [Fact]
    public void AddCompany_ThrowsArgumentException_WhenNameIsEmpty()
    {
        var companyRepository = new Mock<ICompanyRepository>();
        var flightRouteRepository = new Mock<IFlightRouteService>();
        var companyService = new CompanyService(companyRepository.Object, flightRouteRepository.Object);

        Assert.Throws<ArgumentException>(() => companyService.AddCompany(string.Empty));
    }

    [Fact]
    public void AddCompany_ThrowsArgumentException_WhenNameIsWhitespace()
    {
        var companyRepository = new Mock<ICompanyRepository>();
        var flightRouteRepository = new Mock<IFlightRouteService>();
        var companyService = new CompanyService(companyRepository.Object, flightRouteRepository.Object);

        Assert.Throws<ArgumentException>(() => companyService.AddCompany("   "));
    }

    [Fact]
    public void AddCompany_ReturnsGeneratedId_WhenNameIsValid()
    {
        var companyRepository = new Mock<ICompanyRepository>();
        var flightRouteRepository = new Mock<IFlightRouteService>();

        companyRepository
            .Setup(companySource => companySource.AddCompany(It.IsAny<Company>()))
            .Returns(TargetCompanyId);

        var companyService = new CompanyService(companyRepository.Object, flightRouteRepository.Object);

        int generatedId = companyService.AddCompany(DefaultCompanyTwoWordsName);

        Assert.Equal(TargetCompanyId, generatedId);
    }

    [Fact]
    public void GetAllCompanies_ReturnsAllRecords_WhenRecordsExist()
    {
        var companyRepository = new Mock<ICompanyRepository>();
        var flightRouteRepository = new Mock<IFlightRouteService>();

        var existingCompanies = new List<Company> { new Company(), new Company() };

        companyRepository.Setup(getAllCompanies => getAllCompanies.GetAllCompanies()).Returns(existingCompanies);

        var companyService = new CompanyService(companyRepository.Object, flightRouteRepository.Object);

        List<Company> resultList = companyService.GetAllCompanies();

        Assert.Equal(2, resultList.Count);
    }

    [Fact]
    public void GetCompanyById_ReturnsNull_WhenIdIsInvalid()
    {
        var companyRepository = new Mock<ICompanyRepository>();
        var flightRouteRepository = new Mock<IFlightRouteService>();
        var companyService = new CompanyService(companyRepository.Object, flightRouteRepository.Object);

        Company? retrievedCompany = companyService.GetCompanyById(InvalidCompanyId);

        Assert.Null(retrievedCompany);
    }

    [Fact]
    public void GetCompanyById_ReturnsCompanyObject_WhenIdIsValid()
    {
        var companyRepository = new Mock<ICompanyRepository>();
        var flightRouteRepository = new Mock<IFlightRouteService>();

        var existingCompany = new Company { Name = DefaultCompanyTwoWordsName };

        companyRepository.Setup(getTheCompanyCorrespondingToId => getTheCompanyCorrespondingToId.GetCompanyById(TargetCompanyId)).Returns(existingCompany);

        var companyService = new CompanyService(companyRepository.Object, flightRouteRepository.Object);

        Company? retrievedCompany = companyService.GetCompanyById(TargetCompanyId);

        Assert.Equal(existingCompany, retrievedCompany);
    }

    [Fact]
    public void GenerateFlightCodeUsingCompanyId_ReturnsDefaultPrefix_WhenCompanyIsNotFound()
    {
        var companyRepositoryThatReturnsNull = new Mock<ICompanyRepository>();
        var flightRouteRepositoryThatReturnsNoFlights = new Mock<IFlightRouteService>();

        companyRepositoryThatReturnsNull
            .Setup(getNoCompany => getNoCompany.GetCompanyById(TargetCompanyId))
            .Returns((Company?)null);

        flightRouteRepositoryThatReturnsNoFlights
            .Setup(getNoFlight => getNoFlight.GetFlightsByCompanyId(TargetCompanyId))
            .Returns(new List<Flight>());

        var companyService = new CompanyService(companyRepositoryThatReturnsNull.Object, flightRouteRepositoryThatReturnsNoFlights.Object);

        string generatedCode = companyService.GenerateFlightCodeUsingCompanyId(TargetCompanyId);

        Assert.StartsWith(NoCompanyPrefix, generatedCode);
    }

    [Fact]
    public void GenerateFlightCodeUsingCompanyId_ReturnsInitialsPrefix_WhenNameHasTwoWords()
    {
        var companyRepository = new Mock<ICompanyRepository>();
        var flightRouteRepository = new Mock<IFlightRouteService>();

        companyRepository.Setup(getCompanyTwoWordsName => getCompanyTwoWordsName.GetCompanyById(TargetCompanyId))
            .Returns(new Company { Name = DefaultCompanyTwoWordsName });

        flightRouteRepository.Setup(getFlight => getFlight.GetFlightsByCompanyId(TargetCompanyId))
            .Returns(new List<Flight>());

        var companyService = new CompanyService(companyRepository.Object, flightRouteRepository.Object);

        string generatedCode = companyService.GenerateFlightCodeUsingCompanyId(TargetCompanyId);

        Assert.StartsWith(DefaultCompanyTwoWordsPrefix, generatedCode);
    }

    [Fact]
    public void GenerateFlightCodeUsingCompanyId_ReturnsFirstTwoLetters_WhenNameHasOneWord()
    {
        var companyRepository = new Mock<ICompanyRepository>();
        var flightRouteRepository = new Mock<IFlightRouteService>();

        companyRepository.Setup(getCompanyOneWordName => getCompanyOneWordName.GetCompanyById(TargetCompanyId))
            .Returns(new Company { Name = DefaultCompanyOneWordName });

        flightRouteRepository.Setup(getFlight => getFlight.GetFlightsByCompanyId(TargetCompanyId))
            .Returns(new List<Flight>());

        var companyService = new CompanyService(companyRepository.Object, flightRouteRepository.Object);

        string generatedCode = companyService.GenerateFlightCodeUsingCompanyId(TargetCompanyId);

        Assert.StartsWith(DefaultCompanyOneWordPrefix, generatedCode);
    }

    [Fact]
    public void GenerateFlightCodeUsingCompanyId_ReturnsStartingSequence_WhenNoFlightsExist()
    {
        var companyRepository = new Mock<ICompanyRepository>();
        var flightRouteRepositoryThatReturnsNoFlights = new Mock<IFlightRouteService>();

        companyRepository.Setup(getDefaultCompany => getDefaultCompany.GetCompanyById(TargetCompanyId))
            .Returns(new Company { Name = DefaultCompanyTwoWordsName });

        flightRouteRepositoryThatReturnsNoFlights.Setup(getNoFlight => getNoFlight.GetFlightsByCompanyId(TargetCompanyId))
            .Returns(new List<Flight>());

        var companyService = new CompanyService(companyRepository.Object, flightRouteRepositoryThatReturnsNoFlights.Object);

        string generatedCode = companyService.GenerateFlightCodeUsingCompanyId(TargetCompanyId);

        Assert.EndsWith(StartingSequence, generatedCode);
    }

    [Fact]
    public void GenerateFlightCodeUsingCompanyId_ReturnsIncrementedSequence_WhenFlightsExist()
    {
        var companyRepository = new Mock<ICompanyRepository>();
        var flightRouteRepositoryWithFlights = new Mock<IFlightRouteService>();

        companyRepository.Setup(getDefaultCompany => getDefaultCompany.GetCompanyById(TargetCompanyId))
            .Returns(new Company { Name = DefaultCompanyTwoWordsName });

        var existingFlightsList = new List<Flight>
        {
            new Flight { FlightNumber = TestFlightNumber1000 },
            new Flight { FlightNumber = TestFlightNumber1005 }
        };

        flightRouteRepositoryWithFlights.Setup(getFlightsOfCompany => getFlightsOfCompany.GetFlightsByCompanyId(TargetCompanyId))
            .Returns(existingFlightsList);

        var companyService = new CompanyService(companyRepository.Object, flightRouteRepositoryWithFlights.Object);

        string generatedCode = companyService.GenerateFlightCodeUsingCompanyId(TargetCompanyId);

        Assert.EndsWith(TestFlightNumber1006Suffix, generatedCode);
    }

    [Fact]
    public void GenerateFlightCodeUsingCompanyId_ReturnsStartingSequence_WhenExistingFlightNumbersAreInvalid()
    {
        var companyRepository = new Mock<ICompanyRepository>();
        var flightRouteRepositoryWithInvalidFlights = new Mock<IFlightRouteService>();

        companyRepository.Setup(getDefaultCompany => getDefaultCompany.GetCompanyById(TargetCompanyId))
            .Returns(new Company { Name = DefaultCompanyOneWordName });

        var invalidFlightsList = new List<Flight>
        {
            new Flight { FlightNumber = FlightNumberInvalid },
            new Flight { FlightNumber = FlightNumberwithBadSuffix }
        };

        flightRouteRepositoryWithInvalidFlights.Setup(getInvalidFlights => getInvalidFlights.GetFlightsByCompanyId(TargetCompanyId))
            .Returns(invalidFlightsList);

        var companyService = new CompanyService(companyRepository.Object, flightRouteRepositoryWithInvalidFlights.Object);

        string generatedCode = companyService.GenerateFlightCodeUsingCompanyId(TargetCompanyId);

        Assert.EndsWith(StartingSequence, generatedCode);
    }

    [Fact]
    public void GenerateFlightCodeUsingCompanyId_ReturnsStartingSequence_WhenFlightListIsNull()
    {
        var companyRepository = new Mock<ICompanyRepository>();
        var flightRouteRepositoryThatReturnsNull = new Mock<IFlightRouteService>();

        companyRepository.Setup(getDefaultCompany => getDefaultCompany.GetCompanyById(TargetCompanyId))
            .Returns(new Company { Name = DefaultCompanyTwoWordsName });

        flightRouteRepositoryThatReturnsNull.Setup(getNullList => getNullList.GetFlightsByCompanyId(TargetCompanyId))
            .Returns((List<Flight>?)null);

        var companyService = new CompanyService(companyRepository.Object, flightRouteRepositoryThatReturnsNull.Object);

        string generatedCode = companyService.GenerateFlightCodeUsingCompanyId(TargetCompanyId);

        Assert.EndsWith(StartingSequence, generatedCode);
    }

    [Fact]
    public void GenerateFlightCodeUsingCompanyId_ReturnsStartingSequence_WhenExistingFlightsHaveNullIsteadOfNumbers()
    {
        var companyRepository = new Mock<ICompanyRepository>();
        var flightRouteRepositoryWithNullNumbers = new Mock<IFlightRouteService>();

        companyRepository.Setup(getDefaultCompany => getDefaultCompany.GetCompanyById(TargetCompanyId))
            .Returns(new Company { Name = DefaultCompanyTwoWordsName });

        var flightsWithEmptyNumbers = new List<Flight>
        {
            new Flight { FlightNumber = null },
            new Flight { FlightNumber = null }
        };

        flightRouteRepositoryWithNullNumbers.Setup(getFlightWithNullNumbers => getFlightWithNullNumbers.GetFlightsByCompanyId(TargetCompanyId))
            .Returns(flightsWithEmptyNumbers);

        var companyService = new CompanyService(companyRepository.Object, flightRouteRepositoryWithNullNumbers.Object);

        string generatedCode = companyService.GenerateFlightCodeUsingCompanyId(TargetCompanyId);

        Assert.EndsWith(StartingSequence, generatedCode);
    }

    [Fact]
    public void GenerateFlightCodeUsingCompanyId_ReturnsFullNamePrefix_WhenNameIsTooShort()
    {
        var companyRepository = new Mock<ICompanyRepository>();
        var flightRouteRepository = new Mock<IFlightRouteService>();

        companyRepository.Setup(getCompanyWithNameShorterThanPrefix => getCompanyWithNameShorterThanPrefix.GetCompanyById(TargetCompanyId))
            .Returns(new Company { Name = SingleCharCompanyName });

        flightRouteRepository.Setup(getNoFlights => getNoFlights.GetFlightsByCompanyId(TargetCompanyId))
            .Returns(new List<Flight>());

        var companyService = new CompanyService(companyRepository.Object, flightRouteRepository.Object);

        string generatedCode = companyService.GenerateFlightCodeUsingCompanyId(TargetCompanyId);

        Assert.StartsWith(SingleCharCompanyCode, generatedCode);
    }

    [Fact]
    public void UpdateCompany_DoesNotCallRepository_WhenCompanyIsNotFound()
    {
        var companyRepositoryThatReturnsNull = new Mock<ICompanyRepository>();
        var flightRouteRepository = new Mock<IFlightRouteService>();

        companyRepositoryThatReturnsNull.Setup(getNullInsteadOfComapny => getNullInsteadOfComapny.GetCompanyById(TargetCompanyId)).Returns((Company?)null);

        var companyService = new CompanyService(companyRepositoryThatReturnsNull.Object, flightRouteRepository.Object);

        companyService.UpdateCompany(TargetCompanyId, UpdatedCompanyName);

        companyRepositoryThatReturnsNull.Verify(doesNotCallRepository => doesNotCallRepository.UpdateCompany(It.IsAny<Company>()), Times.Never);
    }

    [Fact]
    public void UpdateCompany_ThrowsArgumentException_WhenNameIsWhitespace()
    {
        var companyRepository = new Mock<ICompanyRepository>();
        var flightRouteRepository = new Mock<IFlightRouteService>();

        var existingCompany = new Company { Name = DefaultCompanyTwoWordsName };

        companyRepository.Setup(getDefaultCompany => getDefaultCompany.GetCompanyById(TargetCompanyId)).Returns(existingCompany);

        var companyService = new CompanyService(companyRepository.Object, flightRouteRepository.Object);

        Assert.Throws<ArgumentException>(() => companyService.UpdateCompany(TargetCompanyId, "   "));
    }

    [Fact]
    public void UpdateCompany_UpdatesName_WhenNameIsValid()
    {
        var companyRepository = new Mock<ICompanyRepository>();
        var flightRouteRepository = new Mock<IFlightRouteService>();

        var companyToUpdate = new Company { Name = DefaultCompanyTwoWordsName };

        companyRepository.Setup(getDefaultCompany => getDefaultCompany.GetCompanyById(TargetCompanyId)).Returns(companyToUpdate);

        var companyService = new CompanyService(companyRepository.Object, flightRouteRepository.Object);

        companyService.UpdateCompany(TargetCompanyId, UpdatedCompanyName);

        Assert.Equal(UpdatedCompanyName, companyToUpdate.Name);
        companyRepository.Verify(repositoryIsCalled => repositoryIsCalled.UpdateCompany(companyToUpdate), Times.Once);
    }

    [Fact]
    public void DeleteCompanyUsingId_DoesNotCallRepository_WhenIdIsInvalid()
    {
        var companyRepository = new Mock<ICompanyRepository>();
        var flightRouteRepository = new Mock<IFlightRouteService>();

        var companyService = new CompanyService(companyRepository.Object, flightRouteRepository.Object);

        companyService.DeleteCompanyUsingId(InvalidCompanyId);

        companyRepository.Verify(doesNotCallRepository => doesNotCallRepository.DeleteCompanyUsingId(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void DeleteCompanyUsingId_CallsRepositoryDelete_WhenIdIsValid()
    {
        var companyRepository = new Mock<ICompanyRepository>();
        var flightRouteRepository = new Mock<IFlightRouteService>();

        var companyService = new CompanyService(companyRepository.Object, flightRouteRepository.Object);

        companyService.DeleteCompanyUsingId(TargetCompanyId);

        companyRepository.Verify(repositoryIsCalled => repositoryIsCalled.DeleteCompanyUsingId(TargetCompanyId), Times.Once);
    }
}
