using System.Collections.Generic;
using Teamkeep.Models.DataModels;

namespace Teamkeep.Models
{
    public class PlayerGroup : PlayerGroupData
    {
        public List<Player> Players { get; set; }
    }
}