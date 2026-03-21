using System;
using System.Collections.Generic;
using TicketSellingModule.Domain;
using TicketSellingModule.Repo;

namespace TicketSellingModule.Service
{
    public class AirportService
    {
        private readonly AirportRepo _airportRepo = new AirportRepo();

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

        public void Update(int id, string city)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(city)) return;

            Airport airportToUpdate = new Airport
            {
                Id = id,
                City = city
            };

            _airportRepo.UpdateAirport(airportToUpdate);
        }

        public void Delete(int id)
        {
            if (id > 0)
            {
                _airportRepo.DeleteAirport(id);
            }
        }
    }
}
