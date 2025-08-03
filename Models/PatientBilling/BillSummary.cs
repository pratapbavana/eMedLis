using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eMedLis.Models.PatientBilling
{
    public class BillSummary
    {
        [Key]
        public int BillSummaryId { get; set; } // Primary Key for this table

        [Required]
        public int PatientInfoId { get; set; } // Foreign Key to PatientInfo

        [ForeignKey("PatientInfoId")]
        public virtual PatientInfo PatientInfo { get; set; } // Navigation property

        public DateTime BillDate { get; set; } = DateTime.Now; // Defaults to current date/time

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalBill { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalDiscountAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal NetAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PaidAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DueAmount { get; set; }

        [StringLength(500)]
        public string Remarks { get; set; } // For the due amount reason
    }
}