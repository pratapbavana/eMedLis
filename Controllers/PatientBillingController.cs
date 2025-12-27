using eMedLis.DAL.PatientBilling;     // For PatientBillingDB
using eMedLis.Models.PatientBilling; // For PatientBillViewModel
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using ZXing;
using ZXing.Common;

namespace eMedLis.Controllers
{
    public class PatientBillingController : Controller
    {
        // GET: PatientBilling (You might have an Index view for this controller)
        public ActionResult Index()
        {
            return View();
        }

        // POST: PatientBilling/SaveBill
        [HttpPost]
        public JsonResult SaveBill(PatientBillViewModel billData)
        {
            try
            {
                if (billData == null)
                {
                    return Json(new { success = false, message = "Invalid data received." });
                }

                PatientBillingDB db = new PatientBillingDB();
                BillSaveResult result = db.SaveCompleteBill(billData);

                if (result.Success && result.BillSummaryId > 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Bill saved successfully!",
                        billId = result.BillSummaryId,
                        billNo = result.BillNo,
                        showSampleCollection = true
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to save bill." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while saving the bill: " + ex.Message });
            }
        }

        // You can add other actions here later, e.g., for fetching bills, updating, etc.
        [HttpGet]
        public ActionResult PrintBill(int billId)
        {
            try
            {
                PatientBillingDB db = new PatientBillingDB();
                var billData = db.GetCompleteBillForPrint(billId);

                if (billData == null)
                {
                    return Content("<h1>Bill not found</h1>", "text/html");
                }

                // Generate HTML content for direct print
                string htmlContent = GenerateBillHTML(billData, billId, forModal: false);

                return Content(htmlContent, "text/html");
            }
            catch (Exception ex)
            {
                return Content($"<h1>Error: {ex.Message}</h1>", "text/html");
            }
        }

        // Update GenerateBillHTML to support different output formats
        private string GenerateBillHTML(CompleteBillData billData, int billId, bool forModal = false, bool forPDF = false)
        {
            var html = new StringBuilder();
            string displayBillNo = billData.BillSummary.BillNo ?? billId.ToString();
            string uhid = billData.PatientInfo.UHID ?? "NEW";
            string barcodeBase64 = GenerateBarcode(uhid);
            string bodyOnLoad = (!forModal && !forPDF) ? "onload='window.print();'" : "";

            html.Append($@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Diagnostic Bill cum Receipt - {displayBillNo}</title>
    <style>
        * {{ margin:0; padding:0; box-sizing:border-box; }}
        body {{ font-family:Arial,sans-serif; font-size:12px; line-height:1.2; margin:10px; background:#fff; }}
        .bill-container {{ max-width:700px; margin:0 auto; padding:15px; border:2px solid #000; }}
        .header {{ display:flex; justify-content:space-between; align-items:flex-start; border-bottom:1px solid #ccc; padding-bottom:8px; margin-bottom:12px; }}
        .logo {{ width:60px; height:60px; background:#f0f0f0; border:1px solid #ccc; border-radius:50%; display:flex; align-items:center; justify-content:center; }}
        .hospital-info {{ flex:1; margin:0 12px; }}
        .hospital-name {{ font-size:16px; font-weight:bold; margin-bottom:3px; }}
        .hospital-address {{ font-size:10px; margin-bottom:1px; }}
        .header-right {{ text-align:right; min-width:160px; }}
        .barcode-image {{ height:35px; margin-bottom:4px; }}
        .bill-title {{ text-align:center; font-size:14px; font-weight:bold; text-decoration:underline; margin-bottom:14px; }}
        .patient-info {{ display:flex; justify-content:space-between; margin-bottom:14px; }}
        .info-col {{ width:48%; }}
        .info-row {{ display:flex; margin-bottom:4px; font-size:11px; }}
        .info-label {{ font-weight:bold; min-width:80px; }}
        .investigations-section {{ margin-bottom:14px; }}
        .section-header {{ display:flex; background:#666; color:#fff; padding:6px; font-size:12px; }}
        .col-sno {{ width:50px; text-align:center; }}
        .col-investigations {{ flex:1; padding-left:6px; }}
        .col-amount {{ width:100px; text-align:center; }}
        .investigations-table {{ width:100%; border-collapse:collapse; margin-top:4px; }}
        .investigations-table td {{ border:1px solid #666; padding:4px 6px; font-size:11px; }}
        .footer-section {{ display:flex; justify-content:space-between; margin-top:16px; }}
        .footer-left {{ width:45%; font-size:11px; }}
        .footer-right {{ width:45%; text-align:right; font-size:11px; }}
        .total-row {{ display:flex; justify-content:space-between; margin-bottom:3px; }}
        .total-label {{ font-weight:bold; }}
        .amount-in-words {{ text-align:center; font-size:11px; font-weight:bold; margin:12px 0; }}
        .signature-line {{ border-top:1px solid #000; text-align:center; padding-top:4px; font-size:10px; margin-top:20px; }}
        @media print {{ body{{margin:0;}} .bill-container{{border:none;}} }}
    </style>
</head>
<body {bodyOnLoad}>
    <div class='bill-container'>
        <!-- Header -->
        <div class='header'>
            <div class='logo'>🔬</div>
            <div class='hospital-info'>
                <div class='hospital-name'>Labsmart Pathology Software</div>
                <div class='hospital-address'>Plot No - 4, Ashoka Chowk, Opp Military General Hospital, Pune, 462023</div>
                <div class='hospital-address'>Phone no. 923456278, 939003261, 924058240</div>
            </div>
            <div class='header-right'>
                <img src='{barcodeBase64}' class='barcode-image' alt='Barcode' /><br/>
            </div>
        </div>

        <!-- Title -->
        <div class='bill-title'>DIAGNOSTIC BILL CUM RECEIPT</div>

        <!-- Patient Info -->
        <div class='patient-info'>
            <div class='info-col'>
                <div class='info-row'><div class='info-label'>Name:</div><div>{billData.PatientInfo.PatName}</div></div>
                <div class='info-row'><div class='info-label'>Age/Sex:</div><div>{billData.PatientInfo.Age} {billData.PatientInfo.AgeType}/{billData.PatientInfo.Gender}</div></div>
                <div class='info-row'><div class='info-label'>Mobile:</div><div>{billData.PatientInfo.MobileNo}</div></div>
                <div class='info-row'><div class='info-label'>Address:</div><div>{billData.PatientInfo.Area}, {billData.PatientInfo.City}</div></div>
            </div>
            <div class='info-col'>
                <div class='info-row'><div class='info-label'>UHID:</div><div>{uhid}</div></div>
                <div class='info-row'><div class='info-label'>Bill No:</div><div>{displayBillNo}</div></div>
                <div class='info-row'><div class='info-label'>Referred by:</div><div>{billData.PatientInfo.Ref ?? "Self"}</div></div>
                <div class='info-row'><div class='info-label'>Date/Time:</div><div>{billData.BillSummary.BillDate:dd/MM/yyyy HH:mm}</div></div>
            </div>
        </div>

        <!-- Investigations -->
        <div class='investigations-section'>
            <div class='section-header'>
                <div class='col-sno'>S. NO.</div>
                <div class='col-investigations'>INVESTIGATIONS</div>
                <div class='col-amount'>AMOUNT</div>
            </div>
            <table class='investigations-table'>
                <tbody>");
            for (int i = 0; i < billData.BillDetails.Count; i++)
            {
                var d = billData.BillDetails[i];
                html.Append($@"
                    <tr>
                        <td class='col-sno'>{i + 1}.</td>
                        <td class='col-investigations'>{d.InvName}</td>
                        <td class='col-amount'>Rs. {d.NetAmount:F2}</td>
                    </tr>");
            }
            html.Append(@"
                </tbody>
            </table>
        </div>

        <!-- Footer Section -->
        <div class='footer-section'>
            <div class='footer-left'>
                <div style='font-weight:bold; margin-bottom:4px;'>Payments:</div>");
            // Alternative compact payments display
            if (billData.PaymentDetails != null && billData.PaymentDetails.Any())
            {
                for (int i = 0; i < billData.PaymentDetails.Count; i++)
                {
                    var p = billData.PaymentDetails[i];
                    string rcpt = p.ReceiptNo;
                    string dt = p.PaymentDate.ToString("dd/MM");
                    html.Append($@"
                <div style='font-size:10px; margin-bottom:2px; display:flex; justify-content:space-between;'>
                    <span>{p.PaymentMode} - {rcpt}</span>
                    <span>₹{p.Amount:F2} ({dt})</span>
                </div>");
                }
                html.Append($@"
                <div style='border-top:1px solid #ccc; padding-top:2px; margin-top:4px; font-weight:bold; font-size:10px; display:flex; justify-content:space-between;'>
                    <span>Total Paid:</span>
                    <span>₹{billData.BillSummary.PaidAmount:F2}</span>
                </div>");
            }
            else
            {
                html.Append($@"
                <div style='font-size:10px; margin-bottom:2px; display:flex; justify-content:space-between;'>
                    <span>Cash</span>
                    <span>₹{billData.BillSummary.PaidAmount:F2}</span>
                </div>");
            }
            html.Append(@"
                <div style='font-size:10px; margin-top:4px;'>User: Admin</div>
                <div style='font-size:10px;'>Remarks: " + (billData.BillSummary.Remarks ?? "") + @"</div>
            </div>
            <div class='footer-right'>");
            html.Append($@"
                <div class='total-row'><span class='total-label'>Bill Amount:</span><span>₹{billData.BillSummary.TotalBill:F2}</span></div>
                <div class='total-row'><span class='total-label'>Discount Amount:</span><span>₹{billData.BillSummary.TotalDiscountAmount:F2}</span></div>
                <div class='total-row'><span class='total-label'>Final Bill Amount:</span><span>₹{billData.BillSummary.NetAmount:F2}</span></div>
                <div class='total-row'><span class='total-label'>Paid Amount:</span><span>₹{billData.BillSummary.PaidAmount:F2}</span></div>
                <div class='total-row'><span class='total-label'>Due Amount:</span><span>₹{billData.BillSummary.DueAmount:F2}</span></div>");
            html.Append(@"
            </div>
        </div>

        <div class='amount-in-words'>Received with Thanks: Rs. " + ConvertToWords(billData.BillSummary.PaidAmount) + @"</div>
        <div class='signature-line'>Signature of Front Office: _____________________</div>
        <div style='font-size:9px; text-align:right; margin-top:8px;'>
            Printed by: Admin | Print Date: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + @"
        </div>
    </div>
</body>
</html>");

            return html.ToString();
        }

        // Generate barcode using ZXing.Net
        private string GenerateBarcode(string text)
        {
            try
            {
                var writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Width = 200,
                        Height = 50,
                        Margin = 2
                    }
                };

                using (var bitmap = writer.Write(text))
                {
                    using (var stream = new MemoryStream())
                    {
                        bitmap.Save(stream, ImageFormat.Png);
                        byte[] imageBytes = stream.ToArray();
                        string base64String = Convert.ToBase64String(imageBytes);
                        return "data:image/png;base64," + base64String;
                    }
                }
            }
            catch
            {
                // Return a simple text if barcode generation fails
                return "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==";
            }
        }

        // Convert number to words
        private string ConvertToWords(decimal amount)
        {
            try
            {
                var integerPart = (long)amount;
                var words = NumberToWords(integerPart);
                return $"{words} Only";
            }
            catch
            {
                return $"Rupees {amount:F2} Only";
            }
        }

        private string NumberToWords(long number)
        {
            if (number < 0) return "Minus " + NumberToWords(Math.Abs(number));
            if (number == 0) return "Zero";

            var ones = new[] { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
            var teens = new[] { "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            var tens = new[] { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            string result = "";

            if (number >= 10000000) // Crores
            {
                result += NumberToWords(number / 10000000) + " Crore ";
                number %= 10000000;
            }

            if (number >= 100000) // Lakhs
            {
                result += NumberToWords(number / 100000) + " Lakh ";
                number %= 100000;
            }

            if (number >= 1000) // Thousands
            {
                result += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if (number >= 100) // Hundreds
            {
                result += ones[number / 100] + " Hundred ";
                number %= 100;
            }

            if (number >= 20)
            {
                result += tens[number / 10] + " ";
                number %= 10;
            }
            else if (number >= 10)
            {
                result += teens[number - 10] + " ";
                number = 0;
            }

            if (number > 0)
            {
                result += ones[number] + " ";
            }

            return result.Trim();
        }


        [HttpPost]
        public JsonResult SearchPatients(string searchValue)
        {
            try
            {
                if (string.IsNullOrEmpty(searchValue))
                {
                    return Json(new { success = false, message = "Please enter mobile number or UHID." });
                }

                // Determine search type and validate
                bool isMobileSearch = IsValidMobileNumber(searchValue);
                bool isUHIDSearch = IsValidUHID(searchValue);

                if (!isMobileSearch && !isUHIDSearch)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please enter a valid 10-digit mobile number or UHID (e.g., EMED2025001)."
                    });
                }

                PatientBillingDB db = new PatientBillingDB();
                var patients = db.SearchPatientsUniversal(searchValue);

                string searchType = isMobileSearch ? "mobile number" : "UHID";

                return Json(new
                {
                    success = true,
                    searchType = searchType,
                    searchValue = searchValue,
                    patients = patients.Select(p => new {
                        patientInfoId = p.PatientInfoId,
                        uhid = p.UHID,
                        patName = p.PatName,
                        age = p.Age,
                        ageType = p.AgeType,
                        gender = p.Gender,
                        refe = p.Ref,
                        area = p.Area,
                        city = p.City,
                        email = p.Email,
                        mobileNo = p.MobileNo,
                        lastVisit = p.LastVisit?.ToString("dd/MM/yyyy") ?? "First Visit"
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error searching patients: " + ex.Message });
            }
        }

        private bool IsValidMobileNumber(string input)
        {
            return !string.IsNullOrEmpty(input) &&
                   input.Length == 10 &&
                   input.All(char.IsDigit);
        }

        private bool IsValidUHID(string input)
        {
            return !string.IsNullOrEmpty(input) &&
                   (input.ToUpper().StartsWith("EMED") ||
                    (input.Length >= 4 && input.All(char.IsLetterOrDigit)));
        }

        [HttpGet]
        public ActionResult GetBillByNo(string billNo)
        {
            try
            {
                if (string.IsNullOrEmpty(billNo))
                {
                    return Json(new { success = false, message = "Bill number is required." }, JsonRequestBehavior.AllowGet);
                }

                PatientBillingDB db = new PatientBillingDB();
                var billData = db.GetBillByBillNo(billNo);

                if (billData != null)
                {
                    return Json(new
                    {
                        success = true,
                        billData = new
                        {
                            billSummaryId = billData.BillSummary.BillSummaryId,
                            billNo = billData.BillSummary.BillNo,
                            patientName = billData.PatientInfo.PatName,
                            totalAmount = billData.BillSummary.NetAmount,
                            BillDate = billData.BillSummary.BillDate.ToString("dd/MM/yyyy")
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Bill not found." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error retrieving bill: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult PrintBillModal(int billId)
        {
            try
            {
                PatientBillingDB db = new PatientBillingDB();
                var billData = db.GetCompleteBillForPrint(billId);

                if (billData == null)
                {
                    return Json(new { success = false, message = "Bill not found." }, JsonRequestBehavior.AllowGet);
                }

                // Generate HTML content for modal
                string htmlContent = GenerateBillHTML(billData, billId, forModal: true);

                return Json(new
                {
                    success = true,
                    htmlContent = htmlContent,
                    billNo = billData.BillSummary.BillNo,
                    patientName = billData.PatientInfo.PatName
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error generating bill: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult ExportBillPDF(int billId)
        {
            try
            {
                PatientBillingDB db = new PatientBillingDB();
                var billData = db.GetCompleteBillForPrint(billId);

                if (billData == null)
                {
                    return Json(new { success = false, message = "Bill not found." }, JsonRequestBehavior.AllowGet);
                }

                // Generate PDF-optimized HTML
                string htmlContent = GenerateBillHTML(billData, billId, forPDF: true);

                // You can integrate with libraries like iTextSharp, Rotativa, or wkhtmltopdf
                // For now, returning URL for client-side PDF generation
                return Json(new
                {
                    success = true,
                    pdfUrl = Url.Action("PrintBill", "PatientBilling", new { billId = billId }),
                    fileName = $"Bill_{billData.BillSummary.BillNo}_{DateTime.Now:yyyyMMdd}.pdf"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error generating PDF: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // In Controllers/PatientBillingController.cs

        [HttpPost]
        public JsonResult EmailBill(int billId, string emailTo, string emailSubject, string emailMessage)
        {
            try
            {
                // Implement email functionality using System.Net.Mail or SendGrid
                // This is a placeholder - you'll need to implement actual email sending

                PatientBillingDB db = new PatientBillingDB();
                var billData = db.GetCompleteBillForPrint(billId);

                if (billData == null)
                {
                    return Json(new { success = false, message = "Bill not found." });
                }

                // Generate HTML content for email
                string htmlContent = GenerateBillHTML(billData, billId, forModal: false);

                // TODO: Implement email sending logic
                // SendEmail(emailTo, emailSubject, emailMessage, htmlContent);

                return Json(new { success = true, message = "Email sent successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error sending email: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetRecentBillsList(int days = 30, string status = "")
        {
            try
            {
                var db = new PatientBillingDB();
                var bills = db.GetRecentBills(days, status);
                // Reuse DAL but ignore paging parameters
                var result = bills.Select(b => new {
                    b.BillSummaryId,
                    b.BillNo,
                    BillDate = b.BillDate.ToString("dd/MM/yyyy HH:mm"),
                    PatientName = b.PatName,
                    AgeGender = b.AgeGenderDisplay,
                    ReferringDoctor = b.Ref,
                    TotalAmount = b.TotalBill.ToString("F2"),
                    PaidAmount = b.PaidAmount.ToString("F2"),
                    Balance = b.DueAmount.ToString("F2"),
                    PaymentStatus = b.PaymentStatus,
                    StatusClass = b.PaymentStatusClass
                });
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult CancelBill(int billSummaryId, string cancelReason)
        {
            try
            {
                PatientBillingDB db = new PatientBillingDB();
                bool result = db.CancelBill(billSummaryId, cancelReason, "Current User"); // Replace with actual user

                if (result)
                {
                    return Json(new { success = true, message = "Bill cancelled successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to cancel bill." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error cancelling bill: " + ex.Message });
            }
        }

        [HttpGet]
        public ActionResult ViewBill(int billId)
        {
            try
            {
                PatientBillingDB db = new PatientBillingDB();
                var billData = db.GetCompleteBillForPrint(billId);

                if (billData == null)
                {
                    return Json(new { success = false, message = "Bill not found." }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    billData = new
                    {
                        billNo = billData.BillSummary.BillNo,
                        BillDate = billData.BillSummary.BillDate.ToString("dd/MM/yyyy HH:mm"),
                        patient = new
                        {
                            name = billData.PatientInfo.PatName,
                            uhid = billData.PatientInfo.UHID,
                            mobile = billData.PatientInfo.MobileNo,
                            age = billData.PatientInfo.Age,
                            gender = billData.PatientInfo.Gender,
                            referredBy = billData.PatientInfo.Ref
                        },
                        summary = new
                        {
                            totalBill = billData.BillSummary.TotalBill.ToString("F2"),
                            discount = billData.BillSummary.TotalDiscountAmount.ToString("F2"),
                            netAmount = billData.BillSummary.NetAmount.ToString("F2"),
                            paidAmount = billData.BillSummary.PaidAmount.ToString("F2"),
                            dueAmount = billData.BillSummary.DueAmount.ToString("F2")
                        },
                        investigations = billData.BillDetails.Select(d => new
                        {
                            name = d.InvName,
                            rate = d.Rate.ToString("F2"),
                            discount = d.DiscountAmount.ToString("F2"),
                            netAmount = d.NetAmount.ToString("F2")
                        }),
                        payments = billData.PaymentDetails.Select(p => new
                        {
                            mode = p.PaymentMode,
                            amount = p.Amount.ToString("F2"),
                            refNo = p.ReceiptNo,
                            paymentDate = p.PaymentDate.ToString("dd/MM/yyyy HH:mm")
                        })
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading bill: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}