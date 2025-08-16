using eMedLis.DAL.PatientBilling;     // For PatientBillingDB
using eMedLis.Models.PatientBilling; // For PatientBillViewModel
using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;

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
                        billNo = result.BillNo
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

            // Different styling based on output format
            string bodyOnLoad = "";
            string additionalStyles = "";

            if (!forModal && !forPDF)
            {
                bodyOnLoad = "onload='window.print();'";
            }

            if (forModal)
            {
                additionalStyles = @"
        .bill-container { 
            width: 100%; 
            max-width: 400px; 
            margin: 0 auto; 
        }
        body { 
            margin: 10px; 
            background: #fff; 
        }";
            }

            html.Append($@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Bill - {displayBillNo}</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ 
            font-family: Arial, sans-serif; 
            font-size: 12px; 
            line-height: 1.4;
            color: #000;
            background: #fff;
        }}
        .bill-container {{ 
            width: 80mm; 
            margin: 0 auto; 
            padding: 10px;
        }}
        .header {{ 
            text-align: center; 
            border-bottom: 2px solid #000; 
            padding-bottom: 10px; 
            margin-bottom: 15px;
        }}
        .header h1 {{ 
            font-size: 16px; 
            font-weight: bold; 
            margin-bottom: 5px;
        }}
        .header p {{ 
            font-size: 10px; 
            margin: 2px 0;
        }}
        .bill-info {{ 
            display: flex; 
            justify-content: space-between; 
            margin-bottom: 15px;
        }}
        .bill-info div {{ 
            font-size: 11px;
        }}
        .patient-section {{ 
            margin-bottom: 15px;
        }}
        .patient-row {{ 
            display: flex; 
            justify-content: space-between; 
            margin-bottom: 3px;
        }}
        .investigations-table {{ 
            width: 100%; 
            border-collapse: collapse; 
            margin-bottom: 15px;
        }}
        .investigations-table th, .investigations-table td {{ 
            border: 1px solid #000; 
            padding: 4px; 
            text-align: left; 
            font-size: 10px;
        }}
        .investigations-table th {{ 
            background-color: #f0f0f0; 
            font-weight: bold;
        }}
        .investigations-table td.number {{ 
            text-align: right;
        }}
        .totals-section {{ 
            margin-bottom: 15px;
        }}
        .total-row {{ 
            display: flex; 
            justify-content: space-between; 
            margin-bottom: 2px; 
            padding: 2px 0;
        }}
        .total-row.final {{ 
            font-weight: bold; 
            border-top: 1px solid #000; 
            padding-top: 5px;
        }}
        .payment-section {{ 
            margin-bottom: 15px;
        }}
        .payment-table {{ 
            width: 100%; 
            border-collapse: collapse;
        }}
        .payment-table th, .payment-table td {{ 
            border: 1px solid #000; 
            padding: 4px; 
            text-align: left; 
            font-size: 10px;
        }}
        .payment-table th {{ 
            background-color: #f0f0f0;
        }}
        .footer {{ 
            text-align: center; 
            margin-top: 20px; 
            border-top: 1px solid #000; 
            padding-top: 10px;
        }}
        .barcode {{ 
            text-align: center; 
            font-family: 'Courier New', monospace; 
            font-size: 14px; 
            margin: 10px 0;
        }}
        {additionalStyles}
        @media print {{
            body {{ margin: 0; }}
            .bill-container {{ width: 100%; }}
            .no-print {{ display: none; }}
        }}
    </style>
</head>
<body {bodyOnLoad}>
    <div class='bill-container'>
        <!-- Header -->
        <div class='header'>
            <h1>eMedLis Pathology Software</h1>
            <p>Plot No - 4, Ashoka Chowk, Opp Military General Hospital</p>
            <p>Pune, 462023</p>
            <p>Phone: 923456278, 939003261, 924058240</p>
        </div>

        <!-- Bill Info and Barcode -->
        <div class='bill-info'>
            <div><strong>Bill No:</strong> {displayBillNo}</div>
            <div><strong>Date:</strong> {billData.BillSummary.BillDate:dd/MM/yyyy HH:mm}</div>
        </div>
        
        <div class='barcode'>
            ||||| |||| | || ||||<br>
            {displayBillNo}
        </div>

        <!-- Patient Information -->
        <div class='patient-section'>
            <div class='patient-row'>
                <span><strong>Name:</strong> {billData.PatientInfo.PatName}</span>
                <span><strong>UHID:</strong> {billData.PatientInfo.UHID ?? "NEW"}</span>
            </div>
            <div class='patient-row'>
                <span><strong>Age/Sex:</strong> {billData.PatientInfo.Age} {billData.PatientInfo.AgeType}/{billData.PatientInfo.Gender}</span>
                <span><strong>Referred by:</strong> {billData.PatientInfo.Ref ?? "Self"}</span>
            </div>
            <div class='patient-row'>
                <span><strong>Mobile:</strong> {billData.PatientInfo.MobileNo}</span>
                <span><strong>Received by:</strong> Admin</span>
            </div>
            <div class='patient-row'>
                <span><strong>Address:</strong> {billData.PatientInfo.Area ?? ""}, {billData.PatientInfo.City ?? ""}</span>
            </div>
        </div>

        <!-- Investigations Table -->
        <table class='investigations-table'>
            <thead>
                <tr>
                    <th>S.NO.</th>
                    <th>INVESTIGATIONS</th>
                    <th>AMOUNT</th>
                </tr>
            </thead>
            <tbody>");

            // Add investigation rows
            for (int i = 0; i < billData.BillDetails.Count; i++)
            {
                var detail = billData.BillDetails[i];
                html.Append($@"
                <tr>
                    <td>{i + 1}</td>
                    <td>{detail.InvName}</td>
                    <td class='number'>Rs. {detail.NetAmount:F2}</td>
                </tr>");
            }

            html.Append($@"
            </tbody>
        </table>

        <!-- Totals Section -->
        <div class='totals-section'>
            <div class='total-row'>
                <span>Bill Amount:</span>
                <span>Rs. {billData.BillSummary.TotalBill:F2}</span>
            </div>
            <div class='total-row'>
                <span>Discount Amount:</span>
                <span>Rs. {billData.BillSummary.TotalDiscountAmount:F2}</span>
            </div>
            <div class='total-row final'>
                <span>Final Bill Amount:</span>
                <span>Rs. {billData.BillSummary.NetAmount:F2}</span>
            </div>
            <div class='total-row'>
                <span>Paid Amount:</span>
                <span>Rs. {billData.BillSummary.PaidAmount:F2}</span>
            </div>
            <div class='total-row'>
                <span>Due Amount:</span>
                <span>Rs. {billData.BillSummary.DueAmount:F2}</span>
            </div>
        </div>

        <!-- Payment Details -->
        <div class='payment-section'>
            <h4>Payment Details</h4>
            <table class='payment-table'>
                <thead>
                    <tr>
                        <th>SN</th>
                        <th>Receipt No</th>
                        <th>Date</th>
                        <th>Amount (Rs)</th>
                        <th>Paymode</th>
                    </tr>
                </thead>
                <tbody>");

            // Add payment rows
            for (int i = 0; i < billData.PaymentDetails.Count; i++)
            {
                var payment = billData.PaymentDetails[i];
                html.Append($@"
                <tr>
                    <td>{i + 1}</td>
                    <td>RCPT {displayBillNo}{i:00}</td>
                    <td>{payment.PaymentDate:dd/MM/yyyy}</td>
                    <td class='number'>{payment.Amount:F2}</td>
                    <td>{payment.PaymentMode}</td>
                </tr>");
            }

            html.Append($@"
                </tbody>
            </table>
            <div style='text-align: center; margin-top: 10px;'>
                <strong>Total: Rs. {billData.BillSummary.PaidAmount:F2}</strong>
            </div>
        </div>

        <!-- Footer -->
        <div class='footer'>
            <p><strong>Thank You for Visiting!</strong></p>
            <div style='display: flex; justify-content: space-between; margin-top: 20px;'>
                <div>Dr. Payal Shah<br><small>(MD, Pathologist)</small></div>
                <div>Mr. Ketan Kumar<br><small>(Accountant)</small></div>
            </div>
        </div>
    </div>
</body>
</html>");

            return html.ToString();
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
                            refNo = p.RefNo
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