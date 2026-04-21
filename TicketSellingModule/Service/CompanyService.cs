using System;
using System.Collections.Generic;
using TicketSellingModule.Domain;
using TicketSellingModule.Repo;

namespace TicketSellingModule.Service
{
    public class CompanyService
    {
        private readonly CompanyRepo _companyRepo;
        public CompanyService (CompanyRepo companyRepo)
        {
            _companyRepo = companyRepo;
        }
        
        public List<Company> GetAll()
        {
            return _companyRepo.GetAllCompanies();
        }

        public Company GetCompanyById(int id)
        {
            if (id<=0) 
                return null;
            return _companyRepo.GetCompanyById(id);
        }
        
        public int Add(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Company name cannot be empty.");

            Company newCompany = new Company
            {
                Name = name
            };

            return _companyRepo.AddNewCompany(newCompany);
        }

        public string GenerateFlightCode(int companyId)
        {
            var company = GetCompanyById(companyId);
            string prefix = "FL";
            if (company != null && !string.IsNullOrEmpty(company.Name))
            {
                string[] words = company.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                prefix = words.Length >= 2
                    ? (words[0][0].ToString() + words[1][0].ToString()).ToUpper()
                    : company.Name.Substring(0, Math.Min(2, company.Name.Length)).ToUpper();
            }
            return $"{prefix}-{new Random().Next(1000, 9999)}";
        }

        public void Update(int id, string? newName = null)
        {
            var existingCompany = _companyRepo.GetCompanyById(id);
            if (existingCompany == null) 
                return;

            if (newName!=null) 
            {
                if (string.IsNullOrWhiteSpace(newName))
                    throw new ArgumentException("New company name cannot be empty.");
                existingCompany.Name = newName;
            }

            _companyRepo.UpdateCompany(existingCompany);
        }
        
        public void Delete(int id)
        {
            if (id>0)
                _companyRepo.DeleteCompany(id);
        }
    }
}