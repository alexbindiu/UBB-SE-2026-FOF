using TicketSellingModule.Data.Services.Interfaces;

namespace TicketSellingModule.Data.Services
{
    public class RunwayService : IRunwayService
    {
        private readonly RunwayRepository runwayRepo;
        private readonly FlightRepository flightRepo;
        public RunwayService(RunwayRepository runwayRepo, FlightRepository flightRepo)
        {
            this.runwayRepo = runwayRepo;
            this.flightRepo = flightRepo;
        }

        public List<Runway> GetAll()
        {
            return runwayRepo.GetAllRunways();
        }

        public Runway GetById(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid runway ID.");
            }

            var runway = runwayRepo.GetRunwayById(id);
            if (runway == null)
            {
                throw new InvalidOperationException($"Runway with ID {id} does not exist.");
            }

            return runway;
        }

        public Runway? GetByIdSafe(int id)
        {
            if (id <= 0)
            {
                return null;
            }
            try
            {
                return runwayRepo.GetRunwayById(id);
            }
            catch
            {
                return null;
            }
        }

        public int Add(string name, int handleTime)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Runway name cannot be empty.");
            }

            if (handleTime <= 0)
            {
                throw new ArgumentException("HandleTime must be greater than zero.");
            }

            Runway newRunway = new Runway { Name = name, HandleTime = handleTime };
            return runwayRepo.AddRunway(newRunway);
        }

        public void Update(int id, string? newName = null, int? newHandleTime = null)
        {
            var existingRunway = runwayRepo.GetRunwayById(id);
            if (existingRunway == null)
            {
                throw new InvalidOperationException($"Runway with ID {id} does not exist.");
            }

            if (newName != null)
            {
                if (string.IsNullOrWhiteSpace(newName))
                {
                    throw new ArgumentException("New runway name cannot be empty.");
                }

                existingRunway.Name = newName;
            }

            if (newHandleTime != null)
            {
                if (newHandleTime <= 0)
                {
                    throw new ArgumentException("HandleTime must be greater than zero.");
                }

                existingRunway.HandleTime = newHandleTime.Value;
            }
            runwayRepo.UpdateRunway(existingRunway);
        }

        public void Delete(int id)
        {
            var runway = runwayRepo.GetRunwayById(id);
            if (runway == null)
            {
                throw new InvalidOperationException($"Runway with ID {id} does not exist.");
            }
            runwayRepo.DeleteRunwayUsingId(id);
        }

        public void SaveRunway(int id, string name, string handleTimeText)
        {
            if (!int.TryParse(handleTimeText, out int handleTime) || handleTime <= 0)
            {
                throw new ArgumentException("Handle Time must be a valid positive number.");
            }

            if (id == 0)
            {
                Add(name, handleTime);
            }
            else
            {
                Update(id, name, handleTime);
            }
        }

        public bool HasFlights(int runwayId)
        {
            return flightRepo.GetFlightsByRunwayId(runwayId).Any();
        }
    }
}