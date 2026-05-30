using System;
using System.Collections.Generic;

namespace eMedLis.Models.ReportAuthorization
{
    public class ReportAuthorizationReviewResponse
    {
        public int SampleDetailId { get; set; }
        public string ResultStatus { get; set; }
        public string Interpretation { get; set; }
        public string RejectedReason { get; set; }
        public string AuthorizedDoctor { get; set; }
        public DateTime? AuthorizedOn { get; set; }
        public bool CanAuthorize { get; set; }
        public bool HasSignature { get; set; }
        public int? DoctorId { get; set; }
        public ReportAuthorizationPatientInfo Patient { get; set; }
        public List<ReportAuthorizationResultItem> Results { get; set; }
    }

    public class ReportAuthorizationPatientInfo
    {
        public string BillNo { get; set; }
        public string PatientName { get; set; }
        public int PatientAge { get; set; }
        public string PatientAgeType { get; set; }
        public string PatientGender { get; set; }
        public string SampleBarcode { get; set; }
        public string InvestigationName { get; set; }
        public string DepartmentName { get; set; }
    }

    public class ReportAuthorizationResultItem
    {
        public int ParameterId { get; set; }
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

    public class ReportAuthorizationActionRequest
    {
        public int SampleDetailId { get; set; }
        public string Interpretation { get; set; }
        public string RejectReason { get; set; }
    }
}
