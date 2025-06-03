using System.Web;
using System.Web.Optimization;

namespace eMedLis
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/datatable").Include(
                    "~/plugins/datatables/jquery.dataTables.min.js",
                    "~/plugins/datatables-bs4/js/dataTables.bootstrap4.min.js",
                    "~/plugins/datatables-responsive/js/dataTables.responsive.min.js",
                    "~/plugins/datatables-responsive/js/responsive.bootstrap4.min.js",
                    "~/plugins/datatables-buttons/js/dataTables.buttons.min.js",
                    "~/plugins/datatables-buttons/js/buttons.bootstrap4.min.js",
                    "~/plugins/jszip/jszip.min.js",
                    "~/plugins/pdfmake/pdfmake.min.js",
                    "~/plugins/pdfmake/vfs_fonts.js",
                    "~/plugins/datatables-buttons/js/buttons.html5.min.js",
                    "~/plugins/datatables-buttons/js/buttons.print.min.js",
                    "~/plugins/datatables-buttons/js/buttons.colVis.min.js"
    ));

            bundles.Add(new ScriptBundle("~/bundles/uijs").Include(
                "~/Plugins/toastr/toastr.min.js",
                "~/Plugins/select2/js/select2.full.min.js"
                ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/datatablecss").Include(
                      "~/plugins/datatables-bs4/css/dataTables.bootstrap4.min.css",
                      "~/plugins/datatables-responsive/css/responsive.bootstrap4.min.css",
                      "~/plugins/datatables-buttons/css/buttons.bootstrap4.min.css"
                      ));

            bundles.Add(new StyleBundle("~/Content/uicss").Include(
                     "~/Plugins/toastr/toastr.min.css",
                     "~/Plugins/select2/css/select2.min.css",
                     "~/Plugins/icheck-bootstrap/icheck-bootstrap.min.css"
                     ));
        }
    }
}
