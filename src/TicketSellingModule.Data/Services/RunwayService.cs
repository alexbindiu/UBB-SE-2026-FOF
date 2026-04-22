namespace TicketSellingModule.Data.Services
{
    public class RunwayService
    {
        private readonly RunwayRepo runwayRepo;
        private readonly FlightRepo flightRepo;
        public RunwayService(RunwayRepo runwayRepo, FlightRepo flightRepo)
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
            runwayRepo.DeleteRunway(id);
        }

        public bool HasFlights(int runwayId)
        {
            return flightRepo.GetFlightsByRunway(runwayId).Any();
        }
    }
}