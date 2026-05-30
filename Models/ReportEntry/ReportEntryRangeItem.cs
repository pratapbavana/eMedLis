namespace eMedLis.Models.ReportEntry
{
    public class ReportEntryRangeItem
    {
        public int ParameterId { get; set; }
        public decimal? NormalMin { get; set; }
        public decimal? NormalMax { get; set; }
        public decimal? CriticalMin { get; set; }
        public decimal? CriticalMax { get; set; }
        public string RangeText { get; set; }
        public string DisplayRange { get; set; }
        public bool Found { get; set; }
    }
}
