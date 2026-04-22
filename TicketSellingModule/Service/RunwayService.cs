using System;
using System.Linq;
using System.Collections.Generic;
using TicketSellingModule.Domain;
using TicketSellingModule.Repo;

namespace TicketSellingModule.Service
{
    public class RunwayService
    {
        private readonly RunwayRepo _runwayRepo;
        private readonly FlightRepo _flightRepo;
        public RunwayService(RunwayRepo runwayRepo, FlightRepo flightRepo)
        {
            _runwayRepo = runwayRepo;
            _flightRepo = flightRepo;
        }

        public List<Runway> GetAll()
        {
            return _runwayRepo.GetAllRunways();
        }

        public Runway GetById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid runway ID.");

            var runway = _runwayRepo.GetRunwayById(id);
            if (runway == null)
                throw new InvalidOperationException($"Runway with ID {id} does not exist.");

            return runway;
        }

        public Runway? GetByIdSafe(int id)
        {
            if (id <= 0) return null;
            try { return _runwayRepo.GetRunwayById(id); }
            catch { return null; }
        }

        public int Add(string name, int handleTime)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Runway name cannot be empty.");
            if (handleTime <= 0)
                throw new ArgumentException("HandleTime must be greater than zero.");

            Runway newRunway = new Runway { Name = name, HandleTime = handleTime };
            return _runwayRepo.AddRunway(newRunway);
        }

        public void Update(int id, string? newName = null, int? newHandleTime = null)
        {
            var existingRunway = _runwayRepo.GetRunwayById(id);
            if (existingRunway == null)
                throw new InvalidOperationException($"Runway with ID {id} does not exist.");

            if (newName != null)
            {
                if (string.IsNullOrWhiteSpace(newName))
                    throw new ArgumentException("New runway name cannot be empty.");
                existingRunway.Name = newName;
            }
            
            if (newHandleTime != null)
            {
                if (newHandleTime <= 0)
                    throw new ArgumentException("HandleTime must be greater than zero.");
                existingRunway.HandleTime = newHandleTime.Value;
            }

            _runwayRepo.UpdateRunway(existingRunway);
        }

        public void Delete(int id)
        {
            var runway = _runwayRepo.GetRunwayById(id);
            if (runway == null)
                throw new InvalidOperationException($"Runway with ID {id} does not exist.");

            _runwayRepo.DeleteRunway(id);
        }

        public void SaveRunway(int id, string name, string handleTimeText)
        {
            if (!int.TryParse(handleTimeText, out int handleTime) || handleTime <= 0)
                throw new ArgumentException("Handle Time must be a valid positive number.");

            if (id == 0)
                Add(name, handleTime);
            else
                Update(id, name, handleTime);
        }

        public bool HasFlights(int runwayId)
        {
            return _flightRepo.GetFlightsByRunway(runwayId).Any();
        }
    }
}