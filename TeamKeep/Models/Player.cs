using System.Collections.Generic;
using TeamKeep.Models.DataModels;

namespace TeamKeep.Models
{
    public class Player : PlayerData
    {
        public Player()
        {
        }

        public Player(PlayerData data)
        {
            Id = data.Id;
            LastName = data.LastName;
            FirstName = data.FirstName;
            Email = data.Email;
            Phone = data.Phone;
            GroupId = data.GroupId;
        }

        public List<AvailabilityData> Availability { get; set; }
    }
}