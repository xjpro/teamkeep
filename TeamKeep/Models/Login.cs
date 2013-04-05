using System.Web.Script.Serialization;

namespace TeamKeep.Models
{
    public class Login
    {
        public string Email { get; set; }
        [ScriptIgnore]
        public string Password { get; set; }
        public AuthToken AuthToken { get; set; }
        public string Redirect { get; set; }
    }
}