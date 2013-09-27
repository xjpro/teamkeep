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
                "~/Content/bootstrap.css", 
                "~/Content/angular-clockpicker.css",
                "~/Content/font-awesome.css",
                "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/Scripts/js").Include(
                "~/Scripts/angular-route.js",
                "~/Scripts/angular-sanitize.js",
                "~/Scripts/lodash.js",
                "~/Scripts/moment.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/angular-clockpicker.js",
                "~/Scripts/ui-bootstrap.js",
                "~/Scripts/app/teamkeep.js",
                "~/Scripts/app/teamkeep-directives.js")
                .IncludeDirectory("~/Scripts/app/team", "*.js"));
        }
    }
}