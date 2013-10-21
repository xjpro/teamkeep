using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Teamkeep.Models.DataModels;

namespace Teamkeep.Models
{
    public class User
    {
        public User()
        {
        }

        public User(UserData userData)
        {
            Id = userData.Id;
            Username = userData.Username;
            Email = userData.Email;
            VerifyCode = userData.Verify;
            Reset = userData.Reset;
            ActiveTeamId = userData.ActiveTeamId;
            LastSeen = userData.LastSeen;
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int? ActiveTeamId { get; set; }
        public DateTime? LastSeen { get; set; }
        public UserSettingsData Settings { get; set; }

        [ScriptIgnore]
        public string VerifyCode { get; set; }
        public bool Verified { get { return VerifyCode == null; } }

        [ScriptIgnore]
        public string Password { get; set; }
        [ScriptIgnore]
        public string Reset { get; set; }

        public IEnumerable<Team> Teams { get; set; }
    }
}