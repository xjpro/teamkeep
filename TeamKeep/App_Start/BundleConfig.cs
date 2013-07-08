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
                "~/Scripts/teamkeep.js",
                "~/Scripts/teamkeep-public.js",
                "~/Scripts/teamkeep-directives.js")
                .IncludeDirectory("~/Scripts/app/", "*.js", true)
                .IncludeDirectory("~/Scripts/app-public/", "*.js", true));
        }
    }
}