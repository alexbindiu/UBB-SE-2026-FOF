using TicketSellingModule.Data.Repositories.Interfaces;
using TicketSellingModule.Data.Services.Interfaces;

namespace TicketSellingModule.Data.Services
{
    public class GateService : IGateService
    {
        private readonly IGateRepo gateRepo;
        private readonly IFlightRepo flightRepo;
        public GateService(IGateRepo gateRepo, IFlightRepo flightRepo)
        {
            this.gateRepo = gateRepo;
            this.flightRepo = flightRepo;
        }

        public List<Gate> GetAll()
        {
            return gateRepo.GetAllGates();
        }

        public Gate GetById(int id)
        {
            if (id <= 0)
            {
                return null;
            }
            return gateRepo.GetGateById(id);
        }

        public int Add(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Gate name cannot be empty.");
            }

            Gate newGate = new Gate
            {
                Name = name
            };

            return gateRepo.AddGate(newGate);
        }

        public void Update(int id, string? newName = null)
        {
            var existingGate = gateRepo.GetGateById(id);
            if (existingGate == null)
            {
                return;
            }

            if (newName != null)
            {
                if (string.IsNullOrWhiteSpace(newName))
                {
                    throw new ArgumentException("New gate name cannot be empty.");
                }
                existingGate.Name = newName;
            }

            gateRepo.UpdateGate(existingGate);
        }

        public void Delete(int id)
        {
            if (id > 0)
            {
                gateRepo.DeleteGate(id);
            }
        }

        public void SaveGate(int id, string name)
        {
            if (id == 0)
            {
                Add(name);
            }
            else
            {
                Update(id, name);
            }
        }

        public bool HasFlights(int gateId)
        {
            return flightRepo.GetFlightsByGate(gateId).Any();
        }
    }
}