using Teamkeep.Models.DataModels;

namespace Teamkeep.Models.ViewModels
{
    public class TeamSettingsViewModel
    {
        public int TeamId { get; set; }
        public string Name { get; set; }

        public TeamPrivacyData Privacy { get; set; }
        public TeamSettingsData Settings { get; set; }
    }
}