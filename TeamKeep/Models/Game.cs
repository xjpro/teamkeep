﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;
using Teamkeep.Models.DataModels;

namespace Teamkeep.Models
{
    public class Game : GameData
    {
        public enum EventType
        {
            Game = 0,
            Practice = 1,
            Meeting = 2,
            Party = 3,
            Other = 99
        }

        public Game()
        {
        }

        public Game(GameData data)
        {
            Id = data.Id;
            Type = data.Type;
            Date = data.Date;
            SeasonId = data.SeasonId;
            HomeTeamId = data.HomeTeamId;
            AwayTeamId = data.AwayTeamId;
            ScoredPoints = data.ScoredPoints;
            AllowedPoints = data.AllowedPoints;
            TiePoints = data.TiePoints;
            OpponentName = data.OpponentName;
        }

        public string DateTime { get; set; }

        //http://msdn.microsoft.com/en-us/library/8kb3ddd4.aspx
        [ScriptIgnore]
        public string When
        {
            get
            {
                if(Date == null) return null;
                return ((DateTime)Date).ToString("ddd MMM d, yyyy h:mm tt");
            }
        }

        public GameLocationData Location { get; set; }
        [ScriptIgnore]
        public string Where
        {
            get
            {
                if (Location == null || 
                    (string.IsNullOrWhiteSpace(Location.Description) && 
                    string.IsNullOrWhiteSpace(Location.Street) && 
                    string.IsNullOrWhiteSpace(Location.City) && 
                    string.IsNullOrWhiteSpace(Location.Postal)))
                {
                    return null;
                }

                var address = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(Location.Description)) address.Append(Location.Description);
                if (!string.IsNullOrWhiteSpace(Location.Street)) address.Append(" " + Location.Street);
                if (!string.IsNullOrWhiteSpace(Location.City)) address.Append(" " + Location.City);
                if (!string.IsNullOrWhiteSpace(Location.Postal)) address.Append(" " + Location.Postal);
                //if (!string.IsNullOrWhiteSpace(Location.InternalLocation)) address.Append(", " + Location.InternalLocation);
                return address.ToString().Trim();
            }
        }

        public List<EventDuty> Duties { get; set; }
    }
}