﻿using TeamKeep.Models.DataModels;
using System.Collections.Generic;

namespace TeamKeep.Models
{
    public class Season : SeasonData
    {
        public Season()
        {
        }

        public Season(SeasonData data)
        {
            Id = data.Id;
            LeagueId = data.LeagueId;
            TeamId = data.TeamId;
            Name = data.Name;
            Order = data.Order;
        }

        public List<Game> Games { get; set; }
    }
}