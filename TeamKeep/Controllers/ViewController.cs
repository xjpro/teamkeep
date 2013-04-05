using System.Web;
using System.Web.Mvc;
using TeamKeep.Models;
using TeamKeep.Services;

namespace TeamKeep.Controllers
{
    public abstract class ViewController : Controller
    {
        protected readonly UserService _userService = new UserService();
        protected readonly TeamService _teamService = new TeamService();

        protected User GetActiveUser(HttpRequestBase request)
        {
            var token = AuthToken.Generate(request.Cookies);
            if (token == null) return null; // No token

            var user = _userService.GetUser(token);
            if (user == null) return null; // Failed auth

            user.Teams = _teamService.GetTeams(user);
            return user;
        }
    }
}
