using System.Collections.Generic;
using Teamkeep.Models;
using Teamkeep.Models.DataModels;
using System.Linq;

namespace Teamkeep.Services
{
    public class PlayerService
    {
        public PlayerGroup GetPlayerGroup(int id)
        {
            using (var entities = Database.GetEntities())
            {
                var groupData = entities.PlayerGroupDatas.Single(x => x.Id == id);
                return new PlayerGroup
                {
                    Id = groupData.Id,
                    TeamId = groupData.TeamId,
                    Name = groupData.Name,
                    Order = groupData.Order
                };
            }
        }

        public PlayerGroup AddPlayerGroup(PlayerGroup group)
        {
            using (var entities = Database.GetEntities())
            {
                var order = entities.PlayerGroupDatas.Count(x => x.TeamId == group.TeamId);

                var groupData = new PlayerGroupData
                {
                    TeamId =  group.TeamId,
                    Name = group.Name,
                    Order = (short) order
                };
                entities.PlayerGroupDatas.AddObject(groupData);
                entities.SaveChanges();

                group.Id = groupData.Id;
                group.Players = new List<Player>();
                group.Order = groupData.Order;
                return group;
            }
        }

        public PlayerGroup UpdatePlayerGroup(PlayerGroup group)
        {
            using (var entities = Database.GetEntities())
            {
                var groupData = entities.PlayerGroupDatas.Single(x => x.Id == group.Id);
                groupData.Name = group.Name;
                
                if (groupData.Order != group.Order) // A reordering
                {
                    // Swapping
                    var swapping = entities.PlayerGroupDatas.FirstOrDefault(x => x.TeamId == group.TeamId && x.Order == group.Order);
                    if (swapping != null)
                    {
                        swapping.Order = groupData.Order; // Give it the current order
                    }

                    // Matching
                    groupData.Order = group.Order; // Give it the new order

                    entities.SaveChanges();

                    // Ensure proper ordering
                    short order = 0;
                    foreach (var orderedGroupData in entities.PlayerGroupDatas.Where(x => x.TeamId == group.TeamId).OrderBy(x => x.Order).ToList())
                    {
                        orderedGroupData.Order = order;
                        order++;
                    }
                }

                entities.SaveChanges();

                return group;
            }
        }

        public void DeletePlayerGroup(PlayerGroup group)
        {
            using (var entities = Database.GetEntities())
            {
                foreach (var playerData in entities.PlayerDatas.Where(x => x.GroupId == group.Id))
                {
                    RemovePlayer(entities, playerData.Id, false);
                }

                var groupData = entities.PlayerGroupDatas.Single(x => x.Id == group.Id);
                int teamId = groupData.TeamId;

                entities.DeleteObject(groupData);
                entities.SaveChanges();

                // Ensure proper ordering
                short order = 0;
                foreach (var playerGroupData in entities.PlayerGroupDatas
                    .Where(x => x.TeamId == teamId).OrderBy(x => x.Order).ToList())
                {
                    playerGroupData.Order = order;
                    order++;
                }
                entities.SaveChanges();
            }
        }

        public Player GetPlayer(int playerId)
        {
            using (var entities = Database.GetEntities())
            {
                var playerData = entities.PlayerDatas.SingleOrDefault(x => x.Id == playerId);
                if (playerData == null) return null;

                var player = new Player(playerData);
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

                return player;
            }
        }

        public Player AddPlayer(Player player)
        {
            using (var entities = Database.GetEntities())
            {
                var playerData = new PlayerData
                {
                    GroupId = player.GroupId,
                    LastName = player.LastName,
                    FirstName = player.FirstName,
                    Position = player.Position,
                    Email = player.Email,
                    Phone = player.Phone
                };

                entities.PlayerDatas.AddObject(playerData);
                entities.SaveChanges();

                player.Id = playerData.Id;
                player.Availability = new List<Availability>();
                return player;
            }
        }

        public Player UpdatePlayer(Player player)
        {
            using (var entities = Database.GetEntities())
            {
                var playerData = entities.PlayerDatas.Single(x => x.Id == player.Id);
                playerData.GroupId = player.GroupId;
                playerData.LastName = player.LastName;
                playerData.FirstName = player.FirstName;
                playerData.Position = player.Position;
                playerData.Email = player.Email;
                playerData.Phone = player.Phone;
                entities.SaveChanges();

                return player;
            }
        }

        public void RemovePlayer(Player player)
        {
            using (var entities = Database.GetEntities())
            {
                RemovePlayer(entities, player.Id);
            }
        }

        private void RemovePlayer(DatabaseEntities entities, int playerId, bool saveChanges = true)
        {
            // Reassign duties
            foreach (var dutyData in entities.GameDutyDatas.Where(x => x.PlayerId == playerId))
            {
                dutyData.PlayerId = null;
            }

            // Remove availabilities
            foreach (var abData in entities.AvailabilityDatas.Where(x => x.PlayerId == playerId))
            {
                entities.DeleteObject(abData);
            }

            entities.DeleteObject(entities.PlayerDatas.Single(x => x.Id == playerId));

            if(saveChanges) 
            {
                entities.SaveChanges();
            }
        }

        public AvailabilityRequest GetAvailabilityRequest(string token)
        {
            using (var entities = Database.GetEntities())
            {
                var requestData = entities.AvailabilityDatas.FirstOrDefault(x => x.Token == token);
                if (requestData == null) return null;
                
                var eventData = entities.GameDatas.Single(x => x.Id == requestData.EventId);
                var eventLocationData = entities.GameLocationDatas.Single(x => x.GameId == requestData.EventId);
                var abEvent = new Game(eventData) {Location = eventLocationData};

                var request = new AvailabilityRequest
                {
                    Data = requestData,
                    Event = abEvent,
                    TeamName = entities.TeamDatas.Single(x => x.Id == abEvent.HomeTeamId).Name
                };

                return request;
            }
        }

        public AvailabilityData UpdatePlayerAvailability(int playerId, AvailabilityData availability)
        {
            using (var entities = Database.GetEntities())
            {
                var playerAbDataList = entities.AvailabilityDatas.Where(x => x.PlayerId == playerId && x.EventId == availability.EventId);

                if (playerAbDataList.Count() == 0) // Create new one
                {
                    entities.AvailabilityDatas.AddObject(new AvailabilityData
                    {
                        PlayerId = availability.PlayerId,
                        EventId = availability.EventId,
                        AdminStatus = availability.AdminStatus,
                        RepliedStatus = availability.RepliedStatus
                    });
                    entities.SaveChanges();
                    return availability;
                }

                if(playerAbDataList.Count() > 1) // Remove extra (rare but can happen)
                {
                    foreach(var extraAbData in playerAbDataList.OrderBy(x => x.Id).Skip(1))
                    {
                        entities.DeleteObject(extraAbData);
                    }
                }

                var abData = playerAbDataList.FirstOrDefault();
                abData.AdminStatus = availability.AdminStatus;
                abData.RepliedStatus = availability.RepliedStatus;
                abData.EmailSent = availability.EmailSent;
                entities.SaveChanges();
                return abData;
            }
        }

        public AvailabilityData SetPlayerAvailability(string token, short status)
        {
            using (var entities = Database.GetEntities())
            {
                var abData = entities.AvailabilityDatas.SingleOrDefault(x => x.Token == token);
                if (abData != null)
                {
                    abData.AdminStatus = status;
                    abData.RepliedStatus = status;
                    entities.SaveChanges();
                }

                return abData;
            }
        }
    }
}