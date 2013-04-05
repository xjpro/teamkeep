using System.Net;
using System.Web;
using System.Web.Mvc;
using TeamKeep.Models;
using TeamKeep.Services;

namespace TeamKeep.Controllers
{
    public class GameController : ViewController
    {
        private readonly GameService _gameService = new GameService();

        [HttpPost]
        public JsonResult Create(Game game)
        {
            var activeUser = GetActiveUser(Request);

            var team = _teamService.GetTeam(game.HomeTeamId);
            if (!team.Owners.Exists(x => x.Id == activeUser.Id))
            {
                throw new HttpException((int) HttpStatusCode.Unauthorized, "Not authorized to add games for this team");
            }

            var season = _teamService.GetSeason(game.SeasonId);

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
    }
}
