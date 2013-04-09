using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TeamKeep.Models.ViewModels;
using TeamKeep.Models;
using System;

namespace TeamKeep.Controllers
{
    public class TeamController : ViewController
    {
        [HttpGet]
        public ActionResult Home(int teamId)
        {
            var activeUser = this.GetActiveUser(this.Request);
            var team = _teamService.GetTeam(teamId);
            if (team == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Team not found");
            }

            var viewModel = new TeamViewModel { Team = team };

            if (activeUser != null)
            {
                if (activeUser.ActiveTeamId != teamId && team.CanEdit(activeUser.Id))
                {
                    activeUser.ActiveTeamId = teamId;
                    _userService.SetActiveTeamId(activeUser.Id, teamId);
                }

                viewModel.User = activeUser;
                team.Editable = team.CanEdit(activeUser.Id);
            }

            if (!team.Editable) // Filtering data based on privacy settings
            {
                if (!team.Privacy.HomePage) // Team home page is not public
                {
                    viewModel.Team = null;
                    return View("TeamPrivateHome", viewModel);
                }

                // Hide email & phone in public view
                foreach (var player in team.PlayerGroups.SelectMany(playerGroup => playerGroup.Players))
                {
                    player.Email = null;
                    player.Phone = null;
                    player.Availability = null;
                }

                if (!team.Privacy.Roster)
                {
                    team.PlayerGroups = null;
                }
            }

            return View("TeamHome", viewModel);
        }

        [HttpGet]
        public ActionResult HomeRedirect(int teamId)
        {
            var team = _teamService.GetTeam(teamId);
            var redirect = (team != null) ? team.Url : string.Format("/teams/{0}/Unknown", teamId);
            Response.Redirect(redirect);
            return null;
        }

        [HttpPost]
        public JsonResult Create(Team team)
        {
            var activeUser = this.GetActiveUser(this.Request);
            team = _teamService.AddTeam(team, activeUser);
            return Json(team);
        }

        [HttpPut]
        public JsonResult UpdateAnnouncement(int id, string announcement)
        {
            var activeUser = this.GetActiveUser(this.Request);
            Team team = _teamService.GetTeam(id);
            if (!team.CanEdit(activeUser.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to edit this team's announcement");
            }

            team = _teamService.UpdateAnnouncements(team, announcement);
            return Json(team);
        }

        [HttpPut]
        public JsonResult UpdateSettings(int id, TeamSettingsViewModel settings)
        {
            var activeUser = this.GetActiveUser(this.Request);
            if (!_teamService.CanEdit(id, activeUser.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to edit this team's settings");
            }

            if (id != settings.TeamId)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Team ID mismatch");
            }

            settings = _teamService.UpdateSettings(settings);
            return Json(settings);
        }

        [HttpPut]
        public JsonResult UpdateBanner(int id)
        {
            var activeUser = this.GetActiveUser(this.Request);
            var team = _teamService.GetTeam(id);
            if (!team.CanEdit(activeUser.Id))
            {
                throw new HttpException((int) HttpStatusCode.Unauthorized, "Not authorized to edit this team's banner");
            }

            long imageSize;
            try
            {
                imageSize = Request.InputStream.Length;
            }
            catch (Exception)
            {
                Response.StatusCode = 400;
                return Json("Image exceeds maximum size");
            }

            if (imageSize > 2000000)
            {
                Response.StatusCode = 400;
                return Json("Image exceeds maximum size");
            }

            string originalFileName = Request["HTTP_X_FILE_NAME"];
            string extension =
                Regex.Match(originalFileName, ".([a-zA-Z]{3,4})$").Groups[1].ToString().ToLowerInvariant();

            if (!extension.Equals("jpg") && !extension.Equals("jpeg") && !extension.Equals("png"))
            {
                Response.StatusCode = 400;
                return Json("Chosen file must be of type jpg or png");
            }

            var fileName = string.Format("Team{0}.{1}", team.Id, extension);
            var imagePath = Path.Combine(Server.MapPath(Url.Content("~/TeamBanners")), fileName);

            var buffer = new byte[Request.InputStream.Length];

            int offset = 0;
            int cnt;
            // Read into buffer
            while ((cnt = Request.InputStream.Read(buffer, offset, 10)) > 0)
            {
                offset += cnt;
            }

            if (!ImageHeaderMatchesExtension(buffer, extension))
            {
                Response.StatusCode = 400;
                return Json("Image header does not match extension");
            }

            // Save file
            using (var fs = new FileStream(imagePath, FileMode.Create))
            {
                fs.Write(buffer, 0, buffer.Length);
                fs.Flush();
            }

            team.BannerImage = fileName;
            team = _teamService.UpdateBanner(team);

            return Json(team);
        }

        [HttpDelete]
        public JsonResult Delete(int id)
        {
            var activeUser = this.GetActiveUser(this.Request);
            if (!_teamService.CanEdit(id, activeUser.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to delete this team");
            }

            _teamService.RemoveTeam(id);
            return Json(null);
        }

        private bool ImageHeaderMatchesExtension(byte[] buffer, string extension)
        {
            if (extension.Equals("jpg") || extension.Equals("jpeg"))
            {
                if (buffer[0] == 255 && buffer[1] == 216) return true;
            }
            else if (extension.Equals("png"))
            {
                if (buffer[0] == 137 && buffer[1] == 80 && buffer[2] == 78 && buffer[3] == 71) return true;
            }
            return false;
        }
    }
}
