using System.Collections.Generic;

namespace eMedLis.Models.ReportEntry
{
    public class ReportEntryTemplateResponse
    {
        public ReportEntryPatientContext Patient { get; set; }
        public List<ReportEntryTemplateItem> TemplateItems { get; set; }
        public List<ReportEntryMethodItem> Methods { get; set; }
        public string ResultStatus { get; set; }
        public bool IsEditable { get; set; }
        public List<ReportEntrySavedResultItem> SavedResults { get; set; }
    }

    public class ReportEntryPatientContext
    {
        public int SampleDetailId { get; set; }
        public int InvestigationId { get; set; }
        public string InvestigationName { get; set; }
        public string PatientName { get; set; }
        public int PatientAge { get; set; }
        public string PatientAgeType { get; set; }
        public string PatientGender { get; set; }
        public string SampleBarcode { get; set; }
        public string BillNo { get; set; }
        public int AgeInDays { get; set; }
    }

    public class ReportEntryTemplateItem
    {
        public string ItemType { get; set; }
        public int? HeaderId { get; set; }
        public string HeaderName { get; set; }
        public int? ParameterId { get; set; }
        public string ParameterName { get; set; }
        public int? DefaultMethodId { get; set; }
        public string DefaultMethodName { get; set; }
        public string Unit { get; set; }
        public string ResultType { get; set; }
        public string Formula { get; set; }
        public int DecimalPrecision { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class ReportEntryMethodItem
    {
        public int MethodId { get; set; }
        public string MethodName { get; set; }
    }
}
