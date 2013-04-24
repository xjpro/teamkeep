using System.Web.Mvc;
using System.Web.Routing;

namespace TeamKeep.App_Start
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Public views
            routes.MapRoute("PublicIndex", "", new { controller = "Public", action = "Index" });
            routes.MapRoute("PublicPasswordReset", "reset", new { controller = "Public", action = "PasswordReset" });
            routes.MapRoute("PublicPasswordResetSent", "resetsent", new { controller = "Public", action = "PasswordResetSent" });
            routes.MapRoute("PublicAvailability", "rsvp", new { controller = "Public", action = "AvailabilityLanding" }, new { httpMethod = new HttpMethodConstraint("GET") });
            routes.MapRoute("PublicAbout", "about", new { controller = "Public", action = "About" });
            routes.MapRoute("PublicTest", "test", new { controller = "Public", action = "Test" });

            // User
            routes.MapRoute("UserCreate", "users", new { controller = "User", action = "Create" }, new { httpMethod = new HttpMethodConstraint("POST") });
            routes.MapRoute("UserLogin", "login", new { controller = "User", action = "Login" }, new { httpMethod = new HttpMethodConstraint("POST") });
            routes.MapRoute("UserPasswordChange", "users/password", new { controller = "User", action = "PasswordChange" }, new { httpMethod = new HttpMethodConstraint("PUT") });
            routes.MapRoute("UserPasswordReset", "users/password", new { controller = "User", action = "PasswordReset" }, new { httpMethod = new HttpMethodConstraint("POST") });
            routes.MapRoute("UserHome", "home", new { controller = "User", action = "Home" }, new { httpMethod = new HttpMethodConstraint("GET") });

            // Team
            routes.MapRoute("TeamCreate", "teams", new { controller = "Team", action = "Create" }, new { httpMethod = new HttpMethodConstraint("POST") });
            routes.MapRoute("TeamHome", "teams/{teamId}/{teamName}", new { controller = "Team", action = "Home" }, new { httpMethod = new HttpMethodConstraint("GET")});
            routes.MapRoute("TeamHomeRedirect", "teams/{teamId}", new { controller = "Team", action = "HomeRedirect" }, new { httpMethod = new HttpMethodConstraint("GET") });
            routes.MapRoute("TeamSendMessage", "teams/{id}/{teamName}/messages", new { controller = "Team", action = "SendMessage" }, new { httpMethod = new HttpMethodConstraint("POST") });
            routes.MapRoute("TeamUpdateAnnouncement", "teams/{id}/{teamName}/announcement", new { controller = "Team", action = "UpdateAnnouncement" }, new { httpMethod = new HttpMethodConstraint("PUT") });
            routes.MapRoute("TeamUpdateSettings", "teams/{id}/{teamName}/settings", new { controller = "Team", action = "UpdateSettings" }, new { httpMethod = new HttpMethodConstraint("PUT") });
            routes.MapRoute("TeamUpdateBanner", "teams/{id}/{teamName}/banner", new { controller = "Team", action = "UpdateBanner" }, new { httpMethod = new HttpMethodConstraint("PUT") });
            routes.MapRoute("TeamDelete", "teams/{id}/{teamName}", new { controller = "Team", action = "Delete" }, new { httpMethod = new HttpMethodConstraint("DELETE") });

            // Players
            routes.MapRoute("PlayerGroupCreate", "teams/{teamId}/{teamName}/groups", new { controller = "Player", action = "CreateGroup" }, new { httpMethod = new HttpMethodConstraint("POST") });
            routes.MapRoute("PlayerGroupUpdate", "teams/{teamId}/{teamName}/groups/{id}", new { controller = "Player", action = "UpdateGroup" }, new { httpMethod = new HttpMethodConstraint("PUT") });
            routes.MapRoute("PlayerGroupDelete", "teams/{teamId}/{teamName}/groups/{id}", new { controller = "Player", action = "DeleteGroup" }, new { httpMethod = new HttpMethodConstraint("DELETE") });
            routes.MapRoute("PlayerCreate", "teams/{teamId}/{teamName}/players", new { controller = "Player", action = "Create" }, new { httpMethod = new HttpMethodConstraint("POST") });
            routes.MapRoute("PlayerUpdate", "teams/{teamId}/{teamName}/players/{id}", new { controller = "Player", action = "Update" }, new { httpMethod = new HttpMethodConstraint("PUT") });
            routes.MapRoute("PlayerDelete", "teams/{teamId}/{teamName}/players/{id}", new { controller = "Player", action = "Delete" }, new { httpMethod = new HttpMethodConstraint("DELETE") });

            // Availability
            routes.MapRoute("PlayerAvailabilityUpdate", "teams/{teamId}/{teamName}/players/{playerId}/availability", new { controller = "Player", action = "UpdateAvailability" }, 
                new { httpMethod = new HttpMethodConstraint("PUT") });
            routes.MapRoute("PlayerAvailabilityRsvp", "rsvp", new { controller = "Player", action = "SetAvailability" },
                new { httpMethod = new HttpMethodConstraint("PUT") });

            // Seasons
            routes.MapRoute("SeasonCreate", "teams/{teamId}/{teamName}/seasons", new { controller = "Game", action = "CreateSeason" }, new { httpMethod = new HttpMethodConstraint("POST") });
            routes.MapRoute("SeasonUpdate", "teams/{teamId}/{teamName}/seasons/{id}", new { controller = "Game", action = "UpdateSeason" }, new { httpMethod = new HttpMethodConstraint("PUT") });
            routes.MapRoute("SeasonDelete", "teams/{teamId}/{teamName}/seasons/{id}", new { controller = "Game", action = "DeleteSeason" }, new { httpMethod = new HttpMethodConstraint("DELETE") });

            // Games
            routes.MapRoute("GameCreate", "games", new { controller = "Game", action = "Create" }, new { httpMethod = new HttpMethodConstraint("POST") });
            routes.MapRoute("GameUpdate", "games/{gameId}", new { controller = "Game", action = "Update" }, new { httpMethod = new HttpMethodConstraint("PUT") });
            routes.MapRoute("GameDelete", "games/{gameId}", new { controller = "Game", action = "Delete" }, new { httpMethod = new HttpMethodConstraint("DELETE") });
            routes.MapRoute("GameConfirmations", "games/{gameId}/confirmations", new { controller = "Game", action = "SendConfirmations" }, new { httpMethod = new HttpMethodConstraint("POST") });
        }
    }
}