using System;
using System.Collections.Generic;
using TicketSellingModule.Domain;
using TicketSellingModule.Repo;

namespace TicketSellingModule.Service
{
    public class GateService
    {
        private readonly GateRepo _gateRepo;
        public GateService (GateRepo gateRepo)
        {
            _gateRepo = gateRepo;
        }

        public List<Gate> GetAll()
        {
            return _gateRepo.GetAllGates();
        }

        public Gate GetById(int id)
        {
            if (id<=0) 
                return null;
            return _gateRepo.GetGateById(id);
        }

        public int Add(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Gate name cannot be empty.");

            Gate newGate = new Gate
            {
                Name = name
            };

            return _gateRepo.AddGate(newGate);
        }

        public void Update(int id, string? newName = null)
        {
            var existingGate = _gateRepo.GetGateById(id);
            if (existingGate == null) return;

            if (newName != null)
            {
                if (string.IsNullOrWhiteSpace(newName))
                    throw new ArgumentException("New gate name cannot be empty.");
                existingGate.Name = newName;
            }

            _gateRepo.UpdateGate(existingGate);
        }

        public void Delete(int id)
        {
            if (id>0)
                _gateRepo.DeleteGate(id);
        }
    }
}