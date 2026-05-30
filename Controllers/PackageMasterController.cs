using eMedLis.Models;
using System;
using System.Web.Mvc;

namespace eMedLis.Controllers
{
    [Authorize]
    public class PackageMasterController : Controller
    {
        private readonly PackageMasterDB _db = new PackageMasterDB();

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult List()
        {
            return Json(_db.Get_Packages(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ActiveList()
        {
            return Json(_db.Get_ActivePackages(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetbyID(int Id)
        {
            return Json(_db.Get_PackageById(Id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetPackageInvestigations(int Id)
        {
            return Json(_db.Get_PackageInvestigations(Id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Add(PackageMaster model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.PackageName))
            {
                return Json(new Tuple<int, string>(0, "Package Name is required"), JsonRequestBehavior.AllowGet);
            }
            return Json(_db.Add_Package(model), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Update(PackageMaster model)
        {
            if (model == null || model.Id <= 0 || string.IsNullOrWhiteSpace(model.PackageName))
            {
                return Json(new Tuple<int, string>(0, "Invalid package details"), JsonRequestBehavior.AllowGet);
            }
            return Json(_db.Update_Package(model), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SetActive(int Id, bool Active)
        {
            return Json(_db.Set_PackageActive(Id, Active), JsonRequestBehavior.AllowGet);
        }
    }
}
