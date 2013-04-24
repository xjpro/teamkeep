using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using TeamKeep.Models.DataModels;
using System.Collections.Generic;

namespace TeamKeep.Models
{
    public class Team : TeamData
    {
        public Team()
        {
        }

        public Team(TeamData teamData)
        {
            Id = teamData.Id;
            Name = teamData.Name;
            Announcement = teamData.Announcements;
            BannerImage = teamData.Banner;
            ResultsView = teamData.ResultsView;
            Postal = teamData.Postal;
        }

        public string Url { get { return "/teams/" + Id + "/" + new Regex("[^A-Za-z0-9]").Replace(Name, ""); } }

        public string Announcement { get; set; }
        public string BannerImage { get; set; }
        [ScriptIgnore]
        public List<User> Owners { get; set; }
        public List<Message> Messages { get; set; } 
        public List<PlayerGroup> PlayerGroups { get; set; }
        public List<Season> Seasons { get; set; }
        public TeamSettingsData Settings { get; set; }
        public TeamPrivacyData Privacy { get; set; }
        public bool Editable { get; set; }

        public bool CanEdit(int userId)
        {
            return Owners != null && Owners.Exists(x => x.Id == userId);
        }
    }
}