using System;
using System.Collections.Generic;
using TicketSellingModule.Domain;
using TicketSellingModule.Repo;

namespace TicketSellingModule.Service
{
    public class RunwayService
    {
        private readonly RunwayRepo _runwayRepo;
        public RunwayService(RunwayRepo runwayRepo)
        {
            _runwayRepo = runwayRepo;
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
    }
}