namespace eMedLis.Models.ReportSettings
{
    public class ReportLayoutSettings
    {
        public string PrintMode { get; set; }
        public bool PrintHeader { get; set; }
        public int HeaderHeightPx { get; set; }
        public bool ShowLogo { get; set; }
        public bool ShowLabDetails { get; set; }
        public bool PrintFooter { get; set; }
        public int FooterHeightPx { get; set; }
        public string FooterText { get; set; }
        public int TopMarginPx { get; set; }
        public int LeftMarginPx { get; set; }
        public int RightMarginPx { get; set; }
        public int BottomMarginPx { get; set; }
        public int ContentStartPx { get; set; }
        public string LabName { get; set; }
        public string LabAddress { get; set; }
        public string LabPhone { get; set; }

        public static ReportLayoutSettings CreateDefault()
        {
            return new ReportLayoutSettings
            {
                PrintMode = "PlainPaper",
                PrintHeader = true,
                HeaderHeightPx = 120,
                ShowLogo = true,
                ShowLabDetails = true,
                PrintFooter = true,
                FooterHeightPx = 60,
                FooterText = "This is a system generated report.",
                TopMarginPx = 38,
                LeftMarginPx = 38,
                RightMarginPx = 38,
                BottomMarginPx = 38,
                ContentStartPx = 0,
                LabName = "SSK Diagnostics",
                LabAddress = "",
                LabPhone = ""
            };
        }
    }
}
