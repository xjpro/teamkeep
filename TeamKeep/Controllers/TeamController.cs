using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Teamkeep.Models.DataModels;
using Teamkeep.Models.ViewModels;
using Teamkeep.Models;
using System;
using Teamkeep.Services;
using System.Collections.Generic;
using System.Globalization;

namespace Teamkeep.Controllers
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
                Response.StatusCode = 404;
                throw new HttpException((int)HttpStatusCode.NotFound, "Team not found");
            }

            var viewModel = new TeamViewModel { Team = team };

            if (activeUser != null)
            {
                if (activeUser.ActiveTeamId != teamId && team.CanEdit(activeUser.Id))
                {
                    activeUser.ActiveTeamId = teamId;
                    activeUser.LastSeen = DateTime.Now;
                    activeUser = _userService.UpdateUser(activeUser);
                }

                activeUser.Settings = _userService.GetUserSettings(activeUser.Id);

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

                // Hide duties
                foreach (var teamEvent in team.Seasons.SelectMany(season => season.Games))
                {
                    teamEvent.Duties = new List<EventDuty>();
                }

                // Hide messages in public view
                team.Messages = new List<Message>();

                // Hide email & phone in public view
                foreach (var player in team.PlayerGroups.SelectMany(playerGroup => playerGroup.Players))
                {
                    player.Email = null;
                    player.Phone = null;
                    player.Availability = null;
                }

                if (!team.Privacy.Roster)
                {
                    team.PlayerGroups = new List<PlayerGroup>();
                }
            }

            return View(viewModel);
        }

        [HttpGet]
        public ActionResult HomeRedirect(int teamId)
        {
            var team = _teamService.GetTeam(teamId);
            var redirect = (team != null) ? team.Url : string.Format("/teams/{0}/Unknown", teamId);
            Response.Redirect(redirect);
            return null;
        }

        [HttpGet]
        public ActionResult HomeEmpty()
        {
            var user = this.GetActiveUser(this.Request);

            if (user == null)
            {
                Response.Redirect("/");
            }
            else if (user.ActiveTeamId != null)
            {
                if (_teamService.GetTeam((int)user.ActiveTeamId) == null)
                {
                    // If active team id is somehow not a real team, correct this 
                    var otherTeams = _teamService.GetTeams(user);
                    if (otherTeams.Count() > 0)
                    {
                        user.ActiveTeamId = otherTeams.First().Id;
                    }
                    else
                    {
                        user.ActiveTeamId = null;
                    }
                    _userService.UpdateUser(user);
                }

                if (user.ActiveTeamId != null)
                {
                    Response.Redirect("/teams/" + user.ActiveTeamId);
                }
            }

            var viewModel = new TeamViewModel { User = user };

            return View("Home", viewModel);
        }

        [HttpPost]
        public JsonResult Create(Team team)
        {
            var activeUser = this.GetActiveUser(this.Request);
            if (activeUser == null)
            {
                throw new HttpException((int) HttpStatusCode.Unauthorized, "Invalid user");
            }

            var type = team.Type;
            var prepopulate = team.Prepopulate;
            var makePublic = team.MakePublic;

            if (prepopulate) 
            {
                // User asked us to prepoplate the team with a sample schedule and roster
                team = new OrchestrationService().CreateSampleTeam(team.Name, activeUser);
            }
            else
            {
                // Normal, empty team
                team = _teamService.AddTeam(team, activeUser);
            }

            // Set settings
            var settings = new TeamSettingsViewModel
            {
                TeamId = team.Id,
                Name = team.Name,
                Privacy = new TeamPrivacyData {HomePage = makePublic, Roster = true},
                Settings = new TeamSettingsData { EmailColumn = true } // Defined below
            };

            switch (type)
            {
                case "online":
                    settings.Settings.ArenaColumn = false;
                    settings.Settings.LastNameColumn = false;
                    settings.Settings.PositionColumn = false;
                    settings.Settings.PhoneColumn = false;
                    settings.Settings.ResultsView = 1;
                    break;
                case "club":
                    settings.Settings.ArenaColumn = false;
                    settings.Settings.LastNameColumn = true;
                    settings.Settings.PositionColumn = false;
                    settings.Settings.PhoneColumn = true;
                    settings.Settings.ResultsView = 3;
                    break;
                default: // & "sports"
                    settings.Settings.ArenaColumn = true;
                    settings.Settings.LastNameColumn = true;
                    settings.Settings.PositionColumn = true;
                    settings.Settings.PhoneColumn = true;
                    settings.Settings.ResultsView = 0;
                    break;
            }
            _teamService.UpdateSettings(settings);

            return Json(team);
        }

        [HttpPut]
        public JsonResult Update(int id, TeamUpdate update)
        {
            var activeUser = this.GetActiveUser(this.Request);
            Team team = _teamService.GetTeam(id);
            if (!team.CanEdit(activeUser.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to edit this team");
            }

            // Update seasons in delta update
            if (update.Seasons != null)
            {
                foreach (var season in update.Seasons)
                {
                    if (season.TeamId != team.Id)
                    {
                        throw new HttpException((int) HttpStatusCode.BadRequest, "Cannot submit updates for non-matching team id");
                    }
                    _gameService.UpdateSeason(season);
                }
            }

            // Update events in delta update
            if (update.Events != null)
            {
                foreach (var teamEvent in update.Events)
                {
                    if (teamEvent.HomeTeamId != team.Id)
                    {
                        throw new HttpException((int) HttpStatusCode.BadRequest, "Cannot submit updates for non-matching team id");
                    }
                    _gameService.UpdateGate(teamEvent);

                    // And their duties
                    if (teamEvent.Duties != null)
                    {
                        foreach (var duty in teamEvent.Duties)
                        {
                            if (duty.EventId != teamEvent.Id)
                            {
                                throw new HttpException((int)HttpStatusCode.BadRequest, "Cannot submit updates for non-matching event id");    
                            }
                            _gameService.UpdateEventDuty(duty);
                        }
                    }
                }
            }

            // Update player groups in delta update
            if (update.PlayerGroups != null)
            {
                foreach (var playerGroup in update.PlayerGroups)
                {
                    if (playerGroup.TeamId != team.Id)
                    {
                        throw new HttpException((int)HttpStatusCode.BadRequest, "Cannot submit updates for non-matching team id");
                    }
                    _playerService.UpdatePlayerGroup(playerGroup);
                }
            }

            // Update players in delta update
            if (update.Players != null)
            {
                foreach (var player in update.Players)
                {
                    if (_playerService.GetPlayerGroup(player.GroupId).TeamId != team.Id)
                    {
                        throw new HttpException((int)HttpStatusCode.BadRequest, "Cannot submit updates for non-matching team id");
                    }
                    _playerService.UpdatePlayer(player);

                    // And their availabilities
                    if (player.Availability != null)
                    {
                        foreach (var ab in player.Availability)
                        {
                            _playerService.UpdatePlayerAvailability(player.Id, ab);
                        }
                    }
                }
            }

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
        public JsonResult UpdateSettings(int id, TeamSettingsViewModel teamSettings)
        {
            var activeUser = this.GetActiveUser(this.Request);
            if (!_teamService.CanEdit(id, activeUser.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to edit this team's settings");
            }

            if (id != teamSettings.TeamId)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Team ID mismatch");
            }

            teamSettings = _teamService.UpdateSettings(teamSettings);
            return Json(teamSettings);
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

            var otherTeams = _teamService.GetTeams(activeUser);
            if (otherTeams.Count() == 0)
            {
                activeUser.ActiveTeamId = null;
            }
            else
            {
                activeUser.ActiveTeamId = otherTeams.First().Id;
            }
            _userService.UpdateUser(activeUser);

            return Json(null);
        }

        [HttpPost]
        public JsonResult SendMessage(int id, Message message)
        {
            var activeUser = this.GetActiveUser(this.Request);
            var team = _teamService.GetTeam(id);
            if (!team.CanEdit(activeUser.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to send messages for this team");
            }

            if (message.RequestAvailability)
            {
                message.AvailabilityEvent = _gameService.GetGame(message.AvailabilityEventId);
            }

            // Check message for errors
            if (string.IsNullOrWhiteSpace(message.Subject))
            {
                if (message.AvailabilityEvent != null)
                {
                    Game.EventType type;
                    Enum.TryParse(message.AvailabilityEvent.Type.ToString(CultureInfo.InvariantCulture), out type);
                    message.Subject = EmailService.GetAvailabilitySubject(type, team.Name, message.AvailabilityEvent.OpponentName);
                }
                else
                {
                    Response.StatusCode = 400;
                    return Json("Message must have a subject");
                }
            }
            if (string.IsNullOrWhiteSpace(message.Content))
            {
                if (message.AvailabilityEvent == null)
                {
                    Response.StatusCode = 400;
                    return Json("Message must have some content");
                }
            }

            message.TeamId = id;
            message.TeamName = team.Name;
            message.From = activeUser.Email;

            message = new EmailService().EmailMessage(message);

            return Json(message);
        }

        [HttpDelete]
        public JsonResult DeleteMessage(int id, Message message)
        {
            var activeUser = this.GetActiveUser(this.Request);
            var team = _teamService.GetTeam(id);
            if (!team.CanEdit(activeUser.Id))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to send messages for this team");
            }

            message.TeamId = id;

            _teamService.RemoveMessage(message);

            return Json(message);
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
