using System.Web.Mvc;
using System.Web.Routing;

namespace Teamkeep.App_Start
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
            routes.MapRoute("PublicFeatures", "features", new { controller = "Public", action = "Features" });
            routes.MapRoute("PublicFeaturesHomepage", "features/free-team-website", new { controller = "Public", action = "FeaturesHomepage" });
            routes.MapRoute("PublicNotFound", "404", new { controller = "Public", action = "NotFound" });
            routes.MapRoute("Sitemap", "sitemap.xml", new { controller = "Public", action = "Sitemap" });

            // User
            routes.MapRoute("UserCreate", "users", new { controller = "User", action = "Create" }, new { httpMethod = new HttpMethodConstraint("POST") });
            routes.MapRoute("UserActive", "users/active", new { controller = "User", action = "Active" }, new { httpMethod = new HttpMethodConstraint("GET") });
            routes.MapRoute("UserUpdateEmail", "users/{id}/email", new { controller = "User", action = "UpdateEmail" }, new { httpMethod = new HttpMethodConstraint("PUT") });
            routes.MapRoute("UserUpdateSettings", "users/{id}/settings", new { controller = "User", action = "UpdateSettings" }, new { httpMethod = new HttpMethodConstraint("PUT") });
            routes.MapRoute("UserPasswordChange", "users/password", new { controller = "User", action = "PasswordChange" }, new { httpMethod = new HttpMethodConstraint("PUT") });
            routes.MapRoute("UserPasswordResetRequest", "users/password", new { controller = "User", action = "PasswordResetRequest" }, new { httpMethod = new HttpMethodConstraint("POST") });
            routes.MapRoute("UserVerify", "users/verify", new { controller = "User", action = "Verify" }, new { httpMethod = new HttpMethodConstraint("GET") });
            routes.MapRoute("UserVerifyResend", "users/verify/resend", new { controller = "User", action = "VerifyResend" }, new { httpMethod = new HttpMethodConstraint("POST") });
            routes.MapRoute("UserHome", "home", new { controller = "User", action = "Home" }, new { httpMethod = new HttpMethodConstraint("GET") });

            // Login
            routes.MapRoute("Login", "login", new { controller = "Login", action = "Default" }, new { httpMethod = new HttpMethodConstraint("POST") });
            routes.MapRoute("LoginFacebook", "login/facebook", new { controller = "Login", action = "Default" }, new { httpMethod = new HttpMethodConstraint("POST") });
            routes.MapRoute("LoginOpenId", "login/openid/{provider}", new { controller = "Login", action = "OpenId" }, new { httpMethod = new HttpMethodConstraint("GET") });

            // Team
            routes.MapRoute("TeamCreate", "teams", new { controller = "Team", action = "Create" }, new { httpMethod = new HttpMethodConstraint("POST") });
            routes.MapRoute("TeamHome", "teams/{teamId}/{teamName}", new { controller = "Team", action = "Home" }, new { httpMethod = new HttpMethodConstraint("GET")});
            routes.MapRoute("TeamRedirect", "teams/{teamId}", new { controller = "Team", action = "HomeRedirect" }, new { httpMethod = new HttpMethodConstraint("GET") });
            routes.MapRoute("TeamHomeEmpty", "teams", new { controller = "Team", action = "HomeEmpty" }, new { httpMethod = new HttpMethodConstraint("GET") });
            routes.MapRoute("TeamSendMessage", "teams/{id}/{teamName}/messages", new { controller = "Team", action = "SendMessage" }, new { httpMethod = new HttpMethodConstraint("POST") });
            routes.MapRoute("TeamDeleteMessage", "teams/{id}/{teamName}/messages", new { controller = "Team", action = "DeleteMessage" }, new { httpMethod = new HttpMethodConstraint("DELETE") });
            routes.MapRoute("TeamUpdate", "teams/{id}/{teamName}", new { controller = "Team", action = "Update" }, new { httpMethod = new HttpMethodConstraint("PUT") });
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

            // Events
            routes.MapRoute("GameCreate", "games", new { controller = "Game", action = "Create" }, new { httpMethod = new HttpMethodConstraint("POST") });
            routes.MapRoute("GameUpdate", "games/{gameId}", new { controller = "Game", action = "Update" }, new { httpMethod = new HttpMethodConstraint("PUT") });
            routes.MapRoute("GameDelete", "games/{gameId}", new { controller = "Game", action = "Delete" }, new { httpMethod = new HttpMethodConstraint("DELETE") });
            routes.MapRoute("DutyCreate", "teams/{teamId}/{teamName}/events/{eventId}/duties", new { controller = "EventDuty", action = "Create" }, new { httpMethod = new HttpMethodConstraint("POST") });
            routes.MapRoute("DutyDelete", "teams/{teamId}/{teamName}/events/{eventId}/duties/{id}", new { controller = "EventDuty", action = "Delete" }, new { httpMethod = new HttpMethodConstraint("DELETE") });

            // Default
            routes.MapRoute("Default", "{*whatever}", new { controller = "Public", action = "NotFound" }, new { httpMethod = new HttpMethodConstraint("GET") });
        }
    }
}