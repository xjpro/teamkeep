﻿using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using TeamKeep.App_Start;
using log4net;
using System.Web;
using System.Web.Optimization;

namespace TeamKeep
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
        }

        protected void Application_End()
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var error = Server.GetLastError();

            ILog log = LogManager.GetLogger("Log");
            log.Fatal("Exception: " + error.Message + " | Stack: " + error.StackTrace);

            if(Response.StatusCode == 404) 
            {
                HttpContext.Current.Server.TransferRequest("~/404");
            }
        }
    }
}