using eMedLis.Domain.PatientBilling.Entities;
using System.Collections.Generic;

namespace eMedLis.Domain.PatientBilling.ViewModels
{
    public class PatientBillViewModel
    {
        public int? PatientInfoId { get; set; }
        public PatientInfo PatientDetails { get; set; }
        public BillSummary SummaryDetails { get; set; }
        public List<BillDetail> BillDetails { get; set; } = new List<BillDetail>();
        public List<PaymentDetail> PaymentDetails { get; set; } = new List<PaymentDetail>();
    }
}