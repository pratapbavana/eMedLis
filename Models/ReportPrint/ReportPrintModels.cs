using System;
using System.Collections.Generic;

namespace eMedLis.Models.ReportPrint
{
    public class ReportPrintSearchItem
    {
        public int SampleDetailId { get; set; }
        public int SampleCollectionId { get; set; }
        public int BillSummaryId { get; set; }
        public string SampleBarcode { get; set; }
        public string BillNo { get; set; }
        public string PatientName { get; set; }
        public int PatientAge { get; set; }
        public string PatientAgeType { get; set; }
        public string PatientGender { get; set; }
        public string MobileNo { get; set; }
        public string InvestigationName { get; set; }
        public string DepartmentName { get; set; }
        public DateTime? CollectionDate { get; set; }
        public string ResultStatus { get; set; }
    }

    public class ReportPrintBillItem
    {
        public int BillSummaryId { get; set; }
        public int SampleCollectionId { get; set; }
        public string BillNo { get; set; }
        public string CollectionBarcode { get; set; }
        public string PatientName { get; set; }
        public int PatientAge { get; set; }
        public string PatientAgeType { get; set; }
        public string PatientGender { get; set; }
        public string MobileNo { get; set; }
        public DateTime? CollectionDate { get; set; }
        public int InvestigationCount { get; set; }
    }

    public class ReportPrintSelectionRequest
    {
        public int BillSummaryId { get; set; }
        public List<int> SampleDetailIds { get; set; }
        public string PrintOption { get; set; }
    }
}
