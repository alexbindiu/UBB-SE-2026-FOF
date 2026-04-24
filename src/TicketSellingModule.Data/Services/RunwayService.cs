namespace TicketSellingModule.Data.Services
{
    public class RunwayService(
        IRunwayRepository runwayRepository,
        IFlightRepository flightRepository) : IRunwayService
    {
        public List<Runway> GetAllRunways()
        {
            return runwayRepository.GetAllRunways();
        }

        public Runway? GetRunwayById(int runwayId)
        {
            if (runwayId <= 0)
            {
                return null;
            }
            return runwayRepository.GetRunwayById(runwayId);
        }

        public int AddRunway(string runwayName, int handleTime)
        {
            if (string.IsNullOrWhiteSpace(runwayName))
            {
                throw new ArgumentException("The runway name cannot be empty.");
            }

            if (handleTime <= 0)
            {
                throw new ArgumentException("The handle time must be a positive number greater than zero.");
            }

            Runway newRunway = new Runway
            {
                Name = runwayName,
                HandleTime = handleTime
            };

            return runwayRepository.AddRunway(newRunway);
        }

        public void UpdateRunway(int runwayId, string? newName = null, int? newHandleTime = null)
        {
            Runway? existingRunway = runwayRepository.GetRunwayById(runwayId);

            if (existingRunway == null)
            {
                throw new InvalidOperationException($"Runway with Id {runwayId} does not exist.");
            }

            if (newName != null)
            {
                if (string.IsNullOrWhiteSpace(newName))
                {
                    throw new ArgumentException("The new runway name cannot be empty.");
                }

                existingRunway.Name = newName;
            }

            if (newHandleTime != null)
            {
                if (newHandleTime <= 0)
                {
                    throw new ArgumentException("The handle time must be a positive number greater than zero.");
                }

                existingRunway.HandleTime = newHandleTime.Value;
            }

            runwayRepository.UpdateRunway(existingRunway);
        }

        public void DeleteRunwayUsingId(int runwayId)
        {
            Runway? runway = runwayRepository.GetRunwayById(runwayId);

            if (runway == null)
            {
                throw new InvalidOperationException($"Runway with Id {runwayId} does not exist.");
            }

            runwayRepository.DeleteRunwayUsingId(runwayId);
        }

        public void SaveRunway(int runwayId, string runwayName, string handleTimeText)
        {
            if (!int.TryParse(handleTimeText, out int handleTime) || handleTime <= 0)
            {
                throw new ArgumentException("Handle time must be a valid positive numeric value.");
            }

            if (runwayId == 0)
            {
                this.AddRunway(runwayName, handleTime);
            }
            else
            {
                this.UpdateRunway(runwayId, runwayName, handleTime);
            }
        }

        public bool HasFlights(int runwayId)
        {
            List<Flight> associatedFlights = flightRepository.GetFlightsByRunwayId(runwayId);

            return associatedFlights.Count > 0;
        }
    }
}