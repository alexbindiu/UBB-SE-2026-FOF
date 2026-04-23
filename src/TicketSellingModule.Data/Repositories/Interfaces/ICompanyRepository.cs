namespace TicketSellingModule.Data.Repositories.Interfaces;

public interface ICompanyRepository
{
    List<Company> GetAllCompanies();
    Company? GetCompanyById(int companyId);
    int AddNewCompany(Company newCompany);
    void DeleteCompanyUsingId(int comapnyId);
    void UpdateCompany(Company updatedCompany);
}
