using System;
using System.Web.Mvc;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.RelyingParty;
using TeamKeep.Models.ViewModels;
using TeamKeep.Models;
using TeamKeep.Services;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using System.Web;

namespace TeamKeep.Controllers
{
    public class UserController : ViewController
    {
        private readonly EmailService _emailService = new EmailService();

        [HttpPost]
        public JsonResult Create(User user)
        {
            if (string.IsNullOrEmpty(user.Email) || !EmailService.IsValidEmail(user.Email))
            {
                Response.StatusCode = 400;
                return Json("Please use a valid email address");
            }
            user.Email = user.Email.Trim();

            if (string.IsNullOrEmpty(user.Password) || user.Password.Length < 4)
            {
                Response.StatusCode = 400;
                return Json("Passwords must be a minimum of four characters");
            }

            // Encrypt the incoming password
            var passwordHash = new PasswordHash(user.Password);

            var serviceResponse = _userService.AddUser(user, passwordHash);
            if (serviceResponse.Error)
            {
                Response.StatusCode = 400;
                return Json(serviceResponse.Message);
            }

            // Perform first time login
            var login = new Login { Email = user.Email, Password = user.Password};
            user = _userService.GetUser(login);

            // Generate a new auth token
            var authToken = _userService.GetAuthToken(user.Id);

            // Redirect new user to home
            login.AuthToken = authToken;
            login.Redirect = "/home";
    
            _emailService.EmailWelcome(user.Email);

            return Json(login, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Login(Login login)
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
            login.Redirect = "/home";

            return Json(login);
        }

        [HttpGet]
        public ActionResult LoginOpenId(string provider)
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

                        var user = _userService.GetUser(openid, email);
                        var authToken = _userService.GetAuthToken(user.Id);

                        return View("OpenIdComplete", new Login { AuthToken = authToken, Redirect = "/home" });
                    }
                    else
                    {
                        // Not sure...
                    }
                }
                else
                {
                    string providerUrl;

                    if (provider.Equals("google")) providerUrl = "https://www.google.com/accounts/o8/id";
                    else if (provider.Equals("facebook")) providerUrl = "https://www.google.com/accounts/o8/id";
                    else throw new HttpException((int) 400, "Invalid open id provider");

                    var request = openAuth.CreateRequest(providerUrl);
                    request.AddExtension(new ClaimsRequest
                    {
                        Email = DemandLevel.Require
                    });
                    request.RedirectToProvider();
                }
            }

            return View();
        }

        [HttpPut]
        public JsonResult PasswordChange(PasswordReset reset)
        {
            var user = _userService.GetUser(reset);
            if (user == null)
            {
                Response.StatusCode = 400;
                return Json("Invalid email");
            }

            if (user.Reset != reset.ResetToken)
            {
                Response.StatusCode = 400;
                return Json("Invalid reset token");
            }

            if (string.IsNullOrEmpty(reset.Password) || reset.Password.Length < 4)
            {
                Response.StatusCode = 400;
                return Json("Passwords must be a minimum of four characters");
            }

            var passwordHash = new PasswordHash(reset.Password);
            _userService.UpdateUserPassword(user.Id, passwordHash);

            var authToken = _userService.GetAuthToken(user.Id);

            return Json(new Login { AuthToken = authToken, Redirect = "/home"});
        }

        [HttpGet]
        public ActionResult Home(string token)
        {
            var activeUser = this.GetActiveUser(this.Request);
            if (activeUser == null)
            {
                Response.Redirect("/");
                return null;
            }

            activeUser.LastSeen = DateTime.Now;
            activeUser = _userService.UpdateUser(activeUser);

            var viewModel = new UserHomeViewModel { User = activeUser };
            return View("Home", viewModel);
        }

        [HttpPost]
        public JsonResult PasswordReset(PasswordReset reset)
        {
            if (!EmailService.IsValidEmail(reset.Email))
            {
                Response.StatusCode = 400;
                return Json("Please enter your email address");
            }

            var user = _userService.GetUser(reset);

            if (user == null)
            {
                Response.StatusCode = 404;
                return Json("That email is not registered");
            }

            reset.ResetToken = AuthToken.GenerateKey(user.Email);
            _userService.SetResetHash(user.Id, reset.ResetToken);

            // Make this async or something? it seems to fail sending back or something...
            try
            {
                _emailService.EmailPassword(reset);
            }
            catch (Exception)
            {
                // TODO log this
                Response.StatusCode = 500;
                return Json("Error sending reset email");
            }

            return Json("");
        }
    }
}
