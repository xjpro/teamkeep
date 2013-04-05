using System.Web.Http;
using TeamKeep.Models;

namespace TeamKeep.Controllers
{
    public class LeagueController : ApiController
    {
        public League GetLeagueById(int id)
        {
            return new League {Id = id, Name = "Hello, world!"};
        }
    }
}
