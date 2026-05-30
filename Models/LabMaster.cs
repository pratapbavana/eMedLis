namespace eMedLis.Models
{
    public class LabMaster
    {
        public int Id { get; set; }
        public string LabName { get; set; }
        public string ShortName { get; set; }
        public string Tagline { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
        public string Country { get; set; }
        public string MobileNumber { get; set; }
        public string AlternateMobile { get; set; }
        public string Landline { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string GSTNumber { get; set; }
        public string PANNumber { get; set; }
        public string LabRegistrationNumber { get; set; }
        public string NABLNumber { get; set; }
        public string DrugLicenseNumber { get; set; }
        public bool ShowLogoInReport { get; set; }
        public bool ShowGSTInReport { get; set; }
        public bool ShowAccreditationInReport { get; set; }
        public string ReceiptFooter { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public bool Active { get; set; }
        public bool HasLogo { get; set; }
        public bool HasReportHeaderImage { get; set; }
        public bool HasReportFooterImage { get; set; }

        // client upload payloads (data URL format)
        public string LogoBase64 { get; set; }
        public string ReportHeaderImageBase64 { get; set; }
        public string ReportFooterImageBase64 { get; set; }
    }
}
