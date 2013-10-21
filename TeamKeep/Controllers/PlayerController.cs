using System.Net;
using System.Web;
using System.Web.Mvc;
using Teamkeep.Models;
using Teamkeep.Models.DataModels;

namespace Teamkeep.Controllers
{
    public class PlayerController : ViewController
    {
        [HttpPost]
        public JsonResult CreateGroup(int teamId, PlayerGroup playerGroup)
        {
            var activeUser = this.GetActiveUser(this.Request);
            if (!_teamService.CanEdit(teamId, activeUser.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to add player groups to this team");
            }

            playerGroup = _playerService.AddPlayerGroup(playerGroup);

            return Json(playerGroup);
        }

        [HttpPut]
        public JsonResult UpdateGroup(int teamId, PlayerGroup playerGroup)
        {
            var activeUser = this.GetActiveUser(this.Request);
            if (!_teamService.CanEdit(teamId, activeUser.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to edit player groups for this team");
            }

            var existingGroup = _playerService.GetPlayerGroup(playerGroup.Id);
            if (existingGroup.TeamId != teamId)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Mismatch between team ID in path and group team ID");
            }

            playerGroup = _playerService.UpdatePlayerGroup(playerGroup);
            return Json(playerGroup);
        }

        [HttpDelete]
        public JsonResult DeleteGroup(int teamId, PlayerGroup playerGroup)
        {
            var activeUser = this.GetActiveUser(this.Request);
            if (!_teamService.CanEdit(teamId, activeUser.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to delete player groups for this team");
            }

            _playerService.DeletePlayerGroup(playerGroup);

            return Json(null);
        }

        [HttpPost]
        public JsonResult Create(int teamId, Player player)
        {
            var activeUser = this.GetActiveUser(this.Request);
            if (!_teamService.CanEdit(teamId, activeUser.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to add players to this team roster");
            }

            var playerGroup = _playerService.GetPlayerGroup(player.GroupId);
            if (playerGroup.TeamId != teamId)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "TeamId mismatch");
            }

            player = _playerService.AddPlayer(player);

            return Json(player);
        }

        [HttpPut]
        public JsonResult Update(int teamId, Player player)
        {
            var activeUser = this.GetActiveUser(this.Request);
            if (!_teamService.CanEdit(teamId, activeUser.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to edit this team roster");
            }

            var knownPlayer = _playerService.GetPlayer(player.Id);
            if (knownPlayer != null)
            {
                var playerGroup = _playerService.GetPlayerGroup(knownPlayer.GroupId);
                if (playerGroup.TeamId != teamId)
                {
                    throw new HttpException((int)HttpStatusCode.BadRequest, "Not authorized to edit this player");
                }

                player = _playerService.UpdatePlayer(player);
            }

            return Json(player);
        }

        [HttpDelete]
        public JsonResult Delete(int teamId, Player player)
        {
            var activeUser = this.GetActiveUser(this.Request);
            if (!_teamService.CanEdit(teamId, activeUser.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to delete players on this team roster");
            }

            var knownPlayer = _playerService.GetPlayer(player.Id);
            if (knownPlayer != null)
            {
                var playerGroup = _playerService.GetPlayerGroup(knownPlayer.GroupId);
                if (playerGroup.TeamId != teamId)
                {
                    throw new HttpException((int)HttpStatusCode.BadRequest, "Not authorized to edit this player");
                }

                _playerService.RemovePlayer(player);
            }

            return Json(null);
        }

        [HttpPut]
        public JsonResult UpdateAvailability(int teamId, int playerId, AvailabilityData availability)
        {
            var activeUser = this.GetActiveUser(this.Request);
            if (!_teamService.CanEdit(teamId, activeUser.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to edit this team");
            }

            var knownPlayer = _playerService.GetPlayer(playerId);
            if (knownPlayer != null)
            {
                var playerGroup = _playerService.GetPlayerGroup(knownPlayer.GroupId);
                if (playerGroup.TeamId != teamId)
                {
                    throw new HttpException((int)HttpStatusCode.BadRequest, "Not authorized to edit this player");
                }
            }

            if (availability.EventId == 0)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "No event ID was given");
            }

            var knownEvent = _gameService.GetGame(availability.EventId);
            if (teamId != knownEvent.HomeTeamId)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Team ID mismatch");
            }

            availability = _playerService.UpdatePlayerAvailability(knownPlayer.Id, availability);

            return Json(availability);
        }

        [HttpPut]
        public JsonResult SetAvailability(AvailabilityData availability)
        {
            if (availability.RepliedStatus == 0 || availability.RepliedStatus > 3)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Invalid reply status");
            }

            availability = _playerService.SetPlayerAvailability(availability.Token, (short) availability.RepliedStatus);
            return Json(availability);
        }
    }
}
