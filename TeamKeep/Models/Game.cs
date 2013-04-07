using System;
using TeamKeep.Models.DataModels;

namespace TeamKeep.Models
{
    public class Game : GameData
    {
        public Game()
        {
        }

        public Game(GameData data)
        {
            Id = data.Id;
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
        public string When
        {
            get
            {
                if(Date == null) return null;
                return ((DateTime)Date).ToString("ddd MMM d, yyyy h:mmtt");
            }
        }

        public GameLocationData Location { get; set; }
        public string Where
        {
            get
            {
                if (Location == null) return null;
                return Location.Description + " " + Location.Street + " " + Location.City + " " + Location.Postal;
            }
        }
    }
}