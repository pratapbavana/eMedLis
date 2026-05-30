using eMedLis.Models;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace eMedLis.Controllers
{
    [Authorize]
    public class DoctorMasterController : Controller
    {
        private readonly DoctorMasterDB _doctorDB = new DoctorMasterDB();

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult List()
        {
            return Json(_doctorDB.Get_DoctorList(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Users()
        {
            return Json(_doctorDB.Get_ActiveUsers(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetbyID(int Id)
        {
            return Json(_doctorDB.Get_DoctorById(Id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Add(DoctorMaster doctor)
        {
            if (doctor == null || doctor.UserId <= 0)
            {
                return Json(new Tuple<int, string>(0, "User is required"), JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(doctor.SubDepartmentIds))
            {
                return Json(new Tuple<int, string>(0, "Select at least one sub department"), JsonRequestBehavior.AllowGet);
            }

            byte[] signatureBytes = null;
            string mimeType = null;
            var signatureValidation = BuildSignature(doctor.SignatureBase64, out signatureBytes, out mimeType);
            if (signatureValidation.Item1 == 0)
            {
                return Json(signatureValidation, JsonRequestBehavior.AllowGet);
            }

            return Json(_doctorDB.Add_Doctor(doctor, signatureBytes, mimeType), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Update(DoctorMaster doctor)
        {
            if (doctor == null || doctor.Id <= 0 || doctor.UserId <= 0)
            {
                return Json(new Tuple<int, string>(0, "Invalid doctor details"), JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(doctor.SubDepartmentIds))
            {
                return Json(new Tuple<int, string>(0, "Select at least one sub department"), JsonRequestBehavior.AllowGet);
            }

            byte[] signatureBytes = null;
            string mimeType = null;
            var signatureValidation = BuildSignature(doctor.SignatureBase64, out signatureBytes, out mimeType);
            if (signatureValidation.Item1 == 0)
            {
                return Json(signatureValidation, JsonRequestBehavior.AllowGet);
            }

            return Json(_doctorDB.Update_Doctor(doctor, signatureBytes, mimeType), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SetActive(int Id, bool Active)
        {
            return Json(_doctorDB.Set_DoctorActive(Id, Active), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Signature(int id)
        {
            if (id <= 0)
            {
                return HttpNotFound();
            }

            var data = _doctorDB.Get_Signature(id);
            if (data == null || data.Item1 == null || data.Item1.Length == 0)
            {
                return HttpNotFound();
            }

            return File(data.Item1, string.IsNullOrWhiteSpace(data.Item2) ? "image/png" : data.Item2);
        }

        private Tuple<int, string> BuildSignature(string signatureBase64, out byte[] signatureBytes, out string mimeType)
        {
            signatureBytes = null;
            mimeType = null;

            if (string.IsNullOrWhiteSpace(signatureBase64))
            {
                return new Tuple<int, string>(1, "No signature");
            }

            var parts = signatureBase64.Split(',');
            if (parts.Length != 2 || !parts[0].StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                return new Tuple<int, string>(0, "Invalid signature format");
            }

            var header = parts[0].ToLowerInvariant();
            if (!header.Contains("png") && !header.Contains("jpeg") && !header.Contains("jpg"))
            {
                return new Tuple<int, string>(0, "Only PNG/JPG signature is allowed");
            }

            byte[] source;
            try
            {
                source = Convert.FromBase64String(parts[1]);
            }
            catch
            {
                return new Tuple<int, string>(0, "Invalid signature file");
            }

            try
            {
                using (var inputStream = new MemoryStream(source))
                using (var sourceImage = Image.FromStream(inputStream))
                using (var targetBitmap = new Bitmap(200, 80))
                {
                    using (var graphics = Graphics.FromImage(targetBitmap))
                    {
                        graphics.Clear(Color.White);
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                        var ratio = Math.Min(200f / sourceImage.Width, 80f / sourceImage.Height);
                        var drawWidth = (int)Math.Round(sourceImage.Width * ratio);
                        var drawHeight = (int)Math.Round(sourceImage.Height * ratio);
                        var x = (200 - drawWidth) / 2;
                        var y = (80 - drawHeight) / 2;

                        graphics.DrawImage(sourceImage, x, y, drawWidth, drawHeight);
                    }

                    using (var outputStream = new MemoryStream())
                    {
                        targetBitmap.Save(outputStream, ImageFormat.Png);
                        signatureBytes = outputStream.ToArray();
                    }
                }

                if (signatureBytes == null || signatureBytes.Length == 0)
                {
                    return new Tuple<int, string>(0, "Failed to process signature");
                }

                if (signatureBytes.Length > 102400)
                {
                    return new Tuple<int, string>(0, "Signature file size exceeds 100 KB after resize");
                }

                mimeType = "image/png";
                return new Tuple<int, string>(1, "Signature ok");
            }
            catch
            {
                return new Tuple<int, string>(0, "Unsupported or corrupted signature image");
            }
        }
    }
}
