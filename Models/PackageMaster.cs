using System.Collections.Generic;

namespace eMedLis.Models
{
    public class PackageMaster
    {
        public int Id { get; set; }
        public string PackageCode { get; set; }
        public string PackageName { get; set; }
        public string ReportingName { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public string InvestigationIds { get; set; }
        public string Investigations { get; set; }
        public int InvestigationCount { get; set; }
    }

    public class PackageInvestigationItem
    {
        public string Id { get; set; }
        public string InvCode { get; set; }
        public string InvName { get; set; }
        public decimal Rate { get; set; }
    }
}
