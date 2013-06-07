
using System.Collections.Generic;

namespace TeamKeep.Models
{
    public class TeamUpdate
    {
        public List<Season> Seasons { get; set; } 
        public List<Game> Events { get; set; }

        public List<PlayerGroup> PlayerGroups { get; set; } 
        public List<Player> Players { get; set; } 
    }
}