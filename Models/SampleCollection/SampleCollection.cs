using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eMedLis.Models.SampleCollection
{
    public class SampleCollectionModel
    {
        [Key]
        public int SampleCollectionId { get; set; }
        public int BillSummaryId { get; set; }
        public int PatientInfoId { get; set; }
        public DateTime CollectionDate { get; set; }
        public TimeSpan CollectionTime { get; set; }
        public string CollectedBy { get; set; }
        public string CollectionBarcode { get; set; }
        public string CollectionStatus { get; set; }
        public string Priority { get; set; }
        public string Remarks { get; set; }
        public bool HomeCollection { get; set; }
        public string PatientAddress { get; set; }
        public decimal CollectionCharges { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }

        public virtual List<SampleCollectionDetail> CollectionDetails { get; set; }
    }

    public class SampleCollectionDetail
    {
        [Key]
        public int SampleDetailId { get; set; }
        public int SampleCollectionId { get; set; }
        public int InvMasterId { get; set; }
        public string InvestigationName { get; set; }
        public string SpecimenType { get; set; }
        public string ContainerType { get; set; }
        public string SampleBarcode { get; set; }
        public string CollectionInstructions { get; set; }
        public bool FastingRequired { get; set; }
        public string SpecialInstructions { get; set; }
        public string SampleStatus { get; set; }
        public string CollectedQuantity { get; set; }
        public string RejectionReason { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ContainerMaster
    {
        [Key]
        public int ContainerId { get; set; }
        public string ContainerName { get; set; }
        public string ContainerCode { get; set; }
        public string CapColor { get; set; }
        public string Volume { get; set; }
        public string Additive { get; set; }
        public string StorageTemp { get; set; }
        public int ExpiryDays { get; set; }
        public bool Active { get; set; }
    }

    public class SampleCollectionViewModel
    {
        public PatientBilling.PatientInfo PatientInfo { get; set; }
        public PatientBilling.BillSummary BillSummary { get; set; }
        public List<PatientBilling.BillDetail> BillDetails { get; set; }
        public SampleCollectionModel SampleCollection { get; set; }
        public List<SampleCollectionDetail> SampleDetails { get; set; }
        public List<ContainerMaster> AvailableContainers { get; set; }
    }

    public class CollectionInstructions
    {
        [Key]
        public int InstructionId { get; set; }
        public int InvMasterId { get; set; }
        public string PatientInstructions { get; set; }
        public string PreCollectionInstructions { get; set; }
        public string PostCollectionInstructions { get; set; }
        public int FastingHours { get; set; }
        public string SpecialInstructions { get; set; }
    }
}