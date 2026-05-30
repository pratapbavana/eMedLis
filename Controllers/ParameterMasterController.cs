using eMedLis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace eMedLis.Controllers
{
    [Authorize]
    public class ParameterMasterController : Controller
    {
        private readonly ParameterMasterDB paramDB = new ParameterMasterDB();

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult List()
        {
            return Json(paramDB.Get_Parameter(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Add(ParameterMaster param)
        {
            return Json(paramDB.Add_Parameter(param), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetbyID(int Id)
        {
            return Json(paramDB.Get_ParameterById(Id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Update(ParameterMaster param)
        {
            return Json(paramDB.Update_Parameter(param), JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetActive(int Id, bool Active)
        {
            return Json(paramDB.Set_ParameterActive(Id, Active), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult DropdownValues(int parameterId)
        {
            return Json(paramDB.Get_DropdownValues(parameterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveDropdownValues(ParameterDropdownSaveRequest request)
        {
            if (request == null)
            {
                return Json(new Tuple<int, string>(0, "Invalid request"), JsonRequestBehavior.AllowGet);
            }
            int parameterId = request.ParameterId;
            if (parameterId <= 0)
            {
                return Json(new Tuple<int, string>(0, "Parameter is required"), JsonRequestBehavior.AllowGet);
            }
            return Json(paramDB.Save_DropdownValues(parameterId, request.Values ?? new List<ParameterDropdownValue>()), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ValidateFormula(string formula)
        {
            if (string.IsNullOrWhiteSpace(formula))
            {
                return Json(new { valid = false, message = "Formula is required" });
            }

            string normalized = formula.Trim();
            if (!Regex.IsMatch(normalized, @"^[0-9\{\}\+\-\*\/\(\)\.\s]+$"))
            {
                return Json(new { valid = false, message = "Invalid character in formula" });
            }

            if (!HasBalancedParentheses(normalized))
            {
                return Json(new { valid = false, message = "Missing parentheses" });
            }

            var tokenMatches = Regex.Matches(normalized, @"\{(\d+)\}");
            var ids = tokenMatches.Cast<Match>().Select(m => Convert.ToInt32(m.Groups[1].Value)).Distinct().ToList();
            var validIds = new HashSet<int>(paramDB.Get_Parameter().Select(p => p.Id));
            if (ids.Any(id => !validIds.Contains(id)))
            {
                return Json(new { valid = false, message = "Invalid parameter reference" });
            }

            if (Regex.IsMatch(normalized, @"\/\s*0+(\.0+)?(\D|$)"))
            {
                return Json(new { valid = false, message = "Division by zero detected" });
            }

            return Json(new { valid = true, message = "Formula Valid" });
        }

        private bool HasBalancedParentheses(string expression)
        {
            int count = 0;
            foreach (char ch in expression)
            {
                if (ch == '(') count++;
                if (ch == ')') count--;
                if (count < 0) return false;
            }
            return count == 0;
        }
    }
}
