using eMedLis.Models;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;

namespace eMedLis.Controllers
{
    [Authorize]
    public class LabMasterController : Controller
    {
        private readonly LabMasterDB _db = new LabMasterDB();

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult Get()
        {
            return Json(_db.Get_Current(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Save(LabMaster lab)
        {
            if (lab == null || string.IsNullOrWhiteSpace(lab.LabName))
            {
                return Json(new Tuple<int, string>(0, "Lab Name is required"), JsonRequestBehavior.AllowGet);
            }

            byte[] logoBytes = null;
            string logoMime = null;
            var logoCheck = BuildImage(lab.LogoBase64, 200, 80, 204800, out logoBytes, out logoMime, true);
            if (logoCheck.Item1 == 0) return Json(logoCheck, JsonRequestBehavior.AllowGet);

            byte[] headerBytes = null;
            string headerMime = null;
            var headerCheck = BuildImage(lab.ReportHeaderImageBase64, 1200, 180, 409600, out headerBytes, out headerMime, false);
            if (headerCheck.Item1 == 0) return Json(headerCheck, JsonRequestBehavior.AllowGet);

            byte[] footerBytes = null;
            string footerMime = null;
            var footerCheck = BuildImage(lab.ReportFooterImageBase64, 1200, 120, 409600, out footerBytes, out footerMime, false);
            if (footerCheck.Item1 == 0) return Json(footerCheck, JsonRequestBehavior.AllowGet);

            var userName = User != null && User.Identity != null ? User.Identity.Name : string.Empty;
            return Json(_db.Save(lab, logoBytes, logoMime, headerBytes, headerMime, footerBytes, footerMime, userName), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Image(string type = "Logo")
        {
            var normalizedType = (type ?? "Logo").Trim();
            if (!string.Equals(normalizedType, "Logo", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(normalizedType, "Header", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(normalizedType, "Footer", StringComparison.OrdinalIgnoreCase))
            {
                normalizedType = "Logo";
            }

            var data = _db.Get_Image(normalizedType);
            if (data == null || data.Item1 == null || data.Item1.Length == 0)
            {
                return HttpNotFound();
            }

            return File(data.Item1, string.IsNullOrWhiteSpace(data.Item2) ? "image/png" : data.Item2);
        }

        private Tuple<int, string> BuildImage(string dataUrl, int width, int height, int maxBytes, out byte[] imageBytes, out string mimeType, bool optional)
        {
            imageBytes = null;
            mimeType = null;

            if (string.IsNullOrWhiteSpace(dataUrl))
            {
                return new Tuple<int, string>(1, optional ? "No image" : "No image");
            }

            var parts = dataUrl.Split(',');
            if (parts.Length != 2 || !parts[0].StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                return new Tuple<int, string>(0, "Invalid image format");
            }

            var header = parts[0].ToLowerInvariant();
            if (!header.Contains("png") && !header.Contains("jpeg") && !header.Contains("jpg"))
            {
                return new Tuple<int, string>(0, "Only PNG/JPG files are allowed");
            }

            byte[] source;
            try
            {
                source = Convert.FromBase64String(parts[1]);
            }
            catch
            {
                return new Tuple<int, string>(0, "Invalid image data");
            }

            try
            {
                using (var inputStream = new MemoryStream(source))
                using (var sourceImage = System.Drawing.Image.FromStream(inputStream))
                using (var targetBitmap = new Bitmap(width, height))
                {
                    using (var graphics = Graphics.FromImage(targetBitmap))
                    {
                        graphics.Clear(Color.White);
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                        var ratio = Math.Min((float)width / sourceImage.Width, (float)height / sourceImage.Height);
                        var drawWidth = (int)Math.Round(sourceImage.Width * ratio);
                        var drawHeight = (int)Math.Round(sourceImage.Height * ratio);
                        var x = (width - drawWidth) / 2;
                        var y = (height - drawHeight) / 2;

                        graphics.DrawImage(sourceImage, x, y, drawWidth, drawHeight);
                    }

                    using (var outputStream = new MemoryStream())
                    {
                        targetBitmap.Save(outputStream, ImageFormat.Png);
                        imageBytes = outputStream.ToArray();
                    }
                }

                if (imageBytes == null || imageBytes.Length == 0)
                {
                    return new Tuple<int, string>(0, "Failed to process image");
                }

                if (imageBytes.Length > maxBytes)
                {
                    return new Tuple<int, string>(0, "Image size exceeds allowed limit after resize");
                }

                mimeType = "image/png";
                return new Tuple<int, string>(1, "Image ok");
            }
            catch
            {
                return new Tuple<int, string>(0, "Unsupported or corrupted image file");
            }
        }
    }
}
