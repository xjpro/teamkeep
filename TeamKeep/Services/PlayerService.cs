using System.Collections.Generic;
using TeamKeep.Models;
using TeamKeep.Models.DataModels;
using System.Linq;

namespace TeamKeep.Services
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
                    entities.DeleteObject(playerData);
                }

                var groupData = entities.PlayerGroupDatas.Single(x => x.Id == group.Id);
                entities.DeleteObject(groupData);
                entities.SaveChanges();

                // Ensure proper ordering
                short order = 0;
                foreach (var playerGroupData in entities.PlayerGroupDatas.OrderBy(x => x.Order).ToList())
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
                player.Availability = entities.AvailabilityDatas.Where(x => x.PlayerId == player.Id).ToList();
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
                    Email = player.Email,
                    Phone = player.Phone
                };

                entities.PlayerDatas.AddObject(playerData);
                entities.SaveChanges();

                player.Id = playerData.Id;
                player.Availability = new List<AvailabilityData>();
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
                entities.DeleteObject(entities.PlayerDatas.Single(x => x.Id == player.Id));
                entities.SaveChanges();
            }
        }

        public AvailabilityData UpdatePlayerAvailability(int playerId, AvailabilityData availability)
        {
            using (var entities = Database.GetEntities())
            {
                var abData = entities.AvailabilityDatas.SingleOrDefault(x => x.PlayerId == playerId && x.EventId == availability.EventId);
                if (abData == null)
                {
                    entities.AvailabilityDatas.AddObject(availability);
                    entities.SaveChanges();
                    return availability;
                }
                
                abData.AdminStatus = availability.AdminStatus;
                abData.RepliedStatus = availability.RepliedStatus;
                abData.EmailSent = availability.EmailSent;
                entities.SaveChanges();
                return abData;
            }
        }
    }
}