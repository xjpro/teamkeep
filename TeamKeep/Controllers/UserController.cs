using System;
using System.Web.Mvc;
using TeamKeep.Models.ViewModels;
using TeamKeep.Models;
using System.Text.RegularExpressions;
using TeamKeep.Services;

namespace TeamKeep.Controllers
{
    public class UserController : ViewController
    {
        private readonly EmailService _emailService = new EmailService();

        [HttpPost]
        public JsonResult Create(User user)
        {
            if (string.IsNullOrEmpty(user.Email) || !IsValidEmail(user.Email))
            {
                Response.StatusCode = 400;
                return Json("Please use a valid e-mail address");
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

            try
            {
                _emailService.EmailWelcome(user.Email);
                //_emailService.SendAllQueuedEmails();
            }
            catch (Exception)
            {
                // Swallow this and TODO log it
            }

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

        [HttpPut]
        public JsonResult PasswordChange(PasswordReset reset)
        {
            var user = _userService.GetUser(reset);
            if (user == null)
            {
                Response.StatusCode = 400;
                return Json("Invalid e-mail");
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
        public ActionResult Home()
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

        [HttpGet]
        public ActionResult EditProfile()
        {
            return View();
        }

        [HttpGet]
        public ActionResult EditSettings()
        {
            return View();
        }

        [HttpPost]
        public JsonResult PasswordReset(PasswordReset reset)
        {
            if (!IsValidEmail(reset.Email))
            {
                Response.StatusCode = 400;
                return Json("Please enter your e-mail address");
            }

            var user = _userService.GetUser(reset);

            if (user == null)
            {
                Response.StatusCode = 404;
                return Json("That e-mail is not registered");
            }

            reset.ResetToken = AuthToken.Generate(user.Id, user.Email).Key;
            _userService.SetResetHash(user.Id, reset.ResetToken);

            // Make this async or something? it seems to fail sending back or something...
            try
            {
                _emailService.EmailPassword(reset);
                //_emailService.SendAllQueuedEmails();
            }
            catch (Exception)
            {
                // TODO log this
                Response.StatusCode = 500;
                return Json("Error sending reset e-mail");
            }

            return Json("");
        }

        private bool IsValidEmail(string test)
        {
            if (string.IsNullOrEmpty(test)) return false;
            return new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+(?:[A-Z]{2}|com|org|net|edu|gov|mil|biz|info|mobi|name|aero|asia|jobs|museum)\b", RegexOptions.IgnoreCase).IsMatch(test.Trim());
        }
    }
}
