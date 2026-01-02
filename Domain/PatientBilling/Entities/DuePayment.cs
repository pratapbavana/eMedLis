using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace eMedLis.Domain.PatientBilling.Entities
{
    public class DuePayment
    {
        [Key]
        public int DuePaymentId { get; set; }
        public int BillSummaryId { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        [Required]
        public string PaymentMode { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public string RefNo { get; set; }
        public string ReceivedBy { get; set; }
        public string Remarks { get; set; }
        public string ReceiptNo { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}