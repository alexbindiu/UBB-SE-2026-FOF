namespace TicketSellingModule.Data.Services
{
    public class CompanyService(
        ICompanyRepository companyRepository,
        IFlightRouteService flightRouteService) : ICompanyService
    {
        private const int StartingFlightSequenceNumber = 1000;
        private const string DefaultFlightIdentificationPrefix = "FL";
        private const char FlightCodeDelimiter = '-';
        private const int RequiredWordsForInitials = 2;
        private const int MinimumPrefixLength = 2;

        public List<Company> GetAllCompanies()
        {
            return companyRepository.GetAllCompanies();
        }

        public Company? GetCompanyById(int companyId)
        {
            if (companyId <= 0)
            {
                return null;
            }

            return companyRepository.GetCompanyById(companyId);
        }

        public int AddCompany(string companyName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
            {
                throw new ArgumentException("The company name cannot be empty.");
            }

            Company newCompany = new Company
            {
                Name = companyName
            };

            return companyRepository.AddCompany(newCompany);
        }

        public string GenerateFlightCodeUsingCompanyId(int companyId)
        {
            Company? company = companyRepository.GetCompanyById(companyId);
            string characterPrefix = this.DetermineFlightPrefix(company);

            List<Flight> existingFlights = flightRouteService.GetFlightsByCompanyId(companyId);
            int nextSequenceNumber = this.CalculateNextAvailableFlightNumber(existingFlights);

            return $"{characterPrefix}{FlightCodeDelimiter}{nextSequenceNumber}";
        }

        private string DetermineFlightPrefix(Company? company)
        {
            if (company == null || string.IsNullOrWhiteSpace(company.Name))
            {
                return DefaultFlightIdentificationPrefix;
            }

            string[] nameWords = company.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (nameWords.Length >= RequiredWordsForInitials)
            {
                string firstInitial = nameWords[0][0].ToString();
                string secondInitial = nameWords[1][0].ToString();
                return (firstInitial + secondInitial).ToUpper();
            }

            if (company.Name.Length >= MinimumPrefixLength)
            {
                return company.Name.Substring(0, MinimumPrefixLength).ToUpper();
            }

            return company.Name.ToUpper();
        }

        private int CalculateNextAvailableFlightNumber(List<Flight> existingFlights)
        {
            int maxFlightNumberFound = 0;

            if (existingFlights == null)
            {
                return StartingFlightSequenceNumber;
            }

            foreach (Flight flight in existingFlights)
            {
                int extractedNumber = this.ExtractNumericPartFromFlightCode(flight.FlightNumber);

                if (extractedNumber > maxFlightNumberFound)
                {
                    maxFlightNumberFound = extractedNumber;
                }
            }

            if (maxFlightNumberFound >= StartingFlightSequenceNumber)
            {
                return maxFlightNumberFound + 1;
            }

            return StartingFlightSequenceNumber;
        }

        private int ExtractNumericPartFromFlightCode(string? flightNumber)
        {
            if (string.IsNullOrEmpty(flightNumber) || !flightNumber.Contains(FlightCodeDelimiter))
            {
                return 0;
            }

            string[] codeParts = flightNumber.Split(FlightCodeDelimiter);
            string lastSegment = codeParts[codeParts.Length - 1];

            if (int.TryParse(lastSegment, out int parsedFlightNumber))
            {
                return parsedFlightNumber;
            }

            return 0;
        }

        public void UpdateCompany(int companyId, string? updatedName = null)
        {
            Company? existingCompany = companyRepository.GetCompanyById(companyId);

            if (existingCompany == null)
            {
                return;
            }

            if (updatedName != null)
            {
                if (string.IsNullOrWhiteSpace(updatedName))
                {
                    throw new ArgumentException("The new company name cannot be empty.");
                }

                existingCompany.Name = updatedName;
            }

            companyRepository.UpdateCompany(existingCompany);
        }

        public void DeleteCompanyUsingId(int companyId)
        {
            if (companyId > 0)
            {
                companyRepository.DeleteCompanyUsingId(companyId);
            }
        }
    }
}