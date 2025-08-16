using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace eMedLis
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
            name: "PrintBill",
            url: "PatientBilling/PrintBill/{billId}",
            defaults: new { controller = "PatientBilling", action = "PrintBill" },
            constraints: new { billId = @"\d+" }
        );
            routes.MapRoute(
            name: "PrintBillModal",
            url: "PatientBilling/PrintBillModal/{billId}",
            defaults: new { controller = "PatientBilling", action = "PrintBillModal" },
            constraints: new { billId = @"\d+" }
        );
           routes.MapRoute(
           name: "ExportBillPDF",
           url: "PatientBilling/ExportBillPDF/{billId}",
           defaults: new { controller = "PatientBilling", action = "ExportBillPDF" },
           constraints: new { billId = @"\d+" }
       );
           routes.MapRoute(
           name: "ViewBill",
           url: "PatientBilling/ViewBill/{billId}",
           defaults: new { controller = "PatientBilling", action = "ViewBill" },
           constraints: new { billId = @"\d+" }
       );
            routes.MapRoute(
           name: "PrintReceipt",
           url: "DuePayment/PrintReceipt/{paymentId}",
           defaults: new { controller = "DuePayment", action = "PrintReceipt" },
           constraints: new { paymentId = @"\d+" }
       );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
