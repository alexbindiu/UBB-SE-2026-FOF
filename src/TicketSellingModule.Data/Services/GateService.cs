namespace TicketSellingModule.Data.Services
{
    public class GateService(
        IGateRepository gateRepository,
        IFlightRepository flightRepository) : IGateService
    {
        public List<Gate> GetAllGates()
        {
            return gateRepository.GetAllGates();
        }

        public Gate? GetGateById(int gateId)
        {
            if (gateId <= 0)
            {
                return null;
            }

            return gateRepository.GetGateById(gateId);
        }

        public int Add(string gateName)
        {
            if (string.IsNullOrWhiteSpace(gateName))
            {
                throw new ArgumentException("The gate name cannot be empty.");
            }

            Gate newGate = new Gate
            {
                Name = gateName
            };

            return gateRepository.AddGate(newGate);
        }

        public void Update(int gateId, string? updatedGateName = null)
        {
            Gate? existingGate = gateRepository.GetGateById(gateId);

            if (existingGate == null)
            {
                return;
            }

            if (updatedGateName != null)
            {
                if (string.IsNullOrWhiteSpace(updatedGateName))
                {
                    throw new ArgumentException("The new gate name cannot be empty.");
                }

                existingGate.Name = updatedGateName;
            }

            gateRepository.UpdateGate(existingGate);
        }

        public void DeleteGateUsingId(int gateId)
        {
            if (gateId > 0)
            {
                gateRepository.DeleteGateUsingId(gateId);
            }
        }

        public void SaveGate(int gateId, string gateName)
        {
            if (gateId == 0)
            {
                this.Add(gateName);
            }
            else
            {
                this.Update(gateId, gateName);
            }
        }

        public bool HasFlights(int gateId)
        {
            List<Flight> associatedFlights = flightRepository.GetFlightsByGateId(gateId);

            return associatedFlights.Count > 0;
        }
    }
}