using System.Net;
using System.Web;
using System.Web.Mvc;
using TeamKeep.Models;

namespace TeamKeep.Controllers
{
    public class EventDutyController : ViewController
    {
        [HttpPost]
        public JsonResult Create(int teamId, int eventId, EventDuty duty)
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

            if (evt.Duties.Count > 20)
            {
                Response.StatusCode = 400;
                return Json("Maximum of 20 duties per event");
            }

            duty.EventId = eventId;
            duty = _gameService.AddEventDuty(duty);

            return Json(duty, JsonRequestBehavior.AllowGet);
        }

        [HttpDelete]
        public JsonResult Delete(int teamId, EventDuty duty)
        {
            var activeUser = this.GetActiveUser(this.Request);
            var team = _teamService.GetTeam(teamId);
            if (!team.Owners.Exists(x => x.Id == activeUser.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to edit this team");
            }

            duty = _gameService.GetEventDuty(duty.Id); // Get copy for ourselves
            var evt = _gameService.GetGame(duty.EventId);

            if (teamId != _gameService.GetSeason(evt.SeasonId).TeamId)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Requested duty does not belong to this team");
            }

            _gameService.RemoveEventDuty(duty.Id);

            return Json(null, JsonRequestBehavior.AllowGet);
        }

    }
}
