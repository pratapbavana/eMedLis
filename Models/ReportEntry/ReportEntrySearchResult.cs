using System;

namespace eMedLis.Models.ReportEntry
{
    public class ReportEntrySearchResult
    {
        public int SampleCollectionId { get; set; }
        public string CollectionBarcode { get; set; }
        public DateTime? CollectionDate { get; set; }
        public int BillSummaryId { get; set; }
        public string BillNo { get; set; }
        public DateTime? BillDate { get; set; }
        public int PatientInfoId { get; set; }
        public string PatientName { get; set; }
        public int PatientAge { get; set; }
        public string PatientAgeType { get; set; }
        public string PatientGender { get; set; }
        public string MobileNo { get; set; }
        public string UHID { get; set; }
        public int InvestigationCount { get; set; }
    }
}
