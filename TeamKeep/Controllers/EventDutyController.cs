using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TeamKeep.Models.DataModels;

namespace TeamKeep.Controllers
{
    public class EventDutyController : ViewController
    {
        [HttpPost]
        public JsonResult Create(int teamId, int eventId, GameDutyData duty)
        {
            var activeUser = GetActiveUser(Request);
            var team = _teamService.GetTeam(teamId);
            if (!team.Owners.Exists(x => x.Id == activeUser.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to edit this team");
            }

            var evt = _gameService.GetGame(eventId);

            if (teamId != _gameService.GetSeason(evt.SeasonId).TeamId)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Requested season does not belong to this team");
            }

            if (string.IsNullOrEmpty(duty.Name))
            {
                Response.StatusCode = 400;
                return Json("Please provide a name for this duty");
            }

            duty.GameId = eventId;
            duty = _gameService.AddEventDuty(duty);

            return Json(duty, JsonRequestBehavior.AllowGet);
        }

        [HttpDelete]
        public JsonResult Delete(int teamId, GameDutyData duty)
        {
            var activeUser = this.GetActiveUser(this.Request);
            var team = _teamService.GetTeam(teamId);
            if (!team.Owners.Exists(x => x.Id == activeUser.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to edit this team");
            }

            var evt = _gameService.GetGame(duty.GameId);

            if (teamId != _gameService.GetSeason(evt.SeasonId).TeamId)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Requested season does not belong to this team");
            }

            //_gameService.RemoveSeason(season.Id);

            return Json(null, JsonRequestBehavior.AllowGet);
        }

    }
}
