namespace TicketSellingModule.Data.Services.Interfaces;

public interface ICompanyService
{
    List<Company> GetAllCompanies();
    Company? GetCompanyById(int id);
    int AddCompany(string name);
    string GenerateFlightCodeUsingCompanyId(int companyId);
    void UpdateCompany(int id, string? newName = null);
    void DeleteCompanyUsingId(int id);
    int ValidateFlightCreationInputs(int companyId, int airportId, string capacityText, int runwayId, int gateId);
}
