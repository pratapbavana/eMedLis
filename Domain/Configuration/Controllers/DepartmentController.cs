using eMedLis.Domain.Configuration.Entities;
using eMedLis.Infrastructure.Data.Department;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eMedLis.Domain.Configuration.Controllers
{
    [Authorize]
    public class DepartmentController : Controller
    {
        DepartmentDB depDB = new DepartmentDB();
        // GET: Department
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult List()
        {
            return Json(depDB.Get_Department(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult Add(Departments dp)
        {
            return Json(depDB.Add_Department(dp), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetbyID(int Id)
        {
            return Json(depDB.Get_DepartmentById(Id), JsonRequestBehavior.AllowGet);
        }
        public JsonResult Update(Departments dep)
        {
            return Json(depDB.Update_Department(dep), JsonRequestBehavior.AllowGet);
        }
        public JsonResult Delete(int Id)
        {
            return Json(depDB.Delete_Department(Id), JsonRequestBehavior.AllowGet);
        }
    }
}