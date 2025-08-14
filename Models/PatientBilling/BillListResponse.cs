using System.Collections.Generic;

namespace eMedLis.Models.PatientBilling
{
    public class BillListResponse
    {
        public List<BillListItem> Bills { get; set; } = new List<BillListItem>();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (TotalRecords + PageSize - 1) / PageSize;
    }
}