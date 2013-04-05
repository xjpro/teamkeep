namespace TeamKeep.Models
{
    public class PasswordReset
    {
        public string Email { get; set; }
        public string ResetToken { get; set; }
        public string Password { get; set; }
    }
}