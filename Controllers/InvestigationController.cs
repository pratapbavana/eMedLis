using eMedLis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eMedLis.Controllers
{
    public class InvestigationController : Controller
    {
        InvMasterDB invDB = new InvMasterDB();
        // GET: Investigation
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult List()
        {
            return Json(invDB.Get_Investigation(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult Add(InvMaster dp)
        {
            return Json(invDB.Add_Investigation(dp), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetbyID(int Id)
        {
            return Json(invDB.Get_InvestigationById(Id), JsonRequestBehavior.AllowGet);
        }
        public JsonResult Update(InvMaster inv)
        {
            return Json(invDB.Update_Investigation(inv), JsonRequestBehavior.AllowGet);
        }
        public JsonResult Delete(int Id)
        {
            return Json(invDB.Delete_Investigation(Id), JsonRequestBehavior.AllowGet);
        }
    }
}