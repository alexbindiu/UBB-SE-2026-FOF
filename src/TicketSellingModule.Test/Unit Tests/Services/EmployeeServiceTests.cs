using System;
using System.Collections.Generic;
using System.Text;

using Moq;

using TicketSellingModule.Data.Repositories.Interfaces;
using TicketSellingModule.Data.Services.Interfaces;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class EmployeeServiceTests
{
    [Fact]
    public void GetAll_Should_Return_All_Employees()
    {
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockEmployeeFlightService = new Mock<IEmployeeFlightService>();

        var employees = new List<Employee> { new Employee(), new Employee() };
        mockEmployeeRepo.Setup(r => r.GetAllEmployees()).Returns(employees);

        var service = new EmployeeService(mockEmployeeRepo.Object, mockEmployeeFlightService.Object);

        var result = service.GetAllEmployees();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetById_Should_Return_Null_For_Invalid_Id()
    {
        var service = new EmployeeService(
            new Mock<IEmployeeRepository>().Object,
            new Mock<IEmployeeFlightService>().Object);

        Assert.Null(service.GetEmployeeById(0));
    }

    [Fact]
    public void GetPilots_Should_Filter_Correctly()
    {
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockEmployeeFlightService = new Mock<IEmployeeFlightService>();

        var employees = new List<Employee>
        {
            new Employee { Role = EmployeeRole.Pilot },
            new Employee { Role = EmployeeRole.CoPilot }
        };

        mockEmployeeRepo.Setup(r => r.GetAllEmployees()).Returns(employees);

        var service = new EmployeeService(mockEmployeeRepo.Object, mockEmployeeFlightService.Object);

        var result = service.GetPilots();

        Assert.Single(result);
        Assert.Equal(EmployeeRole.Pilot, result[0].Role);
    }

    [Fact]
    public void GetCoPilots_Should_Filter_Correctly()
    {
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockEmployeeFlightService = new Mock<IEmployeeFlightService>();

        var employees = new List<Employee>
        {
            new Employee { Role = EmployeeRole.Pilot },
            new Employee { Role = EmployeeRole.CoPilot }
        };

        mockEmployeeRepo.Setup(r => r.GetAllEmployees()).Returns(employees);

        var service = new EmployeeService(mockEmployeeRepo.Object, mockEmployeeFlightService.Object);

        var result = service.GetCoPilots();

        Assert.Single(result);
        Assert.Equal(EmployeeRole.CoPilot, result[0].Role);
    }

    [Fact]
    public void GetFlightAttendants_Should_Filter_Correctly()
    {
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockEmployeeFlightService = new Mock<IEmployeeFlightService>();

        var employees = new List<Employee>
        {
            new Employee { Role = EmployeeRole.Pilot },
            new Employee { Role = EmployeeRole.FlightAttendant }
        };

        mockEmployeeRepo.Setup(r => r.GetAllEmployees()).Returns(employees);

        var service = new EmployeeService(mockEmployeeRepo.Object, mockEmployeeFlightService.Object);

        var result = service.GetFlightAttendants();

        Assert.Single(result);
        Assert.Equal(EmployeeRole.FlightAttendant, result[0].Role);
    }

    [Fact]
    public void GetFlightDispatchers_Should_Filter_Correctly()
    {
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockEmployeeFlightService = new Mock<IEmployeeFlightService>();

        var employees = new List<Employee>
        {
            new Employee { Role = EmployeeRole.Pilot },
            new Employee { Role = EmployeeRole.FlightDispatcher }
        };

        mockEmployeeRepo.Setup(r => r.GetAllEmployees()).Returns(employees);

        var service = new EmployeeService(mockEmployeeRepo.Object, mockEmployeeFlightService.Object);

        var result = service.GetFlightDispatchers();

        Assert.Single(result);
        Assert.Equal(EmployeeRole.FlightDispatcher, result[0].Role);
    }

    [Fact]
    public void Add_Should_Throw_For_Invalid_Inputs()
    {
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockEmployeeFlightService = new Mock<IEmployeeFlightService>();

        var service = new EmployeeService(mockEmployeeRepo.Object, mockEmployeeFlightService.Object);

        Assert.Throws<ArgumentException>(() =>
            service.AddEmployee(null, EmployeeRole.Pilot, DateOnly.FromDateTime(DateTime.Now), 100, DateOnly.FromDateTime(DateTime.Now)));

        Assert.Throws<ArgumentException>(() =>
            service.AddEmployee("Test", EmployeeRole.Pilot, DateOnly.FromDateTime(DateTime.Now), -1, DateOnly.FromDateTime(DateTime.Now)));
    }

    [Fact]
    public void Add_Should_Work()
    {
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockEmployeeFlightService = new Mock<IEmployeeFlightService>();

        mockEmployeeRepo.Setup(r => r.AddEmployee(It.IsAny<Employee>())).Returns(10);

        var service = new EmployeeService(mockEmployeeRepo.Object, mockEmployeeFlightService.Object);

        var result = service.AddEmployee(
            "Test",
            EmployeeRole.Pilot,
            DateOnly.FromDateTime(DateTime.Now.AddYears(-20)),
            1000,
            DateOnly.FromDateTime(DateTime.Now.AddYears(-1)));

        Assert.Equal(10, result);
    }

    [Fact]
    public void Update_Should_Do_Nothing_If_Not_Found()
    {
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockEmployeeFlightService = new Mock<IEmployeeFlightService>();

        var service = new EmployeeService(mockEmployeeRepo.Object, mockEmployeeFlightService.Object);

        mockEmployeeRepo.Setup(r => r.GetEmployeeById(1)).Returns((Employee)null);

        service.UpdateEmployee(1, name: "New");

        mockEmployeeRepo.Verify(r => r.UpdateEmployee(It.IsAny<Employee>()), Times.Never);
    }

    [Fact]
    public void Update_Should_Update_All_Fields()
    {
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockEmployeeFlightService = new Mock<IEmployeeFlightService>();

        var employee = new Employee { Id = 1, Name = "Old" };

        mockEmployeeRepo.Setup(r => r.GetEmployeeById(1)).Returns(employee);

        var service = new EmployeeService(mockEmployeeRepo.Object, mockEmployeeFlightService.Object);
        service.UpdateEmployee(1,
            name: "New",
            role: EmployeeRole.CoPilot,
            salary: 2000,
            birthday: DateOnly.FromDateTime(DateTime.Now.AddYears(-30)),
            hiringDate: DateOnly.FromDateTime(DateTime.Now.AddYears(-1)));

        Assert.Equal("New", employee.Name);
        Assert.Equal(2000, employee.Salary);

        mockEmployeeRepo.Verify(r => r.UpdateEmployee(employee), Times.Once);
    }

    [Fact]
    public void Delete_Should_Not_Call_Repo_For_Invalid_Id()
    {
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockEmployeeFlightService = new Mock<IEmployeeFlightService>();
        var service = new EmployeeService(mockEmployeeRepo.Object, mockEmployeeFlightService.Object);

        service.DeleteEmployeeUsingId(0);

        mockEmployeeRepo.Verify(r => r.DeleteEmployee(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void DeleteWithAssignments_Should_Call_Cleanup_And_Delete()
    {
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockEmployeeFlightService = new Mock<IEmployeeFlightService>();

        var service = new EmployeeService(mockEmployeeRepo.Object, mockEmployeeFlightService.Object);

        service.DeleteWithAssignments(1);

        mockEmployeeFlightService.Verify(r => r.RemoveAllFlightsAssignmentsForEmployee(1), Times.Once);
        mockEmployeeRepo.Verify(r => r.DeleteEmployee(1), Times.Once);
    }

    [Fact]
    public void SaveEmployee_Should_Throw_For_Null_Employee()
    {
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockEmployeeFlightService = new Mock<IEmployeeFlightService>();

        var service = new EmployeeService(mockEmployeeRepo.Object, mockEmployeeFlightService.Object);

        Assert.Throws<ArgumentException>(() =>
            service.SaveEmployee(null, DateTimeOffset.Now, DateTimeOffset.Now, "100"));
    }

    [Fact]
    public void SaveEmployee_Should_Throw_For_Invalid_Salary()
    {
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockEmployeeFlightService = new Mock<IEmployeeFlightService>();

        var service = new EmployeeService(mockEmployeeRepo.Object, mockEmployeeFlightService.Object);

        var employee = new Employee();

        Assert.Throws<ArgumentException>(() =>
            service.SaveEmployee(employee, DateTimeOffset.Now, DateTimeOffset.Now, "abc"));
    }

    [Fact]
    public void SaveEmployee_Should_Call_Add_For_New_Employee()
    {
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockEmployeeFlightService = new Mock<IEmployeeFlightService>();

        var service = new EmployeeService(mockEmployeeRepo.Object, mockEmployeeFlightService.Object);

        var employee = new Employee
        {
            Id = 0,
            Name = "Test",
            Role = EmployeeRole.Pilot
        };

        mockEmployeeRepo.Setup(r => r.AddEmployee(It.IsAny<Employee>())).Returns(1);

        service.SaveEmployee(employee,
            DateTimeOffset.Now.AddYears(-20),
            DateTimeOffset.Now.AddYears(-1),
            "1000");

        mockEmployeeRepo.Verify(r => r.AddEmployee(It.IsAny<Employee>()), Times.Once);
    }

    [Fact]
    public void SaveEmployee_Should_Call_Update_For_Existing_Employee()
    {
        var mockEmployeeRepo = new Mock<IEmployeeRepository>();
        var mockEmployeeFlightService = new Mock<IEmployeeFlightService>();

        var emp = new Employee
        {
            Id = 1,
            Name = "Test",
            Role = EmployeeRole.Pilot
        };

        mockEmployeeRepo.Setup(r => r.GetEmployeeById(1)).Returns(emp);

        var service = new EmployeeService(mockEmployeeRepo.Object, mockEmployeeFlightService.Object);

        service.SaveEmployee(emp,
            DateTimeOffset.Now.AddYears(-20),
            DateTimeOffset.Now.AddYears(-1),
            "1000");

        mockEmployeeRepo.Verify(r => r.UpdateEmployee(It.IsAny<Employee>()), Times.Once);
    }
}
