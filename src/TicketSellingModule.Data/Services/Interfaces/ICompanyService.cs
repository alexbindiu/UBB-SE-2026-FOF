namespace TicketSellingModule.Data.Services.Interfaces;

public interface ICompanyService
{
    List<Company> GetAllCompanies();
    Company? GetCompanyById(int id);
    int AddCompany(string companyName);
    string GenerateFlightCodeUsingCompanyId(int companyId);
    void UpdateCompany(int companyId, string? newName = null);
    void DeleteCompanyUsingId(int companyId);
    int ValidateFlightCreationInputs(int companyId, int airportId, string capacityText, int runwayId, int gateId);
}
