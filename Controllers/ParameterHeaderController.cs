using eMedLis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eMedLis.Controllers
{
    [Authorize]
    public class ParameterHeaderController : Controller
    {
        private readonly ParameterHeaderDB headerDB = new ParameterHeaderDB();

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult List()
        {
            return Json(headerDB.Get_ParameterHeader(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Add(ParameterHeader header)
        {
            return Json(headerDB.Add_ParameterHeader(header), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetbyID(int Id)
        {
            return Json(headerDB.Get_ParameterHeaderById(Id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Update(ParameterHeader header)
        {
            return Json(headerDB.Update_ParameterHeader(header), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Delete(int Id)
        {
            return Json(headerDB.Delete_ParameterHeader(Id), JsonRequestBehavior.AllowGet);
        }
    }
}
