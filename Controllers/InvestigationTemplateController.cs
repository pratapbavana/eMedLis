using eMedLis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eMedLis.Controllers
{
    [Authorize]
    public class InvestigationTemplateController : Controller
    {
        private readonly InvestigationTemplateDB templateDB = new InvestigationTemplateDB();

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult List(string InvestigationId)
        {
            return Json(templateDB.Get_TemplateByInvestigation(InvestigationId), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetInterpretation(string InvestigationId)
        {
            var html = templateDB.Get_InterpretationByInvestigation(InvestigationId);
            return Json(new { InterpretationHtml = html ?? string.Empty }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Save(InvestigationTemplateBatch batch)
        {
            if (batch == null || string.IsNullOrWhiteSpace(batch.InvestigationId))
            {
                return Json(new Tuple<int, string>(0, "Investigation is required"), JsonRequestBehavior.AllowGet);
            }
            if (batch.Items == null || batch.Items.Count == 0)
            {
                return Json(new Tuple<int, string>(0, "At least one template item is required"), JsonRequestBehavior.AllowGet);
            }

            templateDB.Delete_TemplateByInvestigation(batch.InvestigationId);
            foreach (var item in batch.Items)
            {
                item.InvestigationId = batch.InvestigationId;
                var result = templateDB.Add_TemplateItem(item);
                if (result.Item1 != 1)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }

            templateDB.Save_Interpretation(batch.InvestigationId, batch.InterpretationHtml);

            return Json(new Tuple<int, string>(1, "Template saved successfully"), JsonRequestBehavior.AllowGet);
        }
    }
}
