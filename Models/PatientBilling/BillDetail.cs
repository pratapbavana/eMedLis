using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eMedLis.Models.PatientBilling
{
    public class BillDetail
    {
        [Key]
        public int BillDetailId { get; set; } // Primary Key for this table

        [Required]
        public int BillSummaryId { get; set; } // Foreign Key to BillSummary

        [ForeignKey("BillSummaryId")]
        public virtual BillSummary BillSummary { get; set; } // Navigation property

        [Required]
        [StringLength(50)]
        public string InvId { get; set; }

        [Required]
        [StringLength(200)]
        public string InvName { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Rate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DiscountPercent { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal NetAmount { get; set; }
        public string SpecimenType { get; set; }
        public string ContainerType { get; set; }
        public bool FastingRequired { get; set; }
        public string SpecialInstructions { get; set; }
    }
}