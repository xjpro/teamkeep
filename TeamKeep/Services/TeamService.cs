using System.Linq;
using TeamKeep.Models;
using TeamKeep.Models.DataModels;
using System.Collections.Generic;
using System;

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

                // Retrieve privacy
                team.Privacy = entities.TeamPrivacyDatas.Single(x => x.TeamId == team.Id);

                // Retrieve owners
                var teamOwnerUserIds = entities.TeamOwnerDatas.Where(x => x.TeamId == team.Id).Select(x => x.UserId).ToList();
                team.Owners = teamOwnerUserIds.Select(ownerId => entities.UserDatas.Single(x => x.Id == ownerId))
                    .Select(userData => new User(userData)).ToList();

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
                        player.Availability = entities.AvailabilityDatas.Where(x => x.PlayerId == player.Id).ToList();
                        players.Add(player);
                    }

                    playerGroup.Players = players;
                }
                team.PlayerGroups = playerGroupDatas;

                // Retrieve team-submitted seasons
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
                        Date = gameData.Date,
                        SeasonId = gameData.SeasonId,
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

        public Team AddTeam(Team team, User creator)
        {
            using (var entities = Database.GetEntities())
            {
                var teamData = new TeamData {Name = team.Name};
                entities.TeamDatas.AddObject(teamData);
                entities.SaveChanges();

                entities.TeamPrivacyDatas.AddObject(new TeamPrivacyData { TeamId = teamData.Id, HomePage = true, Roster = true });
                entities.TeamOwnerDatas.AddObject(new TeamOwnerData { TeamId = teamData.Id, UserId = creator.Id });
                entities.SaveChanges();

                team.Id = teamData.Id;
                return team;
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
                    Order = (short) order
                };

                entities.SeasonDatas.AddObject(seasonData);
                entities.SaveChanges();

                season.Id = seasonData.Id;
                season.Games = new List<Game>();
                season.Order = seasonData.Order;
                return season;
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

        public Team UpdateSettings(Team team)
        {
            using (var entities = Database.GetEntities())
            {
                var teamData = entities.TeamDatas.Single(x => x.Id == team.Id);
                teamData.Name = team.Name;

                if (team.Privacy != null)
                {
                    var teamPrivacyData = entities.TeamPrivacyDatas.Single(x => x.TeamId == team.Id);
                    teamPrivacyData.HomePage = team.Privacy.HomePage;
                    teamPrivacyData.Roster = team.Privacy.Roster;
                }

                entities.SaveChanges();

                return team;
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

        public void RemoveTeam(int teamId)
        {
            using (var entities = Database.GetEntities())
            {
                // Delete seasons and games
                var seasonIds = entities.SeasonDatas.Where(x => x.TeamId == teamId).Select(x => x.Id).ToList();
                foreach (var seasonId in seasonIds)
                {
                    RemoveSeason(seasonId);
                }

                // Delete owners
                foreach (var teamOwnerData in entities.TeamOwnerDatas.Where(x => x.TeamId == teamId).ToList())
                {
                    entities.DeleteObject(teamOwnerData);
                }

                // Delete roster
                foreach (var playerGroup in entities.PlayerGroupDatas.Where(x => x.TeamId == teamId).ToList())
                {
                    foreach(var player in entities.PlayerDatas.Where(x => x.GroupId == playerGroup.Id))
                    {
                        entities.DeleteObject(player);
                    }
                    entities.DeleteObject(playerGroup);
                }

                entities.DeleteObject(entities.TeamDatas.Single(x => x.Id == teamId));
                entities.SaveChanges();
            }
        }

        public void RemoveSeason(int seasonId)
        {
            using (var entities = Database.GetEntities())
            {
                var gameDatas = entities.GameDatas.Where(x => x.SeasonId == seasonId).ToList();
                foreach(var gameData in gameDatas)
                {
                    entities.GameDatas.DeleteObject(gameData);
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