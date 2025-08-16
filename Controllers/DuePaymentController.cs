// In Controllers/DuePaymentController.cs
using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using eMedLis.DAL.PatientBilling;
using eMedLis.Models.PatientBilling;

namespace eMedLis.Controllers
{
    public class DuePaymentController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetDueBills(int days = 30)
        {
            try
            {
                var db = new PatientBillingDB();
                var bills = db.GetDueBills(days);

                var result = bills.Select(b => new {
                    b.BillSummaryId,
                    b.BillNo,
                    BillDate = b.BillDate.ToString("dd/MM/yyyy"),
                    b.PatName,
                    AgeGender = b.AgeGenderDisplay,
                    b.MobileNo,
                    b.UHID,
                    NetAmount = b.NetAmount.ToString("F2"),
                    PaidAmount = b.PaidAmount.ToString("F2"),
                    DueAmount = b.DueAmount.ToString("F2"),
                    b.DaysPending
                });

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ProcessPayment(DuePayment payment)
        {
            try
            {
                var db = new PatientBillingDB();
                var result = db.ProcessDuePayment(payment);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Payment processed successfully!",
                        duePaymentId = result.DuePaymentId,
                        receiptNo = result.ReceiptNo
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error processing payment: " + ex.Message });
            }
        }

        [HttpGet]
        public ActionResult PrintReceipt(int paymentId)
        {
            try
            {
                var db = new PatientBillingDB();
                var receiptData = db.GetPaymentReceipt(paymentId);

                if (receiptData == null)
                {
                    return Content("<h1>Receipt not found</h1>");
                }

                string htmlContent = GenerateReceiptHTML(receiptData);
                return Content(htmlContent, "text/html");
            }
            catch (Exception ex)
            {
                return Content($"<h1>Error: {ex.Message}</h1>");
            }
        }

        private string GenerateReceiptHTML(PaymentReceiptData data)
        {
            var html = new StringBuilder();

            html.Append($@"
<!DOCTYPE html>
<html>
<head>
    <title>Payment Receipt - {data.Payment.ReceiptNo}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; }}
        .receipt-container {{ max-width: 400px; margin: 0 auto; }}
        .header {{ text-align: center; border-bottom: 2px solid #000; padding-bottom: 10px; margin-bottom: 15px; }}
        .header h1 {{ font-size: 16px; margin: 0; }}
        .header p {{ font-size: 10px; margin: 2px 0; }}
        .receipt-info {{ margin-bottom: 15px; }}
        .receipt-row {{ display: flex; justify-content: space-between; margin-bottom: 5px; font-size: 12px; }}
        .patient-section, .payment-section {{ margin-bottom: 15px; }}
        .section-title {{ font-weight: bold; border-bottom: 1px solid #ccc; margin-bottom: 5px; }}
        .amount-highlight {{ font-size: 14px; font-weight: bold; text-align: center; 
                           border: 2px solid #000; padding: 10px; margin: 15px 0; }}
        .footer {{ text-align: center; margin-top: 20px; border-top: 1px solid #000; padding-top: 10px; }}
        @media print {{ body {{ margin: 0; }} }}
    </style>
</head>
<body onload='window.print();'>
    <div class='receipt-container'>
        <div class='header'>
            <h1>eMedLis Pathology Software</h1>
            <p>Plot No - 4, Ashoka Chowk, Opp Military General Hospital</p>
            <p>Pune, 462023 | Phone: 923456278, 939003261</p>
            <p><strong>PAYMENT RECEIPT</strong></p>
        </div>

        <div class='receipt-info'>
            <div class='receipt-row'>
                <span><strong>Receipt No:</strong> {data.Payment.ReceiptNo}</span>
                <span><strong>Date:</strong> {data.Payment.PaymentDate:dd/MM/yyyy HH:mm}</span>
            </div>
        </div>

        <div class='patient-section'>
            <div class='section-title'>Patient Details</div>
            <div class='receipt-row'>
                <span><strong>Name:</strong> {data.Patient.PatName}</span>
                <span><strong>UHID:</strong> {data.Patient.UHID ?? "N/A"}</span>
            </div>
            <div class='receipt-row'>
                <span><strong>Mobile:</strong> {data.Patient.MobileNo}</span>
                <span><strong>Age/Sex:</strong> {data.Patient.Age}/{data.Patient.Gender}</span>
            </div>
        </div>

        <div class='payment-section'>
            <div class='section-title'>Bill & Payment Details</div>
            <div class='receipt-row'>
                <span><strong>Original Bill No:</strong> {data.Bill.BillNo}</span>
            </div>
            <div class='receipt-row'>
                <span>Bill Amount:</span>
                <span>₹{data.Bill.NetAmount:F2}</span>
            </div>
            <div class='receipt-row'>
                <span>Total Paid (including this):</span>
                <span>₹{data.Bill.PaidAmount:F2}</span>
            </div>
            <div class='receipt-row'>
                <span>Remaining Due:</span>
                <span>₹{data.Bill.DueAmount:F2}</span>
            </div>
        </div>

        <div class='amount-highlight'>
            <div>PAYMENT RECEIVED: ₹{data.Payment.Amount:F2}</div>
            <div style='font-size: 12px; margin-top: 5px;'>
                Mode: {data.Payment.PaymentMode}
                {(string.IsNullOrEmpty(data.Payment.RefNo) ? "" : $" | Ref: {data.Payment.RefNo}")}
            </div>
        </div>

        {(string.IsNullOrEmpty(data.Payment.Remarks) ? "" : $@"
        <div style='font-size: 11px; margin: 10px 0;'>
            <strong>Remarks:</strong> {data.Payment.Remarks}
        </div>")}

        <div class='footer'>
            <p style='font-size: 10px;'>Thank you for your payment!</p>
            <p style='font-size: 10px;'>Received by: {data.Payment.ReceivedBy ?? "System"}</p>
        </div>
    </div>
</body>
</html>");

            return html.ToString();
        }
    }
}
