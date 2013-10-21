using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Teamkeep.Models;

namespace Teamkeep.Controllers
{
    public class GameController : ViewController
    {
        [HttpPost]
        public JsonResult Create(Game game)
        {
            var activeUser = GetActiveUser(Request);

            var team = _teamService.GetTeam(game.HomeTeamId);
            if (!team.Owners.Exists(x => x.Id == activeUser.Id))
            {
                throw new HttpException((int) HttpStatusCode.Unauthorized, "Not authorized to add games for this team");
            }

            var season = _gameService.GetSeason(game.SeasonId);

            if (season.TeamId != team.Id)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "TeamId of selected season did not match Game's TeamId");
            }

            game = _gameService.AddGame(game);

            return Json(game, JsonRequestBehavior.AllowGet);
        }

        [HttpPut]
        public JsonResult Update(Game game)
        {
            var activeUser = GetActiveUser(Request);

            if (!_gameService.CanEditGame(activeUser.Id, game.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to edit games for this team");
            }

            // Validate inputs
            if (game.ScoredPoints > 999 || game.AllowedPoints > 999 || game.TiePoints > 999)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Points value exceeds maximum");
            }

            game = _gameService.UpdateGate(game);
            return Json(game, JsonRequestBehavior.AllowGet);
        }

        [HttpDelete]
        public JsonResult Delete(int gameId)
        {
            var activeUser = GetActiveUser(Request);

            if (!_gameService.CanEditGame(activeUser.Id, gameId))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to delete games for this team");
            }

            _gameService.RemoveGame(gameId);
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateSeason(int teamId, Season season)
        {
            var activeUser = this.GetActiveUser(this.Request);
            if (!_teamService.CanEdit(teamId, activeUser.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to add seasons to this team");
            }

            season.TeamId = teamId;
            season = _gameService.AddSeason(season);
            return Json(season);
        }

        [HttpPut]
        public JsonResult UpdateSeason(int teamId, Season season)
        {
            var activeUser = this.GetActiveUser(this.Request);
            if (!_teamService.CanEdit(teamId, activeUser.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to update seasons for this team");
            }

            var existingSeason = _gameService.GetSeason(season.Id);
            if (existingSeason.TeamId != teamId)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Mismatch between team ID in path and season team ID");
            }

            season = _gameService.UpdateSeason(season);
            return Json(season);
        }

        [HttpDelete]
        public JsonResult DeleteSeason(int teamId, Season season)
        {
            var activeUser = this.GetActiveUser(this.Request);
            if (!_teamService.CanEdit(teamId, activeUser.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to remove seasons from this team");
            }

            var existingSeason = _gameService.GetSeason(season.Id);
            if (existingSeason.TeamId != teamId)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Mismatch between team ID in path and season team ID");
            }

            _gameService.RemoveSeason(season.Id);

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SendConfirmations(int gameId, List<int> playerIds)
        {
            var activeUser = this.GetActiveUser(this.Request);
            if (!_gameService.CanEditGame(activeUser.Id, gameId))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to send emails for this team");
            }

            if (playerIds == null || playerIds.Count == 0)
            {
                Response.StatusCode = 400;
                return Json("Specify at least one recipient for confirmation");
            }

            var serviceResponse = _gameService.SendConfirmationEmails(gameId, playerIds);

            if (serviceResponse.Error)
            {
                Response.StatusCode = 400;
                return Json(serviceResponse.Message);
            }
           
            return Json(serviceResponse);
        }
    }
}
