using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eMedLis.Domain.PatientBilling.Entities
{
    public class DueBillItem
    {
        public int BillSummaryId { get; set; }
        public string BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public string PatName { get; set; }
        public string MobileNo { get; set; }
        public string UHID { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public int DaysPending { get; set; }

        public string AgeGenderDisplay => $"{Age} / {Gender?.Substring(0, 1)}";
        public string DueAmountFormatted => $"₹{DueAmount:F2}";
    }
}