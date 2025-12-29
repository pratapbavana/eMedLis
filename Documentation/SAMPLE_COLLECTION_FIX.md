# Fix: Partial Sample Collection - Collected Samples Showing as Pending

## Issue
When a bill with 3 tests has sample collected for 1 test, reopening the bill shows ALL 3 tests as pending and allows collecting the same sample again.

## Root Cause
1. **SampleCollectionDetail** status tracking exists but isn't properly enforced
2. When fetching samples for collection, the query doesn't filter OUT already collected samples
3. The UI/Frontend doesn't distinguish between collected and pending samples

## Solution

### 1. Modify GetBillForCollection Method (SampleCollectionDB.cs)

**Current Issue**: This method returns ALL tests in a bill without checking if they're already collected.

**Fix**: Filter out already collected samples from previous collection sessions:

```csharp
public SampleCollectionViewModel GetBillForCollection(int billSummaryId)
{
    var viewModel = new SampleCollectionViewModel();

    using (var connection = new SqlConnection(_connectionString))
    {
        using (var cmd = new SqlCommand("usp_GetBillForSampleCollection", connection))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@BillSummaryId", billSummaryId);
            cmd.Parameters.AddWithValue("@OnlyPending", 1); // NEW PARAMETER

            connection.Open();
            // ... rest of code
        }
    }

    return viewModel;
}
```

### 2. Update Stored Procedure: usp_GetBillForSampleCollection

**Location**: Documentation/script.sql

```sql
ALTER PROCEDURE [dbo].[usp_GetBillForSampleCollection]
    @BillSummaryId INT,
    @OnlyPending INT = 1
AS
BEGIN
    SELECT 
        bs.BillSummaryId,
        bs.BillNo,
        bs.BillDate,
        bs.NetAmount,
        bs.TotalBill,
        bs.TotalDiscountAmount,
        bs.PaidAmount,
        bs.DueAmount,
        pi.PatientInfoId,
        pi.UHID,
        pi.PatName,
        pi.MobileNo,
        pi.Age,
        pi.AgeType,
        pi.Gender,
        pi.Area,
        pi.City,
        pi.Ref,
        bd.InvId,
        bd.InvName,
        bd.Rate,
        i.SpecimenId,
        s.SpecimenName,
        i.VacutainerId,
        c.ContainerName as VacutainerName,
        i.FastingRequired,
        i.GuideLines as SpecialInstructions,
        COALESCE(scd.SampleStatus, 'Pending') as CurrentSampleStatus,
        scd.IsCollected,
        scd.SampleDetailId
    FROM BillSummary bs
    INNER JOIN PatientInfo pi ON bs.PatientInfoId = pi.PatientInfoId
    INNER JOIN BillDetail bd ON bs.BillSummaryId = bd.BillSummaryId
    LEFT JOIN Investigations i ON bd.InvId = i.Id
    LEFT JOIN LookupTable s ON i.SpecimenId = s.Id AND s.ItemType = 'Specimen'
    LEFT JOIN ContainerMaster c ON i.VacutainerId = c.ContainerId
    LEFT JOIN SampleCollectionDetail scd ON i.Id = scd.InvMasterId 
        AND scd.SampleCollectionId = (
            SELECT TOP 1 SampleCollectionId 
            FROM SampleCollection 
            WHERE BillSummaryId = @BillSummaryId 
            ORDER BY SampleCollectionId DESC
        )
    WHERE bs.BillSummaryId = @BillSummaryId
    AND (
        @OnlyPending = 0 
        OR COALESCE(scd.SampleStatus, 'Pending') != 'Collected'
    )
    ORDER BY bd.InvId
END
```

### 3. Add New Method: GetCollectionSummary

Add to **SampleCollectionDB.cs** to show which samples are already collected:

```csharp
public SampleCollectionViewModel GetCollectionSummary(int billSummaryId)
{
    var viewModel = new SampleCollectionViewModel();
    var sampleDetails = new List<SampleCollectionDetail>();
    var collectedSamples = new List<SampleCollectionDetail>();

    using (var connection = new SqlConnection(_connectionString))
    {
        using (var cmd = new SqlCommand(@"
            SELECT 
                scd.SampleDetailId,
                scd.InvMasterId,
                scd.InvestigationName,
                scd.SampleStatus,
                scd.CollectionDate,
                scd.CollectionTime,
                scd.RejectionReason,
                scd.IsCollected,
                scd.IsRejected
            FROM SampleCollectionDetail scd
            INNER JOIN SampleCollection sc ON scd.SampleCollectionId = sc.SampleCollectionId
            WHERE sc.BillSummaryId = @BillSummaryId
            ORDER BY scd.InvMasterId, scd.SampleDetailId DESC
        ", connection))
        {
            cmd.Parameters.AddWithValue("@BillSummaryId", billSummaryId);
            connection.Open();

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var detail = new SampleCollectionDetail
                    {
                        SampleDetailId = SafeGetInt(reader, "SampleDetailId"),
                        InvMasterId = SafeGetInt(reader, "InvMasterId"),
                        InvestigationName = SafeGetString(reader, "InvestigationName"),
                        SampleStatus = SafeGetString(reader, "SampleStatus"),
                        CollectionDate = SafeGetDateTime(reader, "CollectionDate", null),
                        CollectionTime = SafeGetTimeSpan(reader, "CollectionTime", null),
                        RejectionReason = SafeGetString(reader, "RejectionReason"),
                        IsCollected = SafeGetBoolean(reader, "IsCollected"),
                        IsRejected = SafeGetBoolean(reader, "IsRejected")
                    };

                    if (detail.SampleStatus == "Collected")
                        collectedSamples.Add(detail);
                    else
                        sampleDetails.Add(detail);
                }
            }
        }
    }

    viewModel.SampleDetails = sampleDetails;
    viewModel.CollectedSamples = collectedSamples; // New property
    return viewModel;
}
```

### 4. Update SampleCollectionViewModel

Add to **Models/SampleCollection/SampleCollectionViewModel.cs**:

```csharp
public class SampleCollectionViewModel
{
    // ... existing properties ...
    
    /// <summary>
    /// List of samples already collected in previous sessions
    /// </summary>
    public List<SampleCollectionDetail> CollectedSamples { get; set; } = new List<SampleCollectionDetail>();
    
    /// <summary>
    /// Get count of pending samples for this bill
    /// </summary>
    public int GetPendingCount()
    {
        if (SampleDetails == null) return 0;
        return SampleDetails.Count(s => s.SampleStatus != "Collected" && !s.IsCollected);
    }
    
    /// <summary>
    /// Get count of collected samples for this bill
    /// </summary>
    public int GetCollectedCount()
    {
        if (CollectedSamples == null) return 0;
        return CollectedSamples.Count;
    }
}
```

### 5. Controller Update

In **Controllers/SampleCollectionController.cs**, when displaying bill for collection:

```csharp
[HttpGet]
public ActionResult CollectSample(int billSummaryId)
{
    try
    {
        var db = new SampleCollectionDB();
        
        // Get collection summary (what's already collected)
        var collectionSummary = db.GetCollectionSummary(billSummaryId);
        
        // Get only pending samples for collection
        var viewModel = db.GetBillForCollection(billSummaryId);
        viewModel.CollectedSamples = collectionSummary.CollectedSamples;
        
        // If all samples collected, show message
        if (viewModel.GetPendingCount() == 0)
        {
            ViewBag.Message = "All samples for this bill have already been collected.";
            viewModel.SampleDetails = new List<SampleCollectionDetail>();
        }
        
        return View(viewModel);
    }
    catch (Exception ex)
    {
        // Log exception
        return RedirectToAction("Error");
    }
}
```

### 6. UI Enhancement

In the view, show:
- **Collected Samples Section**: Display samples already collected with dates
- **Pending Samples Section**: Show only pending samples for collection
- **Status Indicator**: Badge or color-coding for each status

## Database Considerations

**Ensure these indices exist**:
```sql
CREATE INDEX IX_SampleCollectionDetail_SampleStatus 
 ON SampleCollectionDetail(SampleCollectionId, SampleStatus);

CREATE INDEX IX_SampleCollection_BillSummaryId 
 ON SampleCollection(BillSummaryId, SampleCollectionId DESC);
```

## Testing

1. Create bill with 3 tests
2. Collect sample for 1 test
3. Reopen bill - should show:
   - 1 collected sample (in collected section, read-only)
   - 2 pending samples (available for collection)
4. Verify no duplicate collection option

## Impact
- **Breaking Change**: No
- **Migration Required**: No (uses existing schema)
- **Performance Impact**: Minimal (adds one parameter to stored procedure)
