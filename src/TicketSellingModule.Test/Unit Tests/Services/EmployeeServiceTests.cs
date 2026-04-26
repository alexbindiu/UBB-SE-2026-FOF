using Moq;

namespace TicketSellingModule.Test.Unit_Tests.Services;

public class EmployeeServiceTests
{
    private const int TargetEmployeeId = 1;
    private const int InvalidEmployeeId = 0;
    private const int InvalidNegativeEmployeeId = -1;
    private const int GeneratedEmployeeId = 10;

    private const string DefaultName = "Test Employee";
    private const string UpdatedName = "Updated Name";

    private const int ValidSalary = 1000;
    private const int UpdatedSalary = 2000;
    private const int NegativeSalary = -1;
    private const string ValidSalaryText = "1000";
    private const string InvalidSalaryText = "abc";

    private static readonly DateOnly ValidBirthday = DateOnly.FromDateTime(DateTime.Today.AddYears(-30));
    private static readonly DateOnly ValidHiringDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-1));
    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
    private static readonly DateTimeOffset ValidBirthdayOffset = DateTimeOffset.Now.AddYears(-30);
    private static readonly DateTimeOffset ValidHiringDateOffset = DateTimeOffset.Now.AddYears(-1);
    private static readonly DateTimeOffset FutureDateOffset = DateTimeOffset.Now.AddDays(1);

    private static EmployeeService CreateTestService(
        Mock<IEmployeeRepository>? employeeDataSource = null,
        Mock<IEmployeeFlightService>? employeeFlightServiceMock = null)
    {
        return new EmployeeService(
            (employeeDataSource ?? new Mock<IEmployeeRepository>()).Object,
            (employeeFlightServiceMock ?? new Mock<IEmployeeFlightService>()).Object);
    }

    [Fact]
    public void GetAllEmployees_ReturnsAllEmployees_WhenEmployeesExist()
    {
        var employeeDataSource = new Mock<IEmployeeRepository>();
        var existingEmployeesList = new List<Employee> { new Employee(), new Employee() };

        employeeDataSource.Setup(getExistingEmployees => getExistingEmployees.GetAllEmployees()).Returns(existingEmployeesList);

        var employeeService = CreateTestService(employeeDataSource: employeeDataSource);

        List<Employee> resultList = employeeService.GetAllEmployees();

        Assert.Equal(2, resultList.Count);
    }

    [Fact]
    public void GetEmployeeById_ReturnsNull_WhenIdIsInvalid()
    {
        var employeeService = CreateTestService();

        Employee? result = employeeService.GetEmployeeById(InvalidEmployeeId);

        Assert.Null(result);
    }

    [Fact]
    public void GetEmployeeById_ReturnsEmployee_WhenIdIsValid()
    {
        var employeeDataSource = new Mock<IEmployeeRepository>();
        var targetEmployee = new Employee { Id = TargetEmployeeId };

        employeeDataSource.Setup(getTargetEmployee => getTargetEmployee.GetEmployeeById(TargetEmployeeId)).Returns(targetEmployee);

        var employeeService = CreateTestService(employeeDataSource: employeeDataSource);

        Employee? result = employeeService.GetEmployeeById(TargetEmployeeId);

        Assert.NotNull(result);
        Assert.Equal(TargetEmployeeId, result.Id);
    }

    [Fact]
    public void GetPilots_ReturnsOnlyPilots_WhenMixedRolesExist()
    {
        var employeeDataSource = new Mock<IEmployeeRepository>();
        var mixedEmployeesList = new List<Employee>
        {
            new Employee { Role = EmployeeRole.Pilot },
            new Employee { Role = EmployeeRole.CoPilot }
        };

        employeeDataSource.Setup(getAllEmplyees => getAllEmplyees.GetAllEmployees()).Returns(mixedEmployeesList);

        var employeeService = CreateTestService(employeeDataSource: employeeDataSource);

        List<Employee> resultList = employeeService.GetPilots();

        Assert.Single(resultList);
        Assert.Equal(EmployeeRole.Pilot, resultList[0].Role);
    }

    [Fact]
    public void GetCoPilots_ReturnsOnlyCoPilots_WhenMixedRolesExist()
    {
        var employeeDataSource = new Mock<IEmployeeRepository>();
        var mixedEmployeesList = new List<Employee>
        {
            new Employee { Role = EmployeeRole.Pilot },
            new Employee { Role = EmployeeRole.CoPilot }
        };

        employeeDataSource.Setup(getAllEmployees => getAllEmployees.GetAllEmployees()).Returns(mixedEmployeesList);

        var employeeService = CreateTestService(employeeDataSource: employeeDataSource);

        List<Employee> resultList = employeeService.GetCoPilots();

        Assert.Single(resultList);
        Assert.Equal(EmployeeRole.CoPilot, resultList[0].Role);
    }

    [Fact]
    public void GetFlightAttendants_ReturnsOnlyFlightAttendants_WhenMixedRolesExist()
    {
        var employeeDataSource = new Mock<IEmployeeRepository>();
        var mixedEmployeesList = new List<Employee>
        {
            new Employee { Role = EmployeeRole.Pilot },
            new Employee { Role = EmployeeRole.FlightAttendant }
        };

        employeeDataSource.Setup(getAllEmployees => getAllEmployees.GetAllEmployees()).Returns(mixedEmployeesList);

        var employeeService = CreateTestService(employeeDataSource: employeeDataSource);

        List<Employee> resultList = employeeService.GetFlightAttendants();

        Assert.Single(resultList);
        Assert.Equal(EmployeeRole.FlightAttendant, resultList[0].Role);
    }

    [Fact]
    public void GetFlightDispatchers_ReturnsOnlyDispatchers_WhenMixedRolesExist()
    {
        var employeeDataSource = new Mock<IEmployeeRepository>();
        var mixedEmployeesList = new List<Employee>
        {
            new Employee { Role = EmployeeRole.Pilot },
            new Employee { Role = EmployeeRole.FlightDispatcher }
        };

        employeeDataSource.Setup(getAllEmployees => getAllEmployees.GetAllEmployees()).Returns(mixedEmployeesList);

        var employeeService = CreateTestService(employeeDataSource: employeeDataSource);

        List<Employee> resultList = employeeService.GetFlightDispatchers();

        Assert.Single(resultList);
        Assert.Equal(EmployeeRole.FlightDispatcher, resultList[0].Role);
    }

    [Fact]
    public void AddEmployee_ThrowsArgumentException_WhenNameIsNull()
    {
        var employeeService = CreateTestService();

        Assert.Throws<ArgumentException>(() =>
            employeeService.AddEmployee(null!, EmployeeRole.Pilot, ValidBirthday, ValidSalary, ValidHiringDate));
    }

    [Fact]
    public void AddEmployee_ThrowsArgumentException_WhenNameIsEmpty()
    {
        var employeeService = CreateTestService();

        Assert.Throws<ArgumentException>(() =>
            employeeService.AddEmployee(string.Empty, EmployeeRole.Pilot, ValidBirthday, ValidSalary, ValidHiringDate));
    }

    [Fact]
    public void AddEmployee_ThrowsArgumentException_WhenNameIsWhitespace()
    {
        var employeeService = CreateTestService();

        Assert.Throws<ArgumentException>(() =>
            employeeService.AddEmployee("   ", EmployeeRole.Pilot, ValidBirthday, ValidSalary, ValidHiringDate));
    }

    [Fact]
    public void AddEmployee_ThrowsArgumentException_WhenSalaryIsNegative()
    {
        var employeeService = CreateTestService();

        Assert.Throws<ArgumentException>(() =>
            employeeService.AddEmployee(DefaultName, EmployeeRole.Pilot, ValidBirthday, NegativeSalary, ValidHiringDate));
    }

    [Fact]
    public void AddEmployee_ThrowsArgumentException_WhenBirthdayIsInFuture()
    {
        var employeeService = CreateTestService();

        Assert.Throws<ArgumentException>(() =>
            employeeService.AddEmployee(DefaultName, EmployeeRole.Pilot, FutureDate, ValidSalary, ValidHiringDate));
    }

    [Fact]
    public void AddEmployee_ThrowsArgumentException_WhenHiringDateIsInFuture()
    {
        var employeeService = CreateTestService();

        Assert.Throws<ArgumentException>(() =>
            employeeService.AddEmployee(DefaultName, EmployeeRole.Pilot, ValidBirthday, ValidSalary, FutureDate));
    }

    [Fact]
    public void AddEmployee_ReturnsGeneratedId_WhenArgumentsAreValid()
    {
        var employeeDataSource = new Mock<IEmployeeRepository>();

        employeeDataSource.Setup(addEmployeeToRepo => addEmployeeToRepo.AddEmployee(It.IsAny<Employee>())).Returns(GeneratedEmployeeId);

        var employeeService = CreateTestService(employeeDataSource: employeeDataSource);

        int resultId = employeeService.AddEmployee(DefaultName, EmployeeRole.Pilot, ValidBirthday, ValidSalary, ValidHiringDate);

        Assert.Equal(GeneratedEmployeeId, resultId);
    }

    [Fact]
    public void UpdateEmployee_DoesNotCallRepository_WhenEmployeeNotFound()
    {
        var employeeDataSourceThatReturnsNull = new Mock<IEmployeeRepository>();
        employeeDataSourceThatReturnsNull.Setup(getNullInsteadOfEmployee => getNullInsteadOfEmployee.GetEmployeeById(TargetEmployeeId)).Returns((Employee?)null);

        var employeeService = CreateTestService(employeeDataSource: employeeDataSourceThatReturnsNull);

        employeeService.UpdateEmployee(TargetEmployeeId, name: UpdatedName);

        employeeDataSourceThatReturnsNull.Verify(doesNotCallRepository => doesNotCallRepository.UpdateEmployee(It.IsAny<Employee>()), Times.Never);
    }

    [Fact]
    public void UpdateEmployee_UpdatesProvidedFields_WhenEmployeeIsFound()
    {
        var employeeDataSource = new Mock<IEmployeeRepository>();
        var targetEmployee = new Employee { Id = TargetEmployeeId, Name = DefaultName };

        employeeDataSource.Setup(getTargetEmployee => getTargetEmployee.GetEmployeeById(TargetEmployeeId)).Returns(targetEmployee);

        var employeeService = CreateTestService(employeeDataSource: employeeDataSource);

        employeeService.UpdateEmployee(
            TargetEmployeeId,
            name: UpdatedName,
            role: EmployeeRole.CoPilot,
            salary: UpdatedSalary,
            birthday: ValidBirthday,
            hiringDate: ValidHiringDate);

        Assert.Equal(UpdatedName, targetEmployee.Name);
        Assert.Equal(UpdatedSalary, targetEmployee.Salary);
        Assert.Equal(EmployeeRole.CoPilot, targetEmployee.Role);

        employeeDataSource.Verify(repositoryGetsCalledToUpdateTheEmployee => repositoryGetsCalledToUpdateTheEmployee.UpdateEmployee(targetEmployee), Times.Once);
    }

    [Fact]
    public void DeleteEmployeeUsingId_DoesNotCallRepository_WhenIdIsInvalid()
    {
        var employeeDataSource = new Mock<IEmployeeRepository>();
        var employeeService = CreateTestService(employeeDataSource: employeeDataSource);

        employeeService.DeleteEmployeeUsingId(InvalidEmployeeId);

        employeeDataSource.Verify(doesNotCallRepository => doesNotCallRepository.DeleteEmployee(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void DeleteWithAssignments_ThrowsArgumentException_WhenIdIsInvalid()
    {
        var employeeService = CreateTestService();

        Assert.Throws<ArgumentException>(() => employeeService.DeleteWithAssignments(InvalidEmployeeId));
        Assert.Throws<ArgumentException>(() => employeeService.DeleteWithAssignments(InvalidNegativeEmployeeId));
    }

    [Fact]
    public void DeleteWithAssignments_CallsCleanupAndDelete_WhenIdIsValid()
    {
        var employeeDataSource = new Mock<IEmployeeRepository>();
        var employeeFlightServiceMock = new Mock<IEmployeeFlightService>();

        var employeeService = CreateTestService(
            employeeDataSource: employeeDataSource,
            employeeFlightServiceMock: employeeFlightServiceMock);

        employeeService.DeleteWithAssignments(TargetEmployeeId);

        employeeFlightServiceMock.Verify(repoGetsCalledToRemoveAllFlightsAssignedToEmployee => repoGetsCalledToRemoveAllFlightsAssignedToEmployee.RemoveAllFlightsAssignmentsForEmployee(TargetEmployeeId), Times.Once);
        employeeDataSource.Verify(repoGetsCalledToDeleteEmployee => repoGetsCalledToDeleteEmployee.DeleteEmployee(TargetEmployeeId), Times.Once);
    }

    [Fact]
    public void SaveEmployee_ThrowsArgumentException_WhenEmployeeIsNull()
    {
        var employeeService = CreateTestService();

        Assert.Throws<ArgumentException>(() =>
            employeeService.SaveEmployee(null!, ValidBirthdayOffset, ValidHiringDateOffset, ValidSalaryText));
    }

    [Fact]
    public void SaveEmployee_ThrowsArgumentException_WhenSalaryIsInvalidFormat()
    {
        var employeeService = CreateTestService();
        var targetEmployee = new Employee();

        Assert.Throws<ArgumentException>(() =>
            employeeService.SaveEmployee(targetEmployee, ValidBirthdayOffset, ValidHiringDateOffset, InvalidSalaryText));
    }

    [Fact]
    public void SaveEmployee_ThrowsArgumentException_WhenBirthdayIsNull()
    {
        var employeeService = CreateTestService();
        var targetEmployee = new Employee { Id = TargetEmployeeId, Name = DefaultName };

        Assert.Throws<ArgumentException>(() =>
            employeeService.SaveEmployee(targetEmployee, null, ValidHiringDateOffset, ValidSalaryText));
    }
    [Fact]
    public void SaveEmployee_ThrowsArgumentException_WhenHiringDateIsNull()
    {
        var employeeService = CreateTestService();
        var targetEmployee = new Employee { Id = TargetEmployeeId, Name = DefaultName };

        Assert.Throws<ArgumentException>(() =>
            employeeService.SaveEmployee(targetEmployee, ValidBirthdayOffset, null, ValidSalaryText));
    }

    [Fact]
    public void SaveEmployee_ThrowsArgumentException_WhenBirthdayIsInFuture()
    {
        var employeeService = CreateTestService();
        var targetEmployee = new Employee { Id = TargetEmployeeId, Name = DefaultName };

        Assert.Throws<ArgumentException>(() =>
            employeeService.SaveEmployee(targetEmployee, FutureDateOffset, ValidHiringDateOffset, ValidSalaryText));
    }

    [Fact]
    public void SaveEmployee_CallsAddEmployee_WhenEmployeeIdIsZero()
    {
        var employeeDataSource = new Mock<IEmployeeRepository>();
        var employeeService = CreateTestService(employeeDataSource: employeeDataSource);

        var newEmployee = new Employee
        {
            Id = 0,
            Name = DefaultName,
            Role = EmployeeRole.Pilot
        };

        employeeDataSource.Setup(employeeSource => employeeSource.AddEmployee(It.IsAny<Employee>())).Returns(TargetEmployeeId);

        employeeService.SaveEmployee(
            newEmployee,
            ValidBirthdayOffset,
            ValidHiringDateOffset,
            ValidSalaryText);

        employeeDataSource.Verify(employeeSource => employeeSource.AddEmployee(It.IsAny<Employee>()), Times.Once);
    }

    [Fact]
    public void SaveEmployee_CallsUpdateEmployee_WhenEmployeeIdIsNotZero()
    {
        var employeeDataSource = new Mock<IEmployeeRepository>();

        var existingEmployee = new Employee
        {
            Id = TargetEmployeeId,
            Name = DefaultName,
            Role = EmployeeRole.Pilot
        };

        employeeDataSource.Setup(employeeSource => employeeSource.GetEmployeeById(TargetEmployeeId)).Returns(existingEmployee);

        var employeeService = CreateTestService(employeeDataSource: employeeDataSource);

        employeeService.SaveEmployee(
            existingEmployee,
            ValidBirthdayOffset,
            ValidHiringDateOffset,
            ValidSalaryText);

        employeeDataSource.Verify(employeeSource => employeeSource.UpdateEmployee(It.IsAny<Employee>()), Times.Once);
    }

    [Fact]
    public void DeleteWithAssignments_ThrowsArgumentException_WhenIdIsZero()
    {
        var employeeService = CreateTestService();
        Assert.Throws<ArgumentException>(() => employeeService.DeleteWithAssignments(InvalidEmployeeId));
    }

    [Fact]
    public void DeleteWithAssignments_ThrowsArgumentException_WhenIdIsNegative()
    {
        var employeeService = CreateTestService();
        Assert.Throws<ArgumentException>(() => employeeService.DeleteWithAssignments(-1));
    }
}
