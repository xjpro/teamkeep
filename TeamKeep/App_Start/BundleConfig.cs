using System.Web;
using System.Web.Optimization;

namespace TeamKeep.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            #if DEBUG
            BundleTable.EnableOptimizations = false;
            #else
            BundleTable.EnableOptimizations = true;
            #endif

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/site.css", 
                "~/Content/bootstrap.css", 
                "~/Content/bootstrap-responsive.css", 
                "~/Content/font-awesome.css"));

            bundles.Add(new ScriptBundle("~/Scripts/js").Include(
                "~/Scripts/jquery-ui-timepicker-addon.js",
                "~/Scripts/jquery.autosize.js",
                "~/Scripts/jquery.bottom-modal.js",
                "~/Scripts/knockout.mapping.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/lodash.js",
                "~/Scripts/moment.js",
                "~/Scripts/viewmodel.js",
                "~/Scripts/teamkeep.js",
                "~/Scripts/app/teamkeep.js",
                "~/Scripts/app/shared/spinner.js",
                "~/Scripts/app/shared/User.js",
                "~/Scripts/app/login/LoginController.js",
                "~/Scripts/app/login/RegisterController.js",
                "~/Scripts/app/tutorial/TutorialController.js"));
        }
    }
}