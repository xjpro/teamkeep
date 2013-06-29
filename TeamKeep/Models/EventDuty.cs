using TeamKeep.Models.DataModels;

namespace TeamKeep.Models
{
    public class EventDuty
    {
        public EventDuty()
        {
        }

        public EventDuty(GameDutyData data)
        {
            Id = data.Id;
            EventId = data.GameId;
            PlayerId = data.PlayerId;
            Name = data.Name;
        }

        public int Id { get; set; }
        public int EventId { get; set; }
        public int? PlayerId { get; set; }
        public string Name { get; set; }
    }
}