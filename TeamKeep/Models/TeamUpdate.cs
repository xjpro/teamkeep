
using System.Collections.Generic;

namespace TeamKeep.Models
{
    public class TeamUpdate
    {
        public List<Game> Events { get; set; }
        public List<Player> Players { get; set; } 
    }
}