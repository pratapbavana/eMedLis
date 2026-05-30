using eMedLis.DAL.ReportEntry;
using eMedLis.Models.ReportEntry;
using System;
using System.Web.Mvc;

namespace eMedLis.Controllers
{
    [Authorize]
    public class ReportEntryController : Controller
    {
        private readonly ReportEntryDB _db = new ReportEntryDB();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Entry(int sampleCollectionId)
        {
            ViewBag.SampleCollectionId = sampleCollectionId;
            return View();
        }

        [HttpGet]
        public JsonResult SearchOrders(string billNo, string sampleBarcode, string patientName, string mobileNo, DateTime? dateFrom, DateTime? dateTo)
        {
            var data = _db.SearchOrders(billNo, sampleBarcode, patientName, mobileNo, dateFrom, dateTo);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetOrderSummary(int sampleCollectionId)
        {
            var data = _db.GetOrderSummary(sampleCollectionId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetInvestigations(int sampleCollectionId)
        {
            var data = _db.GetInvestigations(sampleCollectionId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult LoadTemplate(int sampleDetailId)
        {
            var data = _db.LoadTemplate(sampleDetailId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult LoadRanges(int sampleDetailId, int methodId)
        {
            if (methodId <= 0)
            {
                return Json(new { success = false, message = "Method is required" }, JsonRequestBehavior.AllowGet);
            }

            var data = _db.LoadRanges(sampleDetailId, methodId);
            return Json(new { success = true, ranges = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult LoadParameterRange(int sampleDetailId, int parameterId, int methodId)
        {
            if (methodId <= 0 || parameterId <= 0)
            {
                return Json(new { success = false, message = "Parameter and method are required" }, JsonRequestBehavior.AllowGet);
            }

            var data = _db.LoadParameterRange(sampleDetailId, parameterId, methodId);
            return Json(new { success = true, range = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveDraft(ReportEntrySaveRequest request)
        {
            if (request == null || request.SampleDetailId <= 0)
            {
                return Json(new Tuple<int, string>(0, "Invalid request"), JsonRequestBehavior.AllowGet);
            }

            var result = _db.SaveResults(request, "Draft");
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SubmitForAuthorization(ReportEntrySaveRequest request)
        {
            if (request == null || request.SampleDetailId <= 0)
            {
                return Json(new Tuple<int, string>(0, "Invalid request"), JsonRequestBehavior.AllowGet);
            }

            var result = _db.SaveResults(request, "Pending Authorization");
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
