using System.Configuration;
using Teamkeep.Models.DataModels;

namespace Teamkeep.Services
{
    public static class Database
    {
        public static DatabaseEntities GetEntities()
        {
            return new DatabaseEntities(ConfigurationManager.ConnectionStrings["DatabaseEntities"].ConnectionString);
        }
    }
}