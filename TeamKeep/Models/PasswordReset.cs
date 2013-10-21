namespace Teamkeep.Models
{
    public class PasswordReset
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ResetToken { get; set; }
    }
}