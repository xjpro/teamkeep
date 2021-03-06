﻿using System.Net;
using System.Web.Mvc;
using Teamkeep.Models.ViewModels;

namespace Teamkeep.Controllers
{
    public class PublicController : ViewController
    {
        [HttpGet]
        [RedirectHttps]
        //[RedirectNonWww]
        public ActionResult Index()
        {
            var user = this.GetActiveUser(this.Request);
            if (user != null)
            {
                return Redirect(user.ActiveTeamId != null ? "/teams/" + user.ActiveTeamId : "/teams");
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
        public ActionResult FeaturesHomepage()
        {
            return View();
        }

        [HttpGet]
        public ActionResult FeaturesScheduleRoster()
        {
            return View();
        }

        [HttpGet]
        public ActionResult FeaturesAvailabilityMessaging()
        {
            return View();
        }

        [HttpGet]
        public ActionResult NotFound()
        {
            Response.StatusCode = (int) HttpStatusCode.NotFound;
            var user = this.GetActiveUser(this.Request);
            return View(new BaseViewModel { User = user });
        }

        [HttpGet]
        public ActionResult Sitemap()
        {
            return View(_teamService.GetPublicTeamUrls());
        }
    }
}
