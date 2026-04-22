using System;
using System.Collections.Generic;
using System.Linq;
using TicketSellingModule.Domain;
using TicketSellingModule.Repo;

namespace TicketSellingModule.Service
{
    public class AirportService
    {
        private readonly AirportRepo _airportRepo;
        private readonly FlightRepo _flightRepo;
        public AirportService (AirportRepo airportRepo, FlightRepo flightRepo)
        {
            _airportRepo = airportRepo;
            _flightRepo = flightRepo;
        }

        public List<Airport> GetAll()
        {
            return _airportRepo.GetAllAirports();
        }

        public Airport GetById(int id)
        {
            if (id <= 0) return null;
            return _airportRepo.GetAirportById(id);
        }

        public int Add(string code, string namde, string city) 
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Airport code cannot be empty.");
            }
             if (string.IsNullOrWhiteSpace(namde))
            {
                throw new ArgumentException("Airport name cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(city))
            {
                throw new ArgumentException("City cannot be empty.");
            }


            Airport newAirport = new Airport
            {
                AirportCode = code,
                AirportName = namde,
                City = city
            };

            return _airportRepo.AddAirport(newAirport);
        }

        public void Update(int id, string? newCity = null, string? newName = null, string? newCode = null)
        {
            var existingAirport = _airportRepo.GetAirportById(id);
            if (existingAirport == null) return;

            if (newName != null) existingAirport.AirportName = newName;
            if (newCity != null) existingAirport.City = newCity;
            if (newCode != null) existingAirport.AirportCode = newCode;

            _airportRepo.UpdateAirport(existingAirport);
        }

        public void Delete(int id)
        {
            if (id > 0)
            {
                _airportRepo.DeleteAirport(id);
            }
        }

        public bool HasFlights(int airportId)
        {
            return _flightRepo.GetFlightsByAirport(airportId).Any();
        }
    }
}
