using TicketSellingModule.Data.Repositories.Interfaces;
using TicketSellingModule.Data.Services.Interfaces;

namespace TicketSellingModule.Data.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly CompanyRepository companyRepo;
        private readonly FlightRouteService flightRouteService;
        private const int InitialFlightNumber = 1000;
        private const string DefaultFlightPrefix = "FL";

        public CompanyService(CompanyRepository companyRepo, FlightRouteService flightRouteService)
        {
            this.companyRepo = companyRepo;
            this.flightRouteService = flightRouteService;
        }

        public List<Company> GetAll()
        {
            return companyRepo.GetAllCompanies();
        }

        public Company GetCompanyById(int id)
        {
            if (id <= 0)
            {
                return null;
            }
            return companyRepo.GetCompanyById(id);
        }

        public int Add(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Company name cannot be empty.");
            }

            Company newCompany = new Company
            {
                Name = name
            };

            return companyRepo.AddNewCompany(newCompany);
        }

        public string GenerateFlightCode(int companyId)
        {
            var company = companyRepo.GetCompanyById(companyId);
            string prefix = DefaultFlightPrefix;

            if (company != null && !string.IsNullOrEmpty(company.Name))
            {
                string[] words = company.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (words.Length >= 2)
                {
                    prefix = (words[0][0].ToString() + words[1][0].ToString()).ToUpper();
                }
                else if (company.Name.Length >= 2)
                {
                    prefix = company.Name.Substring(0, 2).ToUpper();
                }
            }

            var existingFlights = flightRouteService.GetFlightsByCompany(companyId);
            int nextNumber = InitialFlightNumber;

            if (existingFlights != null)
            {
                int maxNumber = 0;

                foreach (var f in existingFlights)
                {
                    if (!string.IsNullOrEmpty(f.FlightNumber) && f.FlightNumber.Contains("-"))
                    {
                        string[] parts = f.FlightNumber.Split('-');
                        string lastPart = parts[parts.Length - 1];

                        if (int.TryParse(lastPart, out int currentVal))
                        {
                            if (currentVal > maxNumber)
                            {
                                maxNumber = currentVal;
                            }
                        }
                    }
                }

                if (maxNumber >= InitialFlightNumber)
                {
                    nextNumber = maxNumber + 1;
                }
            }

            return $"{prefix}-{nextNumber}";
        }

        public void Update(int id, string? newName = null)
        {
            var existingCompany = companyRepo.GetCompanyById(id);
            if (existingCompany == null)
            {
                return;
            }

            if (newName != null)
            {
                if (string.IsNullOrWhiteSpace(newName))
                {
                    throw new ArgumentException("New company name cannot be empty.");
                }
                existingCompany.Name = newName;
            }

            companyRepo.UpdateCompany(existingCompany);
        }

        public void Delete(int id)
        {
            if (id > 0)
            {
                companyRepo.DeleteCompanyUsingId(id);
            }
        }
    }
}