using System.Configuration;
using TeamKeep.Models.DataModels;

namespace TeamKeep.Services
{
    public static class Database
    {
        public static DatabaseEntities GetEntities()
        {
            return new DatabaseEntities(ConfigurationManager.ConnectionStrings["DatabaseEntities"].ConnectionString);
        }
    }
}