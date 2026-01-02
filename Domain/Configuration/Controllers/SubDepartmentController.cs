using eMedLis.Domain.Configuration.Entities;
using eMedLis.Infrastructure.Data.SubDepartment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eMedLis.Domain.Configuration.Controllers
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
        public JsonResult Add(SubDepartments dp)
        {
            return Json(depDB.Add_SubDepartment(dp), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetbyID(int Id)
        {
            return Json(depDB.Get_SubDepartmentById(Id), JsonRequestBehavior.AllowGet);
        }
        public JsonResult Update(SubDepartments dep)
        {
            return Json(depDB.Update_SubDepartment(dep), JsonRequestBehavior.AllowGet);
        }
       
    }
}