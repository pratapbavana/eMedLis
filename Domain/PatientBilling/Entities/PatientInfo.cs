using System;
using System.ComponentModel.DataAnnotations;

namespace eMedLis.Domain.PatientBilling.Entities
{
    public class PatientInfo
    {
        [Key]
        public int PatientInfoId { get; set; } // Primary Key for this table

        [StringLength(20)]
        public string UHID { get; set; } // New UHID field

        [Required]
        [StringLength(20)]
        public string MobileNo { get; set; }

        [Required]
        [StringLength(100)]
        public string PatName { get; set; }

        public int Age { get; set; }

        [StringLength(10)]
        public string AgeType { get; set; } // e.g., "Years", "Months", "Days"

        [StringLength(10)]
        public string Gender { get; set; }

        [StringLength(100)]
        public string Ref { get; set; } // Referring Doctor/Source

        [StringLength(100)]
        public string Area { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        public DateTime? LastVisit { get; set; }
    }
}