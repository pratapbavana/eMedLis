using System.Collections.Generic;

namespace eMedLis.Models.PatientBilling
{
    public class PatientBillViewModel
    {
        public PatientInfo PatientDetails { get; set; }
        public BillSummary SummaryDetails { get; set; }
        public List<BillDetail> BillDetails { get; set; } = new List<BillDetail>();
        public List<PaymentDetail> PaymentDetails { get; set; } = new List<PaymentDetail>();
    }
}