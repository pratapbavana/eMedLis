using eMedLis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eMedLis.Controllers
{
    [Authorize]
    public class ReferenceRangeController : Controller
    {
        private readonly ReferenceRangeDB rangeDB = new ReferenceRangeDB();

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult List(int ParameterId)
        {
            return Json(rangeDB.Get_ReferenceRangesByParameter(ParameterId), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Add(ReferenceRange range)
        {
            NormalizeAgeDays(range);
            return Json(rangeDB.Add_ReferenceRange(range), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetbyID(int Id)
        {
            return Json(rangeDB.Get_ReferenceRangeById(Id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Update(ReferenceRange range)
        {
            NormalizeAgeDays(range);
            return Json(rangeDB.Update_ReferenceRange(range), JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetActive(int Id, bool Active)
        {
            return Json(rangeDB.Set_ReferenceRangeActive(Id, Active), JsonRequestBehavior.AllowGet);
        }

        public JsonResult ComboList()
        {
            return Json(rangeDB.Get_ReferenceRangeCombos(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult ListByParameterMethod(int ParameterId, int MethodId)
        {
            return Json(rangeDB.Get_ReferenceRangesByParameterMethod(ParameterId, MethodId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveBatch(ReferenceRangeBatch batch)
        {
            if (batch == null || batch.ParameterId <= 0)
            {
                return Json(new Tuple<int, string>(0, "Parameter is required"), JsonRequestBehavior.AllowGet);
            }
            if (batch.MethodId <= 0)
            {
                return Json(new Tuple<int, string>(0, "Method is required"), JsonRequestBehavior.AllowGet);
            }
            if (batch.Ranges == null || batch.Ranges.Count == 0)
            {
                return Json(new Tuple<int, string>(0, "At least one range is required"), JsonRequestBehavior.AllowGet);
            }

            foreach (var range in batch.Ranges)
            {
                NormalizeAgeDays(range);
                range.ParameterId = batch.ParameterId;
                range.MethodId = batch.MethodId;
            }

            string validationMsg;
            if (!ValidateBatch(batch.Ranges, out validationMsg))
            {
                return Json(new Tuple<int, string>(0, validationMsg), JsonRequestBehavior.AllowGet);
            }

            var resultReplace = rangeDB.Replace_ReferenceRangeBatch(batch.ParameterId, batch.MethodId, batch.Ranges);
            return Json(resultReplace, JsonRequestBehavior.AllowGet);
        }

        private bool ValidateBatch(List<ReferenceRange> ranges, out string message)
        {
            message = "";
            if (ranges == null || ranges.Count == 0)
            {
                message = "At least one range is required";
                return false;
            }
            foreach (var r in ranges)
            {
                if (r.AgeFromDays < 0 || r.AgeToDays < 0 || r.AgeFromDays > r.AgeToDays)
                {
                    message = "Invalid age range. Age From must be less than or equal to Age To.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(r.Gender))
                {
                    message = "Gender is required for all ranges.";
                    return false;
                }

                var hasNormalMin = r.NormalMin.HasValue;
                var hasNormalMax = r.NormalMax.HasValue;
                var hasRangeText = !string.IsNullOrWhiteSpace(r.RangeText);

                if (!hasRangeText && !hasNormalMin && !hasNormalMax)
                {
                    message = "Provide either numeric normal range or descriptive range text.";
                    return false;
                }

                if (hasNormalMin != hasNormalMax)
                {
                    message = "Both Normal Min and Normal Max are required when numeric range is used.";
                    return false;
                }

                if (hasNormalMin && hasNormalMax && r.NormalMin.Value >= r.NormalMax.Value)
                {
                    message = "Normal Min must be less than Normal Max.";
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(r.RangeText) && r.RangeText.Length > 500)
                {
                    message = "Reference Range Text max length is 500 characters.";
                    return false;
                }
            }

            for (int i = 0; i < ranges.Count; i++)
            {
                for (int j = i + 1; j < ranges.Count; j++)
                {
                    if (!GendersOverlap(ranges[i].Gender, ranges[j].Gender))
                    {
                        continue;
                    }
                    if (ranges[i].AgeFromDays <= ranges[j].AgeToDays && ranges[j].AgeFromDays <= ranges[i].AgeToDays)
                    {
                        message = "Overlapping age ranges detected for gender " + ranges[i].Gender + " / " + ranges[j].Gender + ".";
                        return false;
                    }
                }
            }
            return true;
        }

        private bool GendersOverlap(string a, string b)
        {
            if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (string.Equals(a, "Both", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (string.Equals(b, "Both", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        private void NormalizeAgeDays(ReferenceRange range)
        {
            if (range == null)
            {
                return;
            }

            range.AgeFromDays = ToStorageDays(range.AgeFromValue, range.AgeFromUnit, true);
            range.AgeToDays = ToStorageDays(range.AgeToValue, range.AgeToUnit, false);
        }

        private int ToStorageDays(decimal value, string unit, bool isFromBoundary)
        {
            var normalized = (unit ?? string.Empty).Trim().ToLowerInvariant();

            if (normalized == "days" || normalized == "day")
            {
                return Convert.ToInt32(Math.Round(value, MidpointRounding.AwayFromZero));
            }

            if (normalized == "months" || normalized == "month")
            {
                var days = Convert.ToInt32(Math.Round(value * 30m, MidpointRounding.AwayFromZero));
                return isFromBoundary ? days + 1 : days;
            }

            var yearDays = Convert.ToInt32(Math.Round(value * 365m, MidpointRounding.AwayFromZero));
            return isFromBoundary ? yearDays + 1 : yearDays;
        }
    }
}
