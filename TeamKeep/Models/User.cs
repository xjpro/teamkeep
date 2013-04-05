﻿using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using TeamKeep.Models.DataModels;

namespace TeamKeep.Models
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
            Reset = userData.Reset;
            ActiveTeamId = userData.ActiveTeamId;
            LastSeen = userData.LastSeen;
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        [ScriptIgnore]
        public string Password { get; set; }
        [ScriptIgnore]
        public string Reset { get; set; }
        public int? ActiveTeamId { get; set; }
        public DateTime? LastSeen { get; set; }

        public IEnumerable<Team> Teams { get; set; }
    }
}