using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TicketSellingModule.Domain;
using TicketSellingModule.Repo;
using TicketSellingModule.Service;

namespace TicketSellingModule.ViewModel
{
    public class CompanyViewModel
    {
        private readonly CompanyService _companyService;
        private readonly AirportService _airportService;
        private readonly FlightRouteService _flightRouteService;
        
        public ObservableCollection<Company> CompaniesList { get; set; }
        public ObservableCollection<Airport> AirportsList { get; set; }
        public ObservableCollection<Flight> CompanyFlightsList { get; set; }

        public CompanyViewModel()
        {
            _companyService = new CompanyService(new CompanyRepo());
            _airportService = new AirportService(new AirportRepo());
            _flightRouteService = new FlightRouteService(new FlightRepo(), new RouteRepo());

            CompaniesList = new ObservableCollection<Company>();
            AirportsList = new ObservableCollection<Airport>();
            CompanyFlightsList = new ObservableCollection<Flight>();
        }

        public List<Company> GetAllCompanies()
        {
            var companies = _companyService.GetAll();
            
            CompaniesList.Clear();
            foreach (var company in companies)
            {
                CompaniesList.Add(company);
            }
            
            return companies;
        }

        public Company GetCompanyById(int companyId)
        {
            return _companyService.GetCompanyById(companyId);
        }
        
        public List<Airport> GetAllAirports()
        {
            var airports = _airportService.GetAll();
            
            AirportsList.Clear();
            foreach (var airport in airports)
            {
                AirportsList.Add(airport);
            }
            
            return airports;
        }

        public int AddAirport(string airportCode, string city, string name)
        {
            int newId = _airportService.Add(airportCode, name, city);
            
            GetAllAirports(); 
            return newId;
        }
        
        public int AddFlight(string flightNumber, int companyId, string routeType, int airportId, 
                             int capacity, TimeOnly departureTime, TimeOnly arrivalTime, 
                             int recurrenceInterval, DateTime startDate, DateTime endDate, 
                             int runwayId, int gateId)
        {
            int newRouteId = _flightRouteService.Add(
                companyId, airportId, routeType, recurrenceInterval, 
                startDate, endDate, departureTime, arrivalTime, 
                capacity, flightNumber, runwayId, gateId
            );
            
            GetCompanyFlights(companyId);

            return newRouteId;
        }

        public List<Flight> GetCompanyFlights(int companyId)
        {
            var allRoutes = _flightRouteService.GetAllRoutes();
            var companyRouteIds = allRoutes.Where(r => r.CompanyId == companyId).Select(r => r.Id).ToList();

            var allFlights = _flightRouteService.GetAllFlights();
            var companyFlights = allFlights.Where(f => companyRouteIds.Contains(f.RouteId)).ToList();

            CompanyFlightsList.Clear();
            foreach (var flight in companyFlights)
            {
                CompanyFlightsList.Add(flight);
            }

            return companyFlights;
        }

        public void DeleteFlight(int flightId, int currentCompanyId)
        {
            _flightRouteService.DeleteFlight(flightId);
            
            GetCompanyFlights(currentCompanyId); 
        }
    }
}