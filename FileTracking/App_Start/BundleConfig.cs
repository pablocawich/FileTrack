using System.Web;
using System.Web.Optimization;

namespace FileTracking
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
           // bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                       // "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/lib").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/metisMenu.min.js",
                "~/Scripts/startmin.js",
                "~/Scripts/raphael.min.js",
                "~/Scripts/startmin.js",
                "~/Scripts/morris.min.js",
                "~/Scripts/morris-data.js"
               ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-social.css",
                      "~/Content/metisMenu.min.css",
                      "~/Content/startmin.css",
                      "~/Content/font-awesome.min.css",
                      "~/Content/timeline.css",
                      "~/Content/morris.css",
                      "~/Content/site.css"));
        }
    }
}
