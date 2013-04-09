using TeamKeep.Models.DataModels;

namespace TeamKeep.Models
{
    public class AvailabilityRequest
    {
        public AvailabilityData Data { get; set; }
        public Game Event { get; set; }
        public string Email { get; set; }
        public string TeamName { get; set; }
    }
}