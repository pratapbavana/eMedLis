using eMedLis.Models.SampleCollection;
using System;
using System.Collections.Generic;
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
        public bool IsFromPackage { get; set; }
        public int? PackageId { get; set; }
        public string ParentPackageCode { get; set; }
        public string ParentPackageName { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PackagePrice { get; set; }
        public bool IsPackageChargeOwner { get; set; }
        public string SpecimenType { get; set; }
        public string ContainerType { get; set; }
        public bool FastingRequired { get; set; }
        public string SpecialInstructions { get; set; }
        public int SampleCollectionId { get; set; }
        public string SampleStatus { get; set; }
        public DateTime? CollectionDate { get; set; }  
        public TimeSpan? CollectionTime { get; set; }
        public string CollectedQuantity { get; set; }
        public string RejectionReason { get; set; }
    }
}
