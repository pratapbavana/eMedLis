// In Models/PatientBilling/BillSaveResult.cs
namespace eMedLis.Domain.PatientBilling.Entities 
{ 
    public class BillSaveResult
    {
        public int BillSummaryId { get; set; }
        public string BillNo { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
