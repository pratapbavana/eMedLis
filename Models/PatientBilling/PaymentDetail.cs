using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eMedLis.Models.PatientBilling
{
    public class PaymentDetail
    {
        [Key]
        public int PaymentDetailId { get; set; } // Primary Key for this table

        [Required]
        public int BillSummaryId { get; set; } // Foreign Key to BillSummary

        [ForeignKey("BillSummaryId")]
        public virtual BillSummary BillSummary { get; set; } // Navigation property

        [Required]
        [StringLength(50)]
        public string PaymentMode { get; set; } // e.g., "Cash", "Card", "Cheque"

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [StringLength(100)]
        public string RefNo { get; set; } // Reference number for non-cash payments
        public bool IsDuePayment { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}