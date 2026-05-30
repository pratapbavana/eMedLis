using eMedLis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eMedLis.Controllers
{
    [Authorize]
    public class TestMethodController : Controller
    {
        private readonly TestMethodDB methodDB = new TestMethodDB();

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult List()
        {
            return Json(methodDB.Get_TestMethod(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Add(TestMethod method)
        {
            return Json(methodDB.Add_TestMethod(method), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetbyID(int Id)
        {
            return Json(methodDB.Get_TestMethodById(Id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Update(TestMethod method)
        {
            return Json(methodDB.Update_TestMethod(method), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Delete(int Id)
        {
            return Json(methodDB.Delete_TestMethod(Id), JsonRequestBehavior.AllowGet);
        }
    }
}
