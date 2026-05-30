namespace eMedLis.Models.ReportEntry
{
    public class ReportEntryInvestigationItem
    {
        public int SampleDetailId { get; set; }
        public int InvMasterId { get; set; }
        public string InvestigationName { get; set; }
        public string SampleBarcode { get; set; }
        public string SpecimenType { get; set; }
        public bool HasTemplate { get; set; }
    }
}
