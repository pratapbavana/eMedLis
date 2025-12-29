using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eMedLis.Models;

namespace eMedLis.Controllers
{
    [Authorize]
    public class SubDepartmentController : Controller
    {
        SubDepartmentDB depDB = new SubDepartmentDB();
        // GET: SubDepartment
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult List()
        {
            return Json(depDB.Get_SubDepartment(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult ListByDeptId(int Id)
        {
            return Json(depDB.Get_SubDepartmentByDeptId(Id), JsonRequestBehavior.AllowGet);
        }
        public JsonResult Add(SubDepartment dp)
        {
            return Json(depDB.Add_SubDepartment(dp), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetbyID(int Id)
        {
            return Json(depDB.Get_SubDepartmentById(Id), JsonRequestBehavior.AllowGet);
        }
        public JsonResult Update(SubDepartment dep)
        {
            return Json(depDB.Update_SubDepartment(dep), JsonRequestBehavior.AllowGet);
        }
       
    }
}