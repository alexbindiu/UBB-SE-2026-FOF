using System;
using System.Collections.Generic;
using TicketSellingModule.Domain;
using TicketSellingModule.Repo;

namespace TicketSellingModule.Service
{
    public class CompanyService
    {
        private readonly CompanyRepo _companyRepo = new CompanyRepo();
        
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