using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using TeamKeep.App_Start;
using TeamKeep.Services;

namespace TeamKeep
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();

            AutomatedScheduler.Start();
        }

        protected void Application_End()
        {
            AutomatedScheduler.Stop();
        }
    }
}