using eMedLis.DAL.ReportSettings;
using eMedLis.Models.ReportSettings;
using System;
using System.Web.Mvc;

namespace eMedLis.Controllers
{
    [Authorize]
    public class ReportSettingsController : Controller
    {
        private readonly ReportSettingsDB _db = new ReportSettingsDB();

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult Get()
        {
            return Json(_db.GetCurrent(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Save(ReportLayoutSettings model)
        {
            if (model == null)
            {
                return Json(new Tuple<int, string>(0, "Invalid request"), JsonRequestBehavior.AllowGet);
            }

            model.PrintMode = string.Equals(model.PrintMode, "PrePrinted", StringComparison.OrdinalIgnoreCase) ? "PrePrinted" : "PlainPaper";
            model.HeaderHeightPx = Clamp(model.HeaderHeightPx, 0, 400);
            model.FooterHeightPx = Clamp(model.FooterHeightPx, 0, 300);
            model.TopMarginPx = Clamp(model.TopMarginPx, 0, 200);
            model.LeftMarginPx = Clamp(model.LeftMarginPx, 0, 200);
            model.RightMarginPx = Clamp(model.RightMarginPx, 0, 200);
            model.BottomMarginPx = Clamp(model.BottomMarginPx, 0, 200);
            model.ContentStartPx = Clamp(model.ContentStartPx, 0, 500);

            if (string.IsNullOrWhiteSpace(model.LabName))
            {
                model.LabName = "SSK Diagnostics";
            }

            var userName = User != null && User.Identity != null ? User.Identity.Name : string.Empty;
            return Json(_db.Save(model, userName), JsonRequestBehavior.AllowGet);
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
