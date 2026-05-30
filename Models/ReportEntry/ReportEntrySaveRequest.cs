using System.Collections.Generic;

namespace eMedLis.Models.ReportEntry
{
    public class ReportEntrySaveRequest
    {
        public int SampleDetailId { get; set; }
        public string TargetStatus { get; set; }
        public List<ReportEntrySaveItem> Items { get; set; }
    }

    public class ReportEntrySaveItem
    {
        public int ParameterId { get; set; }
        public int? MethodId { get; set; }
        public string ResultValue { get; set; }
        public string ResultType { get; set; }
        public string Unit { get; set; }
        public decimal? NormalMin { get; set; }
        public decimal? NormalMax { get; set; }
        public decimal? CriticalMin { get; set; }
        public decimal? CriticalMax { get; set; }
        public string RangeText { get; set; }
        public string Flag { get; set; }
        public bool IsCritical { get; set; }
        public int DisplayOrder { get; set; }
    }
}
