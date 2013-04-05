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
            SeasonId = data.SeasonId;
            HomeTeamId = data.HomeTeamId;
            AwayTeamId = data.AwayTeamId;
            ScoredPoints = data.ScoredPoints;
            AllowedPoints = data.AllowedPoints;
            TiePoints = data.TiePoints;
            OpponentName = data.OpponentName;
        }

        public string DateTime { get; set; }
        public GameLocationData Location { get; set; }
    }
}