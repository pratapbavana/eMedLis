// In Models/PatientBilling/BillListItem.cs
using System;
using System.Collections.Generic;

namespace eMedLis.Models.PatientBilling
{
    public class BillListItem
    {
        public int BillSummaryId { get; set; }
        public string BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public decimal TotalBill { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string PatName { get; set; }
        public int Age { get; set; }
        public string AgeType { get; set; }
        public string Gender { get; set; }
        public string Ref { get; set; }
        public string MobileNo { get; set; }
        public string UHID { get; set; }
        public string PaymentStatus { get; set; }
        public string Status { get; set; } = "Active";

        // Computed properties for display
        public string AgeGenderDisplay => $"{Age} {AgeType?.Substring(0, 1) ?? "Y"} / {Gender?.Substring(0, 1) ?? ""}";
        public string PaymentStatusClass
        {
            get
            {
                var status = PaymentStatus?.ToLower();
                if (status == "paid") return "badge-success";
                else if (status == "partial") return "badge-warning";
                else if (status == "unpaid") return "badge-danger";
                else return "badge-secondary";
            }
        }
    }
}