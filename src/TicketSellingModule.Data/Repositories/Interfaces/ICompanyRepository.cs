namespace TicketSellingModule.Data.Repositories.Interfaces;

public interface ICompanyRepository
{
    List<Company> GetAllCompanies();
    Company? GetCompanyById(int companyId);
    int AddCompany(Company newCompany);
    void DeleteCompanyUsingId(int comapnyId);
    void UpdateCompany(Company updatedCompany);
}
