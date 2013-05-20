using System.Web.Script.Serialization;

namespace TeamKeep.Models
{
    public class FacebookLogin
    {
        public string FacebookId { get; set; }

        public AuthToken AuthToken { get; set; }
        public string Redirect { get; set; }
    }
}