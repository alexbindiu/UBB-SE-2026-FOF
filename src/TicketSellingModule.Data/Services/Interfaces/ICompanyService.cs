using System;
using System.Collections.Generic;
using System.Text;

namespace TicketSellingModule.Data.Services.Interfaces;

public interface ICompanyService
{
    List<Company> GetAll();
    Company GetCompanyById(int id);
    int Add(string name);
    string GenerateFlightCode(int companyId);
    void Update(int id, string? newName = null);
    void Delete(int id);
}
