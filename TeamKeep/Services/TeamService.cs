using System.Linq;
using TeamKeep.Models;
using TeamKeep.Models.DataModels;
using System.Collections.Generic;
using System;
using TeamKeep.Models.ViewModels;

namespace TeamKeep.Services
{
    public class TeamService
    {
        public bool CanEdit(int teamId, int userId)
        {
            using (var entities = Database.GetEntities())
            {
                return entities.TeamOwnerDatas.Where(x => x.TeamId == teamId).Select(x => x.UserId).Any(ownerId => ownerId == userId);
            }
        }

        public Team GetTeam(int id)
        {
            using (var entities = Database.GetEntities())
            {
                var teamData = entities.TeamDatas.SingleOrDefault(x => x.Id == id);
                if (teamData == null) return null;

                var team = new Team(teamData);

                // Retrieve settings & privacy
                team.Settings = entities.TeamSettingsDatas.SingleOrDefault(x => x.TeamId == team.Id);
                team.Privacy = entities.TeamPrivacyDatas.Single(x => x.TeamId == team.Id);

                // Retrieve owners
                var teamOwnerUserIds = entities.TeamOwnerDatas.Where(x => x.TeamId == team.Id).Select(x => x.UserId).ToList();
                team.Owners = teamOwnerUserIds.Select(ownerId => entities.UserDatas.Single(x => x.Id == ownerId))
                    .Select(userData => new User(userData)).ToList();

                // Retrieve messages
                team.Messages = entities.MessageDatas.Where(x => x.TeamId == team.Id).Select(msgData => new Message
                {
                    Id = msgData.Id,
                    TeamId = msgData.TeamId,
                    Date = msgData.Date,
                    To = msgData.To,
                    Subject = msgData.Subject,
                    Content = msgData.Content
                }).ToList();

                // Retrieve player groups

                var playerGroupDatas = entities.PlayerGroupDatas.Where(x => x.TeamId == team.Id).OrderBy(x => x.Order)
                    .Select(groupData => new PlayerGroup
                    {
                        Id = groupData.Id,
                        TeamId = groupData.TeamId,
                        Name = groupData.Name,
                        Order = groupData.Order
                    }).ToList();

                foreach (var playerGroup in playerGroupDatas)
                {
                    var players = new List<Player>();

                    var playerDatas = entities.PlayerDatas.Where(x => x.GroupId == playerGroup.Id).ToList();
                    foreach (var player in playerDatas.Select(playerData => new Player(playerData)))
                    {
                        player.Availability = entities.AvailabilityDatas.Where(x => x.PlayerId == player.Id).Select(abData => new Availability
                        {
                            Id = abData.Id,
                            EventId = abData.EventId,
                            PlayerId = abData.PlayerId,
                            EmailSent = abData.EmailSent,
                            Token = abData.Token,
                            AdminStatus = abData.AdminStatus,
                            RepliedStatus = abData.RepliedStatus
                        }).ToList();
                        players.Add(player);
                    }

                    playerGroup.Players = players;
                }
                team.PlayerGroups = playerGroupDatas;

                // Retrieve Seasons

                var seasons = entities.SeasonDatas.Where(x => x.TeamId == team.Id).OrderBy(x => x.Order)
                    .Select(seasonData => new Season
                    {
                        Id = seasonData.Id,
                        LeagueId = seasonData.LeagueId,
                        TeamId = seasonData.TeamId,
                        Name = seasonData.Name,
                        Order = seasonData.Order
                    }).ToList();
                
                foreach (var season in seasons)
                {
                    season.Games = entities.GameDatas.Where(x => x.SeasonId == season.Id).Select(gameData => new Game
                    { 
                        Id = gameData.Id,
                        SeasonId = gameData.SeasonId,
                        Type = gameData.Type,
                        Date = gameData.Date,
                        HomeTeamId = gameData.HomeTeamId,
                        AwayTeamId = gameData.AwayTeamId,
                        ScoredPoints = gameData.ScoredPoints,
                        AllowedPoints = gameData.AllowedPoints,
                        TiePoints = gameData.TiePoints,
                        OpponentName = gameData.OpponentName
                    }).ToList();

                    foreach (var game in season.Games)
                    {
                        game.DateTime = (game.Date) != null ? ((DateTime)game.Date).ToString("MMM d, yyyy, h:mm tt") : null;

                        if (game.AwayTeamId != 0)
                        {
                            game.OpponentName = entities.TeamDatas.Single(x => x.Id == game.AwayTeamId).Name;
                        }

                        game.Location = entities.GameLocationDatas.Single(x => x.GameId == game.Id);
                        game.Duties = entities.GameDutyDatas.Where(x => x.GameId == game.Id).Select(x => new EventDuty
                        {
                            Id = x.Id,
                            EventId = x.GameId,
                            PlayerId = x.PlayerId,
                            Name = x.Name
                        }).ToList();
                    }
                }

                team.Seasons = seasons;

                return team;
            }
        }

        public IEnumerable<Team> GetTeams(User user)
        {
            using (var entities = Database.GetEntities())
            {
                var teamIds = entities.TeamOwnerDatas.Where(x => x.UserId == user.Id).Select(y => y.TeamId).ToList();

                var teams = new List<Team>();
                foreach (var teamId in teamIds)
                {
                    var teamData = entities.TeamDatas.Single(x => x.Id == teamId);
                    var team = new Team(teamData);

                    var teamOwnerUserIds = entities.TeamOwnerDatas.Where(x => x.TeamId == team.Id).Select(x => x.UserId).ToList();
                    team.Owners = teamOwnerUserIds.Select(userId => entities.UserDatas.Single(x => x.Id == userId)).Select(ownerData => new User(ownerData)).ToList();

                    teams.Add(team);
                }

                return teams;
            }
        }

        public List<string> GetPublicTeamUrls()
        {
            using (var entities = Database.GetEntities())
            {
                var publicTeamUrls = new List<string>();
                IQueryable<int> publicTeamIds = entities.TeamPrivacyDatas.Where(x => x.HomePage).Select(x => x.TeamId);
                foreach (var teamId in publicTeamIds)
                {
                    var team = entities.TeamDatas.SingleOrDefault(x => x.Id == teamId);
                    if(team != null) 
                    {
                        publicTeamUrls.Add(string.Format("https://teamkeep.com{0}", Team.FormatUrl(team.Id, team.Name)));
                    }
                }
                return publicTeamUrls;
            }
        }

        public Team AddTeam(Team team, User creator)
        {
            using (var entities = Database.GetEntities())
            {
                var teamData = new TeamData {Name = team.Name};
                entities.TeamDatas.AddObject(teamData);
                entities.SaveChanges();

                entities.TeamSettingsDatas.AddObject(new TeamSettingsData 
                { 
                    TeamId = teamData.Id, 
                    ResultsView = 0,
                    ArenaColumn = true, 
                    LastNameColumn = true, 
                    PositionColumn = true,
                    PhoneColumn = true,
                    EmailColumn = true
                });

                entities.TeamPrivacyDatas.AddObject(new TeamPrivacyData 
                {
                    TeamId = teamData.Id, 
                    HomePage = true, 
                    Roster = true 
                });

                entities.TeamOwnerDatas.AddObject(new TeamOwnerData 
                { 
                    TeamId = teamData.Id, 
                    UserId = creator.Id 
                });

                entities.SaveChanges();

                team.Id = teamData.Id;
                return team;
            }
        }

        public Team UpdateAnnouncements(Team team, string announcement)
        {
            using (var entities = Database.GetEntities())
            {
                var teamData = entities.TeamDatas.Single(x => x.Id == team.Id);
                teamData.Announcements = announcement;
                entities.SaveChanges();

                team.Announcement = teamData.Announcements;
                return team;
            }
        }

        public TeamSettingsViewModel UpdateSettings(TeamSettingsViewModel teamSettings)
        {
            using (var entities = Database.GetEntities())
            {
                var teamData = entities.TeamDatas.Single(x => x.Id == teamSettings.TeamId);

                if (!string.IsNullOrEmpty(teamSettings.Name))
                {
                    teamData.Name = teamSettings.Name;    
                }

                // Update settings
                if (teamSettings.Settings != null)
                {
                    var settingsData = entities.TeamSettingsDatas.Single(x => x.TeamId == teamSettings.TeamId);
                    settingsData.ResultsView = teamSettings.Settings.ResultsView;
                    settingsData.ArenaColumn = teamSettings.Settings.ArenaColumn;
                    settingsData.LastNameColumn = teamSettings.Settings.LastNameColumn;
                    settingsData.PositionColumn = teamSettings.Settings.PositionColumn;
                    settingsData.PhoneColumn = teamSettings.Settings.PhoneColumn;
                    settingsData.EmailColumn = teamSettings.Settings.EmailColumn;
                }

                // Update privacy
                if (teamSettings.Privacy != null)
                {
                    var privacyData = entities.TeamPrivacyDatas.Single(x => x.TeamId == teamSettings.TeamId);
                    privacyData.HomePage = teamSettings.Privacy.HomePage;
                    privacyData.Roster = teamSettings.Privacy.Roster;
                }

                entities.SaveChanges();

                return teamSettings;
            }
        }

        public Team UpdateBanner(Team team)
        {
            using (var entities = Database.GetEntities())
            {
                var teamData = entities.TeamDatas.Single(x => x.Id == team.Id);
                teamData.Banner = team.BannerImage;
                entities.SaveChanges();

                return team;
            }
        }

        public void RemoveTeam(int teamId)
        {
            using (var entities = Database.GetEntities())
            {
                // Delete seasons and games
                var seasonIds = entities.SeasonDatas.Where(x => x.TeamId == teamId).Select(x => x.Id).ToList();
                var gameService = new GameService();
                foreach (var seasonId in seasonIds)
                {
                    gameService.RemoveSeason(seasonId);
                }

                // Delete owners
                foreach (var teamOwnerData in entities.TeamOwnerDatas.Where(x => x.TeamId == teamId).ToList())
                {
                    entities.DeleteObject(teamOwnerData);
                }

                // Delete roster
                foreach (var playerGroup in entities.PlayerGroupDatas.Where(x => x.TeamId == teamId).ToList())
                {
                    var group = playerGroup;
                    foreach(var player in entities.PlayerDatas.Where(x => x.GroupId == group.Id))
                    {
                        entities.DeleteObject(player);
                    }
                    entities.DeleteObject(playerGroup);
                }

                entities.DeleteObject(entities.TeamDatas.Single(x => x.Id == teamId));
                entities.DeleteObject(entities.TeamSettingsDatas.Single(x => x.TeamId == teamId));
                entities.DeleteObject(entities.TeamPrivacyDatas.Single(x => x.TeamId == teamId));
                entities.SaveChanges();
            }
        }

        public void RemoveMessage(Message message)
        {
            using (var entities = Database.GetEntities())
            {
                var messageData = entities.MessageDatas.Single(x => x.TeamId == message.TeamId && x.Id == message.Id);
                entities.MessageDatas.DeleteObject(messageData);
                entities.SaveChanges();
            }
        }
    }
}