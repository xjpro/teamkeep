using System.Web.Mvc;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.RelyingParty;
using TeamKeep.Models;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using System.Web;

namespace TeamKeep.Controllers
{
    public class LoginController : ViewController
    {
        [HttpPost]
        public JsonResult Default(Login login)
        {
            var user = _userService.GetUser(login);
            if (user == null)
            {
                Response.StatusCode = 400;
                return Json("Invalid username or password");
            }

            // Create and save a token for the user, for use in future requests
            var authToken = _userService.GetAuthToken(user.Id);

            login.AuthToken = authToken;
            login.Redirect = (user.ActiveTeamId != null) ? "/teams/" + user.ActiveTeamId : "/home";

            return Json(login);
        }

        [HttpGet]
        public ActionResult OpenId(string provider)
        {
            using (var openAuth = new OpenIdRelyingParty())
            {
                var response = openAuth.GetResponse();
                if (response != null)
                {
                    if (response.Status == AuthenticationStatus.Authenticated)
                    {
                        string openid = response.ClaimedIdentifier;
                        string email = null;

                        var fetch = response.GetExtension<FetchResponse>();
                        if (fetch != null)
                        {
                            email = fetch.GetAttributeValue(WellKnownAttributes.Contact.Email);
                        }

                        var login = new Login { UniqueId = openid, Email = email };

                        var user = _userService.GetUser(login);
                        var authToken = _userService.GetAuthToken(user.Id);
                        var redirectPath = (user.ActiveTeamId != null) ? "/teams/" + user.ActiveTeamId : "/home";

                        return View("OpenIdComplete", new Login { AuthToken = authToken, Redirect = redirectPath });
                    }
                    else
                    {
                        // TODO Not sure...
                    }
                }
                else
                {
                    string providerUrl;

                    if (provider.Equals("google")) providerUrl = "https://www.google.com/accounts/o8/id";
                    //else if (provider.Equals("abc")) providerUrl = "";
                    else throw new HttpException(400, "Invalid open id provider");

                    var request = openAuth.CreateRequest(providerUrl);
                    request.AddExtension(new ClaimsRequest
                    {
                        Email = DemandLevel.Require
                    });
                    request.RedirectToProvider();
                    Response.End();
                }
            }

            return null;
        }

    }
}
