using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TeamKeep.Models.DataModels;
using TeamKeep.Models.ViewModels;
using TeamKeep.Models;
using TeamKeep.Services;

namespace TeamKeep.Controllers
{
    public class UserController : ViewController
    {
        private readonly EmailService _emailService = new EmailService();

        [HttpPost]
        public JsonResult Create(User user)
        {
            if (string.IsNullOrEmpty(user.Username) || user.Username.Trim().Length < 3)
            {
                Response.StatusCode = 400;
                return Json("Username must be a minimum of three characters");
            }
            user.Username = user.Username.Trim();

            if (string.IsNullOrEmpty(user.Email) || !EmailService.IsValidEmail(user.Email.Trim()))
            {
                Response.StatusCode = 400;
                return Json("Please use a valid email address");
            }
            user.Email = user.Email.Trim();

            if (string.IsNullOrEmpty(user.Password) || user.Password.Length < 2)
            {
                Response.StatusCode = 400;
                return Json("Passwords must be a minimum of two characters");
            }
            var passwordHash = new PasswordHash(user.Password); // Encrypt the incoming password

            var serviceResponse = _userService.AddUser(new UserData { Username = user.Username, Email = user.Email }, passwordHash);
            if (serviceResponse.Error)
            {
                Response.StatusCode = 400;
                return Json(serviceResponse.Message);
            }

            // Perform first time login
            var login = new Login { Username = user.Username, Password = user.Password};
            user = _userService.GetUser(login);

            // Generate a new auth token
            var authToken = _userService.GetAuthToken(user.Id);

            // Redirect new user to home
            login.AuthToken = authToken;
            login.Redirect = "/home";
    
            _emailService.EmailWelcome(user.Email, user.Username, user.VerifyCode);

            return Json(login, JsonRequestBehavior.AllowGet);
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
        public JsonResult Active()
        {
            return Json(this.GetActiveUser(this.Request), JsonRequestBehavior.AllowGet);
        }

        [HttpPut]
        public JsonResult UpdateEmail(int id, string email)
        {
            var activeUser = this.GetActiveUser(this.Request);
            if (id != activeUser.Id)
            {
                throw new HttpException((int) HttpStatusCode.Unauthorized, "Not authorized to edit this user");
            }

            var user = _userService.UpdateUserEmail(activeUser.Id, email);
            //_emailService.EmailVerification(user.Email, user.VerifyCode);

            return Json(user);
        }

        [HttpPut]
        public JsonResult UpdateSettings(int id, UserSettingsData settings)
        {
            var activeUser = this.GetActiveUser(this.Request);
            if (id != activeUser.Id)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Not authorized to edit this user");
            }

            settings = _userService.UpdateSettings(id, settings);

            return Json(settings);
        }

        [HttpPut]
        public JsonResult PasswordChange(PasswordReset reset)
        {
            var user = _userService.GetUser(reset);
            if (user == null)
            {
                Response.StatusCode = 400;
                return Json("Invalid username");
            }

            if (user.Reset != reset.ResetToken)
            {
                Response.StatusCode = 400;
                return Json("Reset token is invalid, expired, or previously used");
            }

            if (string.IsNullOrEmpty(reset.Password) || reset.Password.Length < 2)
            {
                Response.StatusCode = 400;
                return Json("Passwords must be a minimum of two characters");
            }

            var passwordHash = new PasswordHash(reset.Password);
            _userService.UpdateUserPassword(user.Id, passwordHash);

            var authToken = _userService.GetAuthToken(user.Id);

            return Json(new Login { AuthToken = authToken, Redirect = "/home"});
        }

        [HttpPost]
        public JsonResult PasswordResetRequest(PasswordReset reset)
        {
            if (reset.Username.Trim().Length < 3)
            {
                Response.StatusCode = 400;
                return Json("Please enter your username");
            }
            reset.Username = reset.Username.Trim();

            var user = _userService.GetUser(reset);

            if (user == null)
            {
                Response.StatusCode = 404;
                return Json("That username is not registered");
            }

            var resetToken = AuthToken.GenerateKey(user.Email);
            _userService.SetResetHash(user.Id, resetToken);

            try
            {
                _emailService.EmailPasswordReset(user.Email, user.Username, resetToken);
            }
            catch (Exception)
            {
                Response.StatusCode = 500;
                return Json("Error sending reset email");
            }

            return Json("");
        }

        [HttpGet]
        public ActionResult Verify(string code)
        {
            _userService.VerifyEmail(code);

            var user = this.GetActiveUser(this.Request);
            return View("VerifyThanks", new BaseViewModel { User = user });
        }

        [HttpPost]
        public JsonResult VerifyResend()
        {
            var user = this.GetActiveUser(this.Request);
            if (user == null)
            {
                throw new HttpException((int) HttpStatusCode.Unauthorized, "No user is logged in");
            }

            if (user.Verified) return Json(false);
            
            _emailService.EmailVerification(user.Email, user.VerifyCode);
            
            return Json(true);
        }
    }
}
