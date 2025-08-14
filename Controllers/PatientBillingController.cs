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
                int billSummaryId = db.SaveCompleteBill(billData); // This method handles the transaction

                if (billSummaryId > 0)
                {
                    return Json(new { success = true, message = "Bill saved successfully!", billId = billSummaryId });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to save bill. No Bill ID returned." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception details (e.g., using log4net, NLog, or a simple file logger)
                // In a real application, avoid sending detailed exception messages to the client.
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
                    return Json(new { success = false, message = "Bill not found." }, JsonRequestBehavior.AllowGet);
                }

                // Generate HTML content
                string htmlContent = GenerateBillHTML(billData, billId);

                return Content(htmlContent, "text/html");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error generating bill: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private string GenerateBillHTML(CompleteBillData billData, int billId)
        {
            var html = new StringBuilder();

            html.Append(@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Bill - " + billId + @"</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { 
            font-family: Arial, sans-serif; 
            font-size: 12px; 
            line-height: 1.4;
            color: #000;
            background: #fff;
        }
        .bill-container { 
            width: 80mm; 
            margin: 0 auto; 
            padding: 10px;
        }
        .header { 
            text-align: center; 
            border-bottom: 2px solid #000; 
            padding-bottom: 10px; 
            margin-bottom: 15px;
        }
        .header h1 { 
            font-size: 16px; 
            font-weight: bold; 
            margin-bottom: 5px;
        }
        .header p { 
            font-size: 10px; 
            margin: 2px 0;
        }
        .bill-info { 
            display: flex; 
            justify-content: space-between; 
            margin-bottom: 15px;
        }
        .bill-info div { 
            font-size: 11px;
        }
        .patient-section { 
            margin-bottom: 15px;
        }
        .patient-row { 
            display: flex; 
            justify-content: space-between; 
            margin-bottom: 3px;
        }
        .investigations-table { 
            width: 100%; 
            border-collapse: collapse; 
            margin-bottom: 15px;
        }
        .investigations-table th, .investigations-table td { 
            border: 1px solid #000; 
            padding: 4px; 
            text-align: left; 
            font-size: 10px;
        }
        .investigations-table th { 
            background-color: #f0f0f0; 
            font-weight: bold;
        }
        .investigations-table td.number { 
            text-align: right;
        }
        .totals-section { 
            margin-bottom: 15px;
        }
        .total-row { 
            display: flex; 
            justify-content: space-between; 
            margin-bottom: 2px; 
            padding: 2px 0;
        }
        .total-row.final { 
            font-weight: bold; 
            border-top: 1px solid #000; 
            padding-top: 5px;
        }
        .payment-section { 
            margin-bottom: 15px;
        }
        .payment-table { 
            width: 100%; 
            border-collapse: collapse;
        }
        .payment-table th, .payment-table td { 
            border: 1px solid #000; 
            padding: 4px; 
            text-align: left; 
            font-size: 10px;
        }
        .payment-table th { 
            background-color: #f0f0f0;
        }
        .footer { 
            text-align: center; 
            margin-top: 20px; 
            border-top: 1px solid #000; 
            padding-top: 10px;
        }
        .barcode { 
            text-align: center; 
            font-family: 'Courier New', monospace; 
            font-size: 14px; 
            margin: 10px 0;
        }
        @media print {
            body { margin: 0; }
            .bill-container { width: 100%; }
            .no-print { display: none; }
        }
    </style>
</head>
<body onload='window.print();'>
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
            <div><strong>Bill/Reg. no:</strong> " + billId + @"</div>
            <div><strong>Date:</strong> " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + @"</div>
        </div>
        
        <div class='barcode'>
            ||||| |||| | || ||||
            " + billId + @"
        </div>

        <!-- Patient Information -->
        <div class='patient-section'>
            <div class='patient-row'>
    <span><strong>Name:</strong> " + billData.PatientInfo.PatName + @"</span>
    <span><strong>UHID:</strong> " + (billData.PatientInfo.UHID ?? "NEW") + @"</span>
</div>
            <div class='patient-row'>
                <span><strong>Age/Sex:</strong> " + billData.PatientInfo.Age + " " + billData.PatientInfo.AgeType + "/" + billData.PatientInfo.Gender + @"</span>
                <span><strong>Referred by:</strong> " + (billData.PatientInfo.Ref ?? "Self") + @"</span>
            </div>
            <div class='patient-row'>
                <span><strong>Mobile:</strong> " + billData.PatientInfo.MobileNo + @"</span>
                <span><strong>Received by:</strong> Admin</span>
            </div>
            <div class='patient-row'>
                <span><strong>Address:</strong> " + (billData.PatientInfo.Area ?? "") + ", " + (billData.PatientInfo.City ?? "") + @"</span>
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

            html.Append(@"
            </tbody>
        </table>

        <!-- Totals Section -->
        <div class='totals-section'>
            <div class='total-row'>
                <span>Bill Amount:</span>
                <span>Rs. " + billData.BillSummary.TotalBill.ToString("F2") + @"</span>
            </div>
            <div class='total-row'>
                <span>Discount Amount:</span>
                <span>Rs. " + billData.BillSummary.TotalDiscountAmount.ToString("F2") + @"</span>
            </div>
            <div class='total-row final'>
                <span>Final Bill Amount:</span>
                <span>Rs. " + billData.BillSummary.NetAmount.ToString("F2") + @"</span>
            </div>
            <div class='total-row'>
                <span>Paid Amount:</span>
                <span>Rs. " + billData.BillSummary.PaidAmount.ToString("F2") + @"</span>
            </div>
            <div class='total-row'>
                <span>Due Amount:</span>
                <span>Rs. " + billData.BillSummary.DueAmount.ToString("F2") + @"</span>
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
                    <td>RCPT {billId + i}</td>
                    <td>{DateTime.Now:dd/MM/yyyy}</td>
                    <td class='number'>{payment.Amount:F2}</td>
                    <td>{payment.PaymentMode}</td>
                </tr>");
            }

            html.Append(@"
                </tbody>
            </table>
            <div style='text-align: center; margin-top: 10px;'>
                <strong>Total: Rs. " + billData.BillSummary.PaidAmount.ToString("F2") + @"</strong>
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
        public JsonResult SearchPatients(string mobileNo)
        {
            try
            {
                if (string.IsNullOrEmpty(mobileNo) || mobileNo.Length < 10)
                {
                    return Json(new { success = false, message = "Please enter a valid 10-digit mobile number." });
                }

                PatientBillingDB db = new PatientBillingDB();
                var patients = db.SearchPatientsByMobile(mobileNo);

                return Json(new
                {
                    success = true,
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
    }
}