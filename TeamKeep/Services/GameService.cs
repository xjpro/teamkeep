using System.Linq;
using TeamKeep.Models;
using TeamKeep.Models.DataModels;
using System;
using System.Collections.Generic;
using TeamKeep.Models.ServiceResponses;

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
                game.Duties = entities.GameDutyDatas.Where(x => x.GameId == gameId).Select(x => new EventDuty 
                { 
                    Id = x.Id, 
                    EventId = x.GameId, 
                    PlayerId = x.PlayerId,
                    Name = x.Name 
                }).ToList();

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
                game.Location = gameLocationData;
                game.Duties = new List<EventDuty>();

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
            // Remove availabilities
            foreach (var data in entities.AvailabilityDatas.Where(x => x.EventId == gameId))
            {
                entities.DeleteObject(data);
            }

            // Remove duties
            foreach (var data in entities.GameDutyDatas.Where(x => x.GameId == gameId))
            {
                entities.DeleteObject(data);
            }

            entities.SaveChanges();

            entities.GameLocationDatas.DeleteObject(entities.GameLocationDatas.Single(x => x.GameId == gameId));
            entities.GameDatas.DeleteObject(entities.GameDatas.Single(x => x.Id == gameId));

            if (saveChanges) entities.SaveChanges();
        }

        public Game UpdateGate(Game teamEvent)
        {
            using (var entities = Database.GetEntities())
            {
                var gameData = entities.GameDatas.Single(x => x.Id == teamEvent.Id);

                if (gameData.SeasonId != teamEvent.SeasonId)
                {
                    var newSeasonId = entities.SeasonDatas.Single(x => x.Id == teamEvent.SeasonId).TeamId;
                    if (newSeasonId != teamEvent.HomeTeamId)
                    {
                        throw new Exception("Cannot move events between teams");
                    }
                }

                gameData.SeasonId = teamEvent.SeasonId;
                gameData.Type = teamEvent.Type;
                gameData.ScoredPoints = teamEvent.ScoredPoints;
                gameData.AllowedPoints = teamEvent.AllowedPoints;
                gameData.TiePoints = teamEvent.TiePoints;
                gameData.OpponentName = teamEvent.OpponentName;

                if (string.IsNullOrWhiteSpace(teamEvent.DateTime))
                {
                    gameData.Date = null;
                }
                else
                {
                    DateTime parsedDateTime;
                    if (DateTime.TryParse(teamEvent.DateTime, out parsedDateTime))
                    {
                        gameData.Date = parsedDateTime;
                    }
                }

                if (teamEvent.Location != null)
                {
                    var gameLocationData = entities.GameLocationDatas.Single(x => x.GameId == teamEvent.Id);
                    gameLocationData.Description = teamEvent.Location.Description;
                    gameLocationData.Link = teamEvent.Location.Link;
                    gameLocationData.Street = teamEvent.Location.Street;
                    gameLocationData.City = teamEvent.Location.City;
                    gameLocationData.Postal = teamEvent.Location.Postal;
                    gameLocationData.InternalLocation = teamEvent.Location.InternalLocation;
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
                int teamId = seasonData.TeamId ?? -1;

                entities.SeasonDatas.DeleteObject(seasonData);
                entities.SaveChanges();

                // Ensure proper order
                short order = 0;
                foreach (var season in entities.SeasonDatas
                    .Where(x => x.TeamId == teamId).OrderBy(x => x.Order).ToList())
                {
                    season.Order = order;
                    order++;
                }
                entities.SaveChanges();
            }
        }

        public EventDuty AddEventDuty(EventDuty duty)
        {
            using (var entities = Database.GetEntities())
            {
                var dutyData = new GameDutyData
                {
                    GameId = duty.EventId,
                    PlayerId = (duty.PlayerId > 0) ? duty.PlayerId : 0,
                    Name = duty.Name
                };
                entities.GameDutyDatas.AddObject(dutyData);
                entities.SaveChanges();

                duty.Id = dutyData.Id;
                return duty;
            }
        }

        public EventDuty GetEventDuty(int dutyId)
        {
            using (var entities = Database.GetEntities())
            {
                return new EventDuty(entities.GameDutyDatas.Single(x => x.Id == dutyId));
            }    
        }

        public void UpdateEventDuty(EventDuty duty)
        {
            using (var entities = Database.GetEntities())
            {
                var dutyData = entities.GameDutyDatas.Single(x => x.Id == duty.Id);
                dutyData.PlayerId = duty.PlayerId;
                dutyData.Name = duty.Name;
                entities.SaveChanges();
            }
        }

        public void RemoveEventDuty(int dutyId)
        {
            using (var entities = Database.GetEntities())
            {
                entities.GameDutyDatas.DeleteObject(entities.GameDutyDatas.Single(x => x.Id == dutyId));
                entities.SaveChanges();
            }
        }

        public EmailConfirmationsServiceResponse SendConfirmationEmails(int gameId, List<int> playerIds)
        {
            var game = GetGame(gameId);
            if (game == null) return new EmailConfirmationsServiceResponse {Error = true, Message = "Event not found"};
            if (game.Date == null) return new EmailConfirmationsServiceResponse { Error = true, Message = "Cannot send confirmations for an event with no date" };
            // TODO we also need to check with time zones in mind!
            if (game.Date.Value.CompareTo(DateTime.Now) <= 0) return new EmailConfirmationsServiceResponse { Error = true, Message = "Event has already past" }; 

            var emailService = new EmailService { AutomaticallySend = false };
            var alreadySentEmails = new List<string>();
            var updatedAvailabilites = new List<Availability>();

            using (var entities = Database.GetEntities())
            {
                var playerDatas = playerIds.Select(playerId => entities.PlayerDatas.Single(x => x.Id == playerId)).ToList();
                foreach (var playerData in playerDatas)
                {
                    // Preliminary filtering 
                    if (!EmailService.IsValidEmail(playerData.Email)) continue;
                    if (entities.PlayerGroupDatas.Single(x => x.Id == playerData.GroupId).TeamId != game.HomeTeamId) continue; // Player doesn't belong to this team, what?
                    // End filtering

                    // Retrieve availability data
                    var abData = entities.AvailabilityDatas.SingleOrDefault(x => x.EventId == gameId && x.PlayerId == playerData.Id);
                    if (abData != null && abData.EmailSent != null) continue; // Email was already sent to this player ID

                    if (abData == null) // Generate an availability entry for them
                    {
                        abData = new AvailabilityData { EventId = gameId, PlayerId = playerData.Id };
                        entities.AvailabilityDatas.AddObject(abData);
                    }

                    if (alreadySentEmails.Contains(playerData.Email.ToLower())) // An email was already sent to this address
                    {
                        if (abData.EmailSent == null)
                        {
                            abData.EmailSent = DateTime.Now; // Prevents double sending on a second try
                            updatedAvailabilites.Add(new Availability(abData));
                            entities.SaveChanges();
                        }
                        continue; 
                    }

                    // Okay, we've made it past the filtering

                    abData.Token = AuthToken.GenerateKey(playerData.Email); // TODO this might be accidently sent to client BAD
                    abData.EmailSent = DateTime.Now;
                    updatedAvailabilites.Add(new Availability(abData));
                    entities.SaveChanges();
                    
                    var abRequest = new AvailabilityRequest
                    {
                        Data = abData,
                        Event = game,
                        Email = playerData.Email,
                        TeamName = entities.TeamDatas.Single(x => x.Id == game.HomeTeamId).Name
                    };

                    // Send out the email!
                    emailService.EmailAvailability(abRequest);
                    alreadySentEmails.Add(playerData.Email.ToLower());
                }
            }

            emailService.SendQueuedMessages();
            return new EmailConfirmationsServiceResponse { UpdatedAvailabilities = updatedAvailabilites };
        }

    }
}