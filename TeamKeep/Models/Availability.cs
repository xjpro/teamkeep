using System.Web.Script.Serialization;
using TeamKeep.Models.DataModels;

namespace TeamKeep.Models
{
    public class Availability : AvailabilityData
    {
        public Availability()
        {
        }

        public Availability(AvailabilityData data)
        {
            EventId = data.EventId;
            PlayerId = data.PlayerId;
            EmailSent = data.EmailSent;
            Token = data.Token;
            AdminStatus = data.AdminStatus;
            RepliedStatus = data.RepliedStatus;
        }

        [ScriptIgnore]
        public new string Token { get; set; }
    }
}