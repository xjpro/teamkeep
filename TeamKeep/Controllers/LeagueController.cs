using System.Web.Http;
using Teamkeep.Models;

namespace Teamkeep.Controllers
{
    public class LeagueController : ApiController
    {
        public League GetLeagueById(int id)
        {
            return new League {Id = id, Name = "Hello, world!"};
        }
    }
}
