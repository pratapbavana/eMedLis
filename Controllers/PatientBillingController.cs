using System;
using System.Web.Mvc;
using eMedLis.Models.PatientBilling; // For PatientBillViewModel
using eMedLis.DAL.PatientBilling;     // For PatientBillingDB

namespace eMedLis.Controllers
{
    public class PatientBillingController : Controller
    {
        // GET: PatientBilling (You might have an Index view for this controller)
        public ActionResult Index()
        {
            return View();
        }

        // POST: PatientBilling/SaveBill
        [HttpPost]
        public JsonResult SaveBill(PatientBillViewModel billData)
        {
            try
            {
                if (billData == null)
                {
                    return Json(new { success = false, message = "Invalid data received." });
                }

                PatientBillingDB db = new PatientBillingDB();
                int billSummaryId = db.SaveCompleteBill(billData); // This method handles the transaction

                if (billSummaryId > 0)
                {
                    return Json(new { success = true, message = "Bill saved successfully!", billId = billSummaryId });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to save bill. No Bill ID returned." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception details (e.g., using log4net, NLog, or a simple file logger)
                // In a real application, avoid sending detailed exception messages to the client.
                return Json(new { success = false, message = "An error occurred while saving the bill: " + ex.Message });
            }
        }

        // You can add other actions here later, e.g., for fetching bills, updating, etc.
    }
}