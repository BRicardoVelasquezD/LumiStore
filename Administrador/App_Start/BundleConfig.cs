using System.Web.Optimization;

namespace Administrador
{
    public class BundleConfig
    {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // JQUERY
            bundles.Add(new Bundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            // COMPLEMENTOS (Scripts de DataTables, FontAwesome y Lógica del Template)
            bundles.Add(new Bundle("~/bundles/complementos").Include(
                        "~/Scripts/fontawesome/all.min.js",
                        "~/Scripts/DataTables/jquery.dataTables.js",
                        "~/Scripts/DataTables/dataTables.responsive.js",
                        "~/Scripts/loadingoverlay.min.js",
                        "~/Scripts/sweetalert2.all.min.js",
                        "~/Scripts/scripts.js"));

            // VALIDACIONES (Descomentado por si necesitas validaciones en formularios)
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // BOOTSTRAP (Bundle incluye Popper.js necesario para Dropdowns)
            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.bundle.js"));

            // ESTILOS CSS
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/DataTables/css/jquery.dataTables.css",
                      "~/Content/DataTables/css/responsive.dataTables.css",
                      "~/Content/sweetalert2.min.css",
                      "~/Content/animate.min.css",
                      "~/Content/site.css"));
        }
    }
}
