using System.Net;
using System.Web.Mvc;
using TeamKeep.Models.ViewModels;

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

            viewModel.Title = (viewModel.AvailabilityRequest != null) ? viewModel.AvailabilityRequest.TeamName + " Event" : "Not found";
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
        public ActionResult Features()
        {
            var user = this.GetActiveUser(this.Request);
            return View(new BaseViewModel { User = user });
        }

        [HttpGet]
        public ActionResult NotFound()
        {
            Response.StatusCode = (int) HttpStatusCode.NotFound;
            var user = this.GetActiveUser(this.Request);
            return View(new BaseViewModel { User = user });
        }

        [HttpGet]
        public JsonResult Health()
        {
            return Json(_teamService.GetNumberOfTeams(), JsonRequestBehavior.AllowGet);
        }
    }
}
