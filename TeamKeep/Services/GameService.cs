using System.Linq;
using TeamKeep.Models;
using TeamKeep.Models.DataModels;
using System;
using System.Collections.Generic;

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

        public Game GetGame(int gameId)
        {
            using (var entities = Database.GetEntities())
            {
                var gameData = entities.GameDatas.Single(x => x.Id == gameId);
                var game = new Game(gameData);
                game.Location = entities.GameLocationDatas.Single(x => x.GameId == game.Id);
                return game;
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
                RemoveGame(entities, gameId);
            }
        }

        private void RemoveGame(DatabaseEntities entities, int gameId, bool saveChanges = true)
        {
            entities.GameDatas.DeleteObject(entities.GameDatas.Single(x => x.Id == gameId));
            entities.GameLocationDatas.DeleteObject(entities.GameLocationDatas.Single(x => x.GameId == gameId));
            foreach (var abData in entities.AvailabilityDatas.Where(x => x.EventId == gameId).ToList())
            {
                entities.DeleteObject(abData);
            }

            if (saveChanges)
            {
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

        public Season GetSeason(int seasonId)
        {
            using (var entities = Database.GetEntities())
            {
                var seasonData = entities.SeasonDatas.Single(x => x.Id == seasonId);
                return new Season(seasonData);
            }
        }

        public Season GetMostRecentSeason(Team team)
        {
            using (var entities = Database.GetEntities())
            {
                var lastSeason = entities.SeasonDatas.FirstOrDefault(x => x.TeamId == team.Id);
                return (lastSeason != null) ? new Season { Id = lastSeason.Id } : null;
            }
        }

        public Season AddSeason(Season season)
        {
            using (var entities = Database.GetEntities())
            {
                var order = entities.SeasonDatas.Count(x => x.TeamId == season.TeamId);

                var seasonData = new SeasonData
                {
                    TeamId = season.TeamId,
                    LeagueId = season.LeagueId,
                    Name = season.Name,
                    Order = (short)order
                };

                entities.SeasonDatas.AddObject(seasonData);
                entities.SaveChanges();

                season.Id = seasonData.Id;
                season.Games = new List<Game>();
                season.Order = seasonData.Order;
                return season;
            }
        }

        public Season UpdateSeason(Season season)
        {
            using (var entities = Database.GetEntities())
            {
                var seasonData = entities.SeasonDatas.Single(x => x.Id == season.Id);
                seasonData.Name = season.Name;

                if (seasonData.Order != season.Order) // A reordering
                {
                    // Swapping
                    var swapping = entities.SeasonDatas.FirstOrDefault(x => x.TeamId == season.TeamId && x.Order == season.Order);
                    if (swapping != null)
                    {
                        swapping.Order = seasonData.Order; // Give it the current order
                    }

                    // Matching
                    seasonData.Order = season.Order; // Give it the new order

                    entities.SaveChanges();

                    // Ensure proper ordering
                    short order = 0;
                    foreach (var orderedSeasonData in entities.SeasonDatas.Where(x => x.TeamId == season.TeamId).OrderBy(x => x.Order).ToList())
                    {
                        orderedSeasonData.Order = order;
                        order++;
                    }
                }

                entities.SaveChanges();

                return season;
            }
        }

        public void RemoveSeason(int seasonId)
        {
            using (var entities = Database.GetEntities())
            {
                var gameDatas = entities.GameDatas.Where(x => x.SeasonId == seasonId).ToList();
                foreach (var gameData in gameDatas)
                {
                    RemoveGame(entities, gameData.Id, false);
                }

                var seasonData = entities.SeasonDatas.Single(x => x.Id == seasonId);
                entities.SeasonDatas.DeleteObject(seasonData);
                entities.SaveChanges();

                // Ensure proper order
                short order = 0;
                foreach (var season in entities.SeasonDatas.OrderBy(x => x.Order).ToList())
                {
                    season.Order = order;
                    order++;
                }
                entities.SaveChanges();
            }
        }
    }
}