using System.Linq;
using TeamKeep.Models;
using TeamKeep.Models.DataModels;
using System;

namespace TeamKeep.Services
{
    public class GameService
    {
        public bool CanEditGame(int editorId, int gameId)
        {
            using (var entities = Database.GetEntities())
            {
                var gameData = entities.GameDatas.Single(x => x.Id == gameId);
                var teamOwnerUserIds = entities.TeamOwnerDatas.Where(x => x.TeamId == gameData.HomeTeamId).Select(x => x.UserId).ToList();
                return teamOwnerUserIds.Exists(x => x == editorId);
            }
        }

        public Game AddGame(Game game)
        {
            using (var entities = Database.GetEntities())
            {
                var gameData = new GameData
                {
                    SeasonId = game.SeasonId,
                    Date = game.Date,
                    HomeTeamId = game.HomeTeamId,
                    AwayTeamId = game.AwayTeamId,
                    ScoredPoints = game.ScoredPoints,
                    AllowedPoints = game.AllowedPoints,
                    TiePoints = game.TiePoints,
                    OpponentName = game.OpponentName
                };
                entities.GameDatas.AddObject(gameData);
                entities.SaveChanges();

                var gameLocationData = new GameLocationData { GameId = gameData.Id };
                if (game.Location != null)
                {
                    gameLocationData.Description = game.Location.Description;
                    gameLocationData.Link = game.Location.Link;
                    gameLocationData.Street = game.Location.Street;
                    gameLocationData.City = game.Location.City;
                    gameLocationData.Postal = game.Location.Postal;
                    gameLocationData.InternalLocation = game.Location.InternalLocation;
                }
                entities.GameLocationDatas.AddObject(gameLocationData);
                entities.SaveChanges();

                game.Id = gameData.Id;
                game.DateTime = (game.Date) != null ? ((DateTime)game.Date).ToString("MMM d, yyyy, h:mm tt") : null;
                game.Location = gameLocationData;

                return game;
            }
        }

        public void RemoveGame(int gameId)
        {
            using (var entities = Database.GetEntities())
            {
                entities.GameDatas.DeleteObject(entities.GameDatas.Single(x => x.Id == gameId));
                entities.GameLocationDatas.DeleteObject(entities.GameLocationDatas.Single(x => x.GameId == gameId));
                entities.SaveChanges();
            }
        }

        public Game UpdateGate(Game game)
        {
            using (var entities = Database.GetEntities())
            {
                var gameData = entities.GameDatas.Single(x => x.Id == game.Id);
                gameData.SeasonId = game.SeasonId;
                gameData.ScoredPoints = game.ScoredPoints;
                gameData.AllowedPoints = game.AllowedPoints;
                gameData.TiePoints = game.TiePoints;
                gameData.OpponentName = game.OpponentName;

                if (string.IsNullOrWhiteSpace(game.DateTime))
                {
                    gameData.Date = null;
                }
                else
                {
                    DateTime parsedDateTime;
                    if (DateTime.TryParse(game.DateTime, out parsedDateTime))
                    {
                        gameData.Date = parsedDateTime;
                    }
                }

                if (game.Location != null)
                {
                    var gameLocationData = entities.GameLocationDatas.Single(x => x.GameId == game.Id);
                    gameLocationData.Description = game.Location.Description;
                    gameLocationData.Link = game.Location.Link;
                    gameLocationData.Street = game.Location.Street;
                    gameLocationData.City = game.Location.City;
                    gameLocationData.Postal = game.Location.Postal;
                    gameLocationData.InternalLocation = game.Location.InternalLocation;
                }

                entities.SaveChanges();

                return new Game(gameData);
            }
        }
    }
}