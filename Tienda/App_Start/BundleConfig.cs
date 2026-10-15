using System.Web.Optimization;

namespace Tienda
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            /*
             * IMPORTANTE:
             * - Se deshabilita la optimización para evitar errores con JS moderno
             * - NO se usa JsMinify en Bootstrap ni plugins
             * - Configuración estable para ASP.NET MVC 5
             */
            BundleTable.EnableOptimizations = false;

            /* ============================
             *  JQUERY (ES5 - SEGURO)
             * ============================ */
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"
            ));

            /* ============================
             *  VALIDACIONES (ES5)
             * ============================ */
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate*"
            ));

            /* ============================
             *  BOOTSTRAP 5 (JS MODERNO)
             *  ⚠️ USAR Bundle (NO ScriptBundle)
             * ============================ */
            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.bundle.min.js"
            ));

            /* ============================
             *  COMPLEMENTOS + JS PROPIO
             *  ⚠️ JS MODERNO → Bundle
             * ============================ */
            bundles.Add(new Bundle("~/bundles/complementos").Include(
                "~/Scripts/fontawesome/all.min.js",
                "~/Scripts/DataTables/jquery.dataTables.js",
                "~/Scripts/DataTables/dataTables.responsive.js",
                "~/Scripts/loadingoverlay/loadingoverlay.min.js",
                "~/Scripts/sweetalert2/sweetalert2.all.min.js",
                "~/Scripts/scripts.js"
            ));

            /* ============================
             *  ESTILOS CSS
             * ============================ */
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
