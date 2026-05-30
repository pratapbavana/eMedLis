using System;
using System.Collections.Generic;

namespace eMedLis.Models.ReportPrint
{
    public class ReportPreviewDocument
    {
        public int SampleDetailId { get; set; }
        public int BillSummaryId { get; set; }
        public string BillNo { get; set; }
        public DateTime? BillDate { get; set; }
        public string SampleBarcode { get; set; }
        public string PatientName { get; set; }
        public int PatientAge { get; set; }
        public string PatientAgeType { get; set; }
        public string PatientGender { get; set; }
        public string MobileNo { get; set; }
        public string ReferralDoctor { get; set; }
        public string InvestigationName { get; set; }
        public string DepartmentName { get; set; }
        public DateTime? CollectionDate { get; set; }
        public string ResultStatus { get; set; }
        public string DoctorInterpretation { get; set; }
        public DateTime? AuthorizedOn { get; set; }
        public string AuthorizedDoctor { get; set; }
        public int? AuthorizedDoctorId { get; set; }
        public bool HasSignature { get; set; }
        public List<ReportPreviewParameterItem> Parameters { get; set; }
    }

    public class ReportPreviewParameterItem
    {
        public int SampleDetailId { get; set; }
        public string HeaderName { get; set; }
        public string ParameterName { get; set; }
        public string MethodName { get; set; }
        public string ResultValue { get; set; }
        public string Unit { get; set; }
        public string NormalRange { get; set; }
        public string Flag { get; set; }
        public bool IsCritical { get; set; }
        public int DisplayOrder { get; set; }
    }
}
