using eMedLis.DAL.ReportPrint;
using eMedLis.DAL.ReportSettings;
using eMedLis.Models;
using eMedLis.Models.ReportPrint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace eMedLis.Controllers
{
    [Authorize]
    public class ReportPrintController : Controller
    {
        private readonly ReportPrintDB _db = new ReportPrintDB();
        private readonly ReportSettingsDB _settingsDb = new ReportSettingsDB();
        private readonly LabMasterDB _labDb = new LabMasterDB();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Entry(int billSummaryId)
        {
            ViewBag.BillSummaryId = billSummaryId;
            return View();
        }

        [HttpGet]
        public JsonResult SearchBills(string patientName, string mobileNo, string sampleBarcode, string billNo, DateTime? dateFrom, DateTime? dateTo, string subDepartment)
        {
            var data = _db.SearchAuthorizedBills(patientName, mobileNo, sampleBarcode, billNo, dateFrom, dateTo, subDepartment);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetBillInvestigations(int billSummaryId)
        {
            var data = _db.GetBillInvestigations(billSummaryId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult PreviewData(ReportPrintSelectionRequest request)
        {
            if (request == null || request.BillSummaryId <= 0 || request.SampleDetailIds == null || !request.SampleDetailIds.Any())
            {
                return Json(new { success = false, message = "Select at least one investigation" }, JsonRequestBehavior.AllowGet);
            }

            var selected = _db.GetSelectedAuthorized(request.BillSummaryId, request.SampleDetailIds);
            if (selected.Count == 0)
            {
                return Json(new { success = false, message = "Selected items are not in authorized status for this bill" }, JsonRequestBehavior.AllowGet);
            }

            var printOption = (request.PrintOption ?? "Individual").Trim();
            if (!string.Equals(printOption, "Grouped", StringComparison.OrdinalIgnoreCase))
            {
                printOption = "Individual";
            }

            object payload;
            if (printOption == "Grouped")
            {
                payload = selected
                    .GroupBy(x => x.DepartmentName ?? "Others")
                    .Select(g => new
                    {
                        GroupName = g.Key,
                        Items = g.ToList()
                    })
                    .ToList();
            }
            else
            {
                payload = selected
                    .Select(x => new
                    {
                        GroupName = x.InvestigationName,
                        Items = new List<ReportPrintSearchItem> { x }
                    })
                    .ToList();
            }

            return Json(new
            {
                success = true,
                printOption = printOption,
                groups = payload
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetPreviewData(ReportPrintSelectionRequest request)
        {
            if (request == null || request.BillSummaryId <= 0 || request.SampleDetailIds == null || !request.SampleDetailIds.Any())
            {
                return Json(new { success = false, message = "Select at least one investigation" }, JsonRequestBehavior.AllowGet);
            }

            var docs = _db.GetPreviewDocuments(request.BillSummaryId, request.SampleDetailIds);
            if (docs == null || docs.Count == 0)
            {
                return Json(new { success = false, message = "No authorized report data found for selected investigations" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = true,
                documents = docs,
                settings = _settingsDb.GetCurrent(),
                labProfile = _labDb.Get_Current()
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
