using System;

namespace eMedLis.Models.ReportAuthorization
{
    public class ReportAuthorizationListItem
    {
        public int SampleDetailId { get; set; }
        public int SampleCollectionId { get; set; }
        public string SampleBarcode { get; set; }
        public string PatientName { get; set; }
        public int PatientAge { get; set; }
        public string PatientAgeType { get; set; }
        public string PatientGender { get; set; }
        public string InvestigationName { get; set; }
        public string DepartmentName { get; set; }
        public DateTime? CollectionDate { get; set; }
        public string ResultStatus { get; set; }
        public bool HasCritical { get; set; }
    }
}
