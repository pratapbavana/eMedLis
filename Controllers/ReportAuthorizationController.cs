using eMedLis.DAL.ReportAuthorization;
using eMedLis.Models.ReportAuthorization;
using System;
using System.Web.Mvc;

namespace eMedLis.Controllers
{
    [Authorize]
    public class ReportAuthorizationController : Controller
    {
        private readonly ReportAuthorizationDB _db = new ReportAuthorizationDB();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Review(int sampleDetailId)
        {
            ViewBag.SampleDetailId = sampleDetailId;
            return View();
        }

        [HttpGet]
        public JsonResult Search(DateTime? dateFrom, DateTime? dateTo, string patientName, string sampleBarcode, string investigation, bool criticalOnly = false)
        {
            var userName = User?.Identity?.Name ?? string.Empty;
            var data = _db.SearchPending(userName, dateFrom, dateTo, patientName, sampleBarcode, investigation, criticalOnly);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetReview(int sampleDetailId)
        {
            var userName = User?.Identity?.Name ?? string.Empty;
            var data = _db.GetReview(userName, sampleDetailId);
            if (data == null)
            {
                return Json(new { success = false, message = "Report not available for authorization" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveReview(ReportAuthorizationActionRequest request)
        {
            if (request == null || request.SampleDetailId <= 0)
            {
                return Json(new Tuple<int, string>(0, "Invalid request"), JsonRequestBehavior.AllowGet);
            }
            var userName = User?.Identity?.Name ?? string.Empty;
            return Json(_db.SaveReview(userName, request), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Authorize(ReportAuthorizationActionRequest request)
        {
            if (request == null || request.SampleDetailId <= 0)
            {
                return Json(new Tuple<int, string>(0, "Invalid request"), JsonRequestBehavior.AllowGet);
            }
            var userName = User?.Identity?.Name ?? string.Empty;
            return Json(_db.Authorize(userName, request), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Reject(ReportAuthorizationActionRequest request)
        {
            if (request == null || request.SampleDetailId <= 0)
            {
                return Json(new Tuple<int, string>(0, "Invalid request"), JsonRequestBehavior.AllowGet);
            }
            var userName = User?.Identity?.Name ?? string.Empty;
            return Json(_db.Reject(userName, request), JsonRequestBehavior.AllowGet);
        }
    }
}
