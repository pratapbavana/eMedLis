using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eMedLis.Models;

namespace eMedLis.Controllers
{
    public class LookupController : Controller
    {
        readonly LookupDB LookupDB = new LookupDB();
        // GET: Specimen
        public ActionResult Specimen()
        {
            return View();
        }
        public ActionResult Vacutainer()
        {
            return View();
        }
        public JsonResult List(string ItemType)
        {
            return Json(LookupDB.Get_Record(ItemType), JsonRequestBehavior.AllowGet);
        }
        public JsonResult Add(Lookup dp)
        {
            return Json(LookupDB.Add_Record(dp), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetbyID(int Id)
        {
            return Json(LookupDB.Get_RecordsById(Id), JsonRequestBehavior.AllowGet);
        }
        public JsonResult Update(Lookup rec)
        {
            return Json(LookupDB.Update_Record(rec), JsonRequestBehavior.AllowGet);
        }
        public JsonResult Delete(int Id)
        {
            return Json(LookupDB.Delete_Record(Id), JsonRequestBehavior.AllowGet);
        }
    }
}