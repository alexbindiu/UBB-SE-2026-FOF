using System;
using System.Collections.Generic;
using System.Text;

using Moq;

using TicketSellingModule.Data.Repositories.Interfaces;
using TicketSellingModule.Data.Services.Interfaces;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class CompanyServiceTests
{
    [Fact]
    public void Add_Should_Throw_Exception_For_Null_Or_Whitespace_Name()
    {
        var mockRepo = new Mock<ICompanyRepository>();
        var mockFlightRouteService = new Mock<IFlightRouteService>();

        var service = new CompanyService(mockRepo.Object, mockFlightRouteService.Object);

        Assert.Throws<ArgumentException>(() => service.AddCompany(null));
        Assert.Throws<ArgumentException>(() => service.AddCompany(string.Empty));
        Assert.Throws<ArgumentException>(() => service.AddCompany("   "));
    }

    [Fact]
    public void Add_Should_Work()
    {
        var mockRepo = new Mock<ICompanyRepository>();
        var mockFlightRouteService = new Mock<IFlightRouteService>();

        mockRepo.Setup(r => r.AddCompany(It.IsAny<Company>())).Returns(1);

        var service = new CompanyService(mockRepo.Object, mockFlightRouteService.Object);

        var result = service.AddCompany("Wizz Air");

        Assert.Equal(1, result);
    }

    [Fact]
    public void GetAll_Should_Return_All_Companies()
    {
        var mockRepo = new Mock<ICompanyRepository>();
        var mockFlightRouteService = new Mock<IFlightRouteService>();

        var companies = new List<Company> { new Company(), new Company() };

        mockRepo.Setup(r => r.GetAllCompanies()).Returns(companies);

        var service = new CompanyService(mockRepo.Object, mockFlightRouteService.Object);

        var result = service.GetAllCompanies();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetCompanyById_Should_Return_Null_For_Invalid_Id()
    {
        var mockRepo = new Mock<ICompanyRepository>();
        var mockFlightRouteService = new Mock<IFlightRouteService>();

        var service = new CompanyService(mockRepo.Object, mockFlightRouteService.Object);

        var result = service.GetCompanyById(0);

        Assert.Null(result);
    }

    [Fact]
    public void GetCompanyById_Should_Return_Company()
    {
        var mockRepo = new Mock<ICompanyRepository>();
        var mockFlightRouteService = new Mock<IFlightRouteService>();

        var company = new Company { Name = "Test" };

        mockRepo.Setup(r => r.GetCompanyById(1)).Returns(company);

        var service = new CompanyService(mockRepo.Object, mockFlightRouteService.Object);

        var result = service.GetCompanyById(1);

        Assert.Equal(company, result);
    }

    [Fact]
    public void GenerateFlightCode_Should_Use_Default_Prefix_When_Company_Not_Found()
    {
        var mockRepo = new Mock<ICompanyRepository>();
        var mockFlightRouteService = new Mock<IFlightRouteService>();

        mockRepo.Setup(r => r.GetCompanyById(1)).Returns((Company)null);
        mockFlightRouteService.Setup(r => r.GetFlightsByCompanyId(1)).Returns(new List<Flight>());

        var service = new CompanyService(mockRepo.Object, mockFlightRouteService.Object);

        var result = service.GenerateFlightCodeUsingCompanyId(1);

        Assert.StartsWith("FL-", result);
    }

    [Fact]
    public void GenerateFlightCode_Should_Use_Initials_For_Two_Words()
    {
        var mockRepo = new Mock<ICompanyRepository>();
        var mockFlightRouteService = new Mock<IFlightRouteService>();

        mockRepo.Setup(r => r.GetCompanyById(1))
            .Returns(new Company { Name = "Wizz Air" });

        mockFlightRouteService.Setup(r => r.GetFlightsByCompanyId(1))
            .Returns(new List<Flight>());

        var service = new CompanyService(mockRepo.Object, mockFlightRouteService.Object);

        var result = service.GenerateFlightCodeUsingCompanyId(1);

        Assert.StartsWith("WA-", result);
    }

    [Fact]
    public void GenerateFlightCode_Should_Use_First_Two_Letters_For_One_Word()
    {
        var mockRepo = new Mock<ICompanyRepository>();
        var mockFlightRouteService = new Mock<IFlightRouteService>();

        mockRepo.Setup(r => r.GetCompanyById(1))
            .Returns(new Company { Name = "Tarom" });

        mockFlightRouteService.Setup(r => r.GetFlightsByCompanyId(1))
            .Returns(new List<Flight>());

        var service = new CompanyService(mockRepo.Object, mockFlightRouteService.Object);

        var result = service.GenerateFlightCodeUsingCompanyId(1);

        Assert.StartsWith("TA-", result);
    }

    [Fact]
    public void GenerateFlightCode_Should_Start_From_Initial_Number()
    {
        var mockRepo = new Mock<ICompanyRepository>();
        var mockFlightRouteService = new Mock<IFlightRouteService>();

        mockRepo.Setup(r => r.GetCompanyById(1))
            .Returns(new Company { Name = "Test Company" });

        mockFlightRouteService.Setup(r => r.GetFlightsByCompanyId(1))
            .Returns(new List<Flight>());

        var service = new CompanyService(mockRepo.Object, mockFlightRouteService.Object);

        var result = service.GenerateFlightCodeUsingCompanyId(1);

        Assert.EndsWith("-1000", result);
    }

    [Fact]
    public void GenerateFlightCode_Should_Increment_Max_Flight_Number()
    {
        var mockRepo = new Mock<ICompanyRepository>();
        var mockFlightRouteService = new Mock<IFlightRouteService>();

        mockRepo.Setup(r => r.GetCompanyById(1))
            .Returns(new Company { Name = "Test Company" });

        var flights = new List<Flight>
        {
            new Flight { FlightNumber = "TC-1000" },
            new Flight { FlightNumber = "TC-1005" }
        };

        mockFlightRouteService.Setup(r => r.GetFlightsByCompanyId(1))
            .Returns(flights);

        var service = new CompanyService(mockRepo.Object, mockFlightRouteService.Object);

        var result = service.GenerateFlightCodeUsingCompanyId(1);

        Assert.EndsWith("-1006", result);
    }

    [Fact]
    public void GenerateFlightCode_Should_Ignore_Invalid_FlightNumbers()
    {
        var mockRepo = new Mock<ICompanyRepository>();
        var mockFlightRouteService = new Mock<IFlightRouteService>();

        mockRepo.Setup(r => r.GetCompanyById(1))
            .Returns(new Company { Name = "Test Company" });

        var flights = new List<Flight>
    {
        new Flight { FlightNumber = "INVALID" },
        new Flight { FlightNumber = "TC-ABC" }
    };

        mockFlightRouteService.Setup(r => r.GetFlightsByCompanyId(1))
            .Returns(flights);

        var service = new CompanyService(mockRepo.Object, mockFlightRouteService.Object);

        var result = service.GenerateFlightCodeUsingCompanyId(1);

        Assert.EndsWith("-1000", result);
    }

    [Fact]
    public void GenerateFlightCode_Should_Use_Starting_Sequence_When_Flights_List_Is_Null()
    {
        var mockRepo = new Mock<ICompanyRepository>();
        var mockFlightRouteService = new Mock<IFlightRouteService>();

        mockRepo.Setup(r => r.GetCompanyById(1))
            .Returns(new Company { Name = "Test Company" });

        mockFlightRouteService.Setup(r => r.GetFlightsByCompanyId(1))
            .Returns((List<Flight>)null);

        var service = new CompanyService(mockRepo.Object, mockFlightRouteService.Object);

        var result = service.GenerateFlightCodeUsingCompanyId(1);

        Assert.EndsWith("-1000", result);
    }

    [Fact]
    public void GenerateFlightCode_Should_Handle_Null_Or_Empty_Existing_FlightNumbers()
    {
        var mockRepo = new Mock<ICompanyRepository>();
        var mockFlightRouteService = new Mock<IFlightRouteService>();

        mockRepo.Setup(r => r.GetCompanyById(1))
            .Returns(new Company { Name = "Test Company" });

        var flights = new List<Flight>
        {
            new Flight { FlightNumber = null },
            new Flight { FlightNumber = string.Empty },
            new Flight { FlightNumber = "NoDelimiterHere" }
        };

        mockFlightRouteService.Setup(r => r.GetFlightsByCompanyId(1))
            .Returns(flights);

        var service = new CompanyService(mockRepo.Object, mockFlightRouteService.Object);

        var result = service.GenerateFlightCodeUsingCompanyId(1);

        Assert.EndsWith("-1000", result);
    }

    [Fact]
    public void GenerateFlightCode_Should_Use_Full_Name_When_Single_Word_Is_Shorter_Than_Minimum_Prefix()
    {
        var mockRepo = new Mock<ICompanyRepository>();
        var mockFlightRouteService = new Mock<IFlightRouteService>();

        mockRepo.Setup(r => r.GetCompanyById(1))
            .Returns(new Company { Name = "X" });

        mockFlightRouteService.Setup(r => r.GetFlightsByCompanyId(1))
            .Returns(new List<Flight>());

        var service = new CompanyService(mockRepo.Object, mockFlightRouteService.Object);

        var result = service.GenerateFlightCodeUsingCompanyId(1);

        Assert.StartsWith("X-", result);
    }

    [Fact]
    public void Update_Should_Do_Nothing_If_Company_Not_Found()
    {
        var mockRepo = new Mock<ICompanyRepository>();
        var mockFlightRouteService = new Mock<IFlightRouteService>();

        mockRepo.Setup(r => r.GetCompanyById(1)).Returns((Company)null);

        var service = new CompanyService(mockRepo.Object, mockFlightRouteService.Object);

        service.UpdateCompany(1, "NewName");

        mockRepo.Verify(r => r.UpdateCompany(It.IsAny<Company>()), Times.Never);
    }

    [Fact]
    public void Update_Should_Throw_For_Whitespace_Name()
    {
        var mockRepo = new Mock<ICompanyRepository>();
        var mockFlightRouteService = new Mock<IFlightRouteService>();

        var company = new Company { Name = "Old" };

        mockRepo.Setup(r => r.GetCompanyById(1)).Returns(company);

        var service = new CompanyService(mockRepo.Object, mockFlightRouteService.Object);

        Assert.Throws<ArgumentException>(() => service.UpdateCompany(1, "   "));
    }

    [Fact]
    public void Update_Should_Update_Name()
    {
        var mockRepo = new Mock<ICompanyRepository>();
        var mockFlightRouteService = new Mock<IFlightRouteService>();

        var company = new Company { Name = "Old" };

        mockRepo.Setup(r => r.GetCompanyById(1)).Returns(company);

        var service = new CompanyService(mockRepo.Object, mockFlightRouteService.Object);

        service.UpdateCompany(1, "New");

        Assert.Equal("New", company.Name);
        mockRepo.Verify(r => r.UpdateCompany(company), Times.Once);
    }

    [Fact]
    public void Delete_Should_Not_Call_Repo_For_Invalid_Id()
    {
        var mockRepo = new Mock<ICompanyRepository>();
        var mockFlightRouteService = new Mock<IFlightRouteService>();

        var service = new CompanyService(mockRepo.Object, mockFlightRouteService.Object);

        service.DeleteCompanyUsingId(0);

        mockRepo.Verify(r => r.DeleteCompanyUsingId(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void Delete_Should_Call_Repo_For_Valid_Id()
    {
        var mockRepo = new Mock<ICompanyRepository>();
        var mockFlightRouteService = new Mock<IFlightRouteService>();

        var service = new CompanyService(mockRepo.Object, mockFlightRouteService.Object);

        service.DeleteCompanyUsingId(3);

        mockRepo.Verify(r => r.DeleteCompanyUsingId(3), Times.Once);
    }
}
