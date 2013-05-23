using System;
using System.Collections.Generic;
using TeamKeep.Models;

namespace TeamKeep.Services
{
    public class OrchestrationService
    {
        private readonly TeamService _teamService = new TeamService();
        private readonly GameService _gameService = new GameService();
        private readonly PlayerService _playerService = new PlayerService();

        public Team CreateSampleTeam(string teamName, User creator)
        {
            var sampleTeam = new Team { Name = teamName };
            sampleTeam = _teamService.AddTeam(sampleTeam, creator);
            
            // Add a sample season with games
            var season = new Season {TeamId = sampleTeam.Id, Name = DateTime.Now.Year + " Season", Order = 0};
            season = _gameService.AddSeason(season);

            var games = new List<Game>
            {
                new Game
                {
                    Date = DateTime.Now.AddDays(-7),
                    SeasonId = season.Id,
                    HomeTeamId = sampleTeam.Id,
                    OpponentName = "The A Team",
                    ScoredPoints = 5,
                    AllowedPoints = 2
                }, 
                new Game
                {
                    Date = DateTime.Now.AddDays(-2),
                    SeasonId = season.Id,
                    HomeTeamId = sampleTeam.Id,
                    OpponentName = "Electric Dream Machine",
                    ScoredPoints = 6,
                    AllowedPoints = 3
                }, 
                new Game
                {
                    Date = DateTime.Now.AddDays(3),
                    SeasonId = season.Id,
                    HomeTeamId = sampleTeam.Id,
                    OpponentName = "Rockets"
                }
            };

            foreach (var game in games)
            {
                _gameService.AddGame(game);
            }

            // Add a sample player grouping and roster
            var playerGroup = new PlayerGroup {TeamId = sampleTeam.Id, Name = "Active Players", Order = 0};
            playerGroup = _playerService.AddPlayerGroup(playerGroup);

            var players = new List<Player>
            {
                new Player
                    {
                        GroupId = playerGroup.Id,
                        LastName = "Teamkeeper",
                        FirstName = "Alice",
                        Email = "info@teamkeep.com",
                        Phone = "763-555-1234",
                        Position = "First base"
                    },
                new Player
                    {
                        GroupId = playerGroup.Id,
                        LastName = "Miller",
                        FirstName = "Thomas",
                        Email = "tom@teamkeep.com",
                        Phone = "952-555-9876",
                        Position = "Second base"
                    },
                new Player
                    {
                        GroupId = playerGroup.Id,
                        LastName = "Williams",
                        FirstName = "Zoe",
                        Email = "zoe@teamkeep.com",
                        Phone = "282-555-5463",
                        Position = "Third base"
                    }
            };

            foreach (var player in players)
            {
                _playerService.AddPlayer(player);
            }

            return sampleTeam;
        }
    }
}
