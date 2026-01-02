using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eMedLis.Domain.PatientBilling.Entities
{
    public class PaymentReceiptData
    {
        public DuePayment Payment { get; set; }
        public BillSummary Bill { get; set; }
        public PatientInfo Patient { get; set; }
    }
}