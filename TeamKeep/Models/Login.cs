using System.Web.Script.Serialization;

namespace TeamKeep.Models
{
    public class Login
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string UniqueId { get; set; }
        [ScriptIgnore]
        public string Password { get; set; }
        public AuthToken AuthToken { get; set; }
        public string Redirect { get; set; }
    }
}