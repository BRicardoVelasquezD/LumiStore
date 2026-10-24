using System.Web.Optimization;

namespace Tienda
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            /* Organiza sección */
            BundleTable.EnableOptimizations = false;

            /* Organiza sección */
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"
            ));

            /* Organiza sección */
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate*"
            ));

            /* Organiza sección */
            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.bundle.min.js"
            ));

            /* Organiza sección */
            bundles.Add(new Bundle("~/bundles/complementos").Include(
                "~/Scripts/fontawesome/all.min.js",
                "~/Scripts/DataTables/jquery.dataTables.js",
                "~/Scripts/DataTables/dataTables.responsive.js",
                "~/Scripts/loadingoverlay/loadingoverlay.min.js",
                "~/Scripts/sweetalert2/sweetalert2.all.min.js",
                "~/Scripts/scripts.js"
            ));

            /* Organiza sección */
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/DataTables/css/jquery.dataTables.css",
                "~/Content/DataTables/css/responsive.dataTables.css",
                "~/Content/sweetalert2/sweetalert2.min.css",
                "~/Content/animate.min.css",
                "~/Content/site.css"
            ));
        }
    }
}
