using System;
using System.Collections.Generic;
using System.Text;

namespace TicketSellingModule.Data.Repositories.Interfaces;

public interface ICompanyRepo
{
    List<Company> GetAllCompanies();
    Company GetCompanyById(int id);
    int AddNewCompany(Company company);
    void DeleteCompany(int id);
    void UpdateCompany(Company company);
}
