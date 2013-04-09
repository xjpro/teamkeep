using System.Web.Mvc;
using TeamKeep.Models.ViewModels;
using TeamKeep.Services;

namespace TeamKeep.Controllers
{
    public class PublicController : ViewController
    {
        [HttpGet]
        [RedirectHttps]
        public ActionResult Index()
        {
            var user = this.GetActiveUser(this.Request);
            if (user != null)
            {
                return Redirect("/home");
            }
            return View();
        }

        [HttpGet]
        [RedirectHttps]
        public ActionResult PasswordReset()
        {
            var user = this.GetActiveUser(this.Request);
            return View(new BaseViewModel { User = user });
        }

        [HttpGet]
        public ActionResult PasswordResetSent()
        {
            var user = this.GetActiveUser(this.Request);
            return View(new BaseViewModel { User = user });
        }

        [HttpGet]
        public ActionResult AvailabilityLanding(string token)
        {
            var viewModel = new AvailabilityLandingViewModel();

            if(!string.IsNullOrEmpty(token))
            {
                viewModel.AvailabilityRequest = _playerService.GetAvailabilityRequest(token);
            }

            viewModel.User = this.GetActiveUser(this.Request);

            return View(viewModel);
        }

        [HttpGet]
        public ActionResult About()
        {
            var user = this.GetActiveUser(this.Request);
            return View(new BaseViewModel { User = user });
        }

        [HttpGet]
        public ActionResult Test()
        {
            new AutomatedEmailsJob().Execute(null);

            string debug;
            #if DEBUG
                debug = "debug";
            #else 
                debug = "release";
            #endif
            return Content(debug);
        }
    }
}
