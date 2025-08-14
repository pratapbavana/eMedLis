using System.Collections.Generic;

namespace eMedLis.Models.PatientBilling
{
    public class CompleteBillData
    {
        public PatientInfo PatientInfo { get; set; }
        public BillSummary BillSummary { get; set; }
        public List<BillDetail> BillDetails { get; set; }
        public List<PaymentDetail> PaymentDetails { get; set; }

        public CompleteBillData()
        {
            BillDetails = new List<BillDetail>();
            PaymentDetails = new List<PaymentDetail>();
        }
    }
}
