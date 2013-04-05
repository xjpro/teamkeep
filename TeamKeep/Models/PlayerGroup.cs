using System.Collections.Generic;
using TeamKeep.Models.DataModels;

namespace TeamKeep.Models
{
    public class PlayerGroup : PlayerGroupData
    {
        public List<Player> Players { get; set; }
    }
}