
namespace TicketSellingModule.Data.Services
{
    public class CompanyService
    {
        private readonly CompanyRepo _companyRepo;
        private readonly FlightRouteService _flightRouteService;
        public CompanyService (CompanyRepo companyRepo, FlightRouteService flightRouteService)
        {
            _companyRepo = companyRepo;
            _flightRouteService = flightRouteService;
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
            var company = _companyRepo.GetCompanyById(companyId);
            string prefix = "FL";

            if (company != null && !string.IsNullOrEmpty(company.Name))
            {
                string[] words = company.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (words.Length >= 2)
                    prefix = (words[0][0].ToString() + words[1][0].ToString()).ToUpper();
                else if (company.Name.Length >= 2)
                    prefix = company.Name.Substring(0, 2).ToUpper();
            }

            var existingFlights = _flightRouteService.GetFlightsByCompany(companyId);

            int nextNumber = 1000;

            if (existingFlights != null && existingFlights.Any())
            {
                var maxNumber = existingFlights
                    .Select(f => {
                        if (string.IsNullOrEmpty(f.FlightNumber) || !f.FlightNumber.Contains("-"))
                            return 0;

                        string parts = f.FlightNumber.Split('-').Last();
                        return int.TryParse(parts, out int val) ? val : 0;
                    })
                    .DefaultIfEmpty(0) 
                    .Max();

                if (maxNumber >= 1000)
                {
                    nextNumber = maxNumber + 1;
                }
            }

            return $"{prefix}-{nextNumber}";
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