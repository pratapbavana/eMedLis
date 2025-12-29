-- ===================================================================
-- FIX: Partial Sample Collection - Collected Samples Showing as Pending
-- ===================================================================
-- This script updates stored procedures to properly handle collected
-- samples and prevent them from appearing as pending on bill reopening
-- ===================================================================

USE [eMedLis]
GO

-- ===================================================================
-- 1. UPDATE: usp_GetBillForSampleCollection
--    Purpose: Fetch only PENDING samples from a bill
-- ===================================================================

IF OBJECT_ID('dbo.usp_GetBillForSampleCollection', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetBillForSampleCollection
GO

CREATE PROCEDURE [dbo].[usp_GetBillForSampleCollection]
    @BillSummaryId INT,
    @OnlyPending BIT = 1  -- 1 = Only Pending, 0 = All
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get Bill Summary and Patient Info
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
        -- Get the current status from latest sample collection detail
        COALESCE(scd.SampleStatus, 'Pending') as CurrentSampleStatus,
        COALESCE(scd.IsCollected, 0) as IsCollected,
        COALESCE(scd.SampleDetailId, 0) as SampleDetailId,
        -- Add flag to show if sample was already collected
        CASE WHEN COALESCE(scd.SampleStatus, 'Pending') = 'Collected' THEN 1 ELSE 0 END as AlreadyCollected
    FROM BillSummary bs
    INNER JOIN PatientInfo pi ON bs.PatientInfoId = pi.PatientInfoId
    INNER JOIN BillDetail bd ON bs.BillSummaryId = bd.BillSummaryId
    LEFT JOIN Investigations i ON bd.InvId = i.Id
    LEFT JOIN LookupTable s ON i.SpecimenId = s.Id AND s.ItemType = 'Specimen'
    LEFT JOIN ContainerMaster c ON i.VacutainerId = c.ContainerId
    -- Get the latest sample collection detail for this bill
    LEFT JOIN SampleCollectionDetail scd ON i.Id = scd.InvMasterId 
        AND scd.SampleCollectionId = (
            SELECT TOP 1 SampleCollectionId 
            FROM SampleCollection 
            WHERE BillSummaryId = @BillSummaryId 
            ORDER BY SampleCollectionId DESC
        )
    WHERE bs.BillSummaryId = @BillSummaryId
    -- Filter: If @OnlyPending = 1, exclude already collected samples
    AND (
        @OnlyPending = 0 
        OR COALESCE(scd.SampleStatus, 'Pending') NOT IN ('Collected', 'Rejected')
    )
    ORDER BY bd.InvId;
END
GO

-- ===================================================================
-- 2. NEW PROCEDURE: usp_GetCollectionSummaryForBill
--    Purpose: Get summary of collected and pending samples for a bill
-- ===================================================================

IF OBJECT_ID('dbo.usp_GetCollectionSummaryForBill', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetCollectionSummaryForBill
GO

CREATE PROCEDURE [dbo].[usp_GetCollectionSummaryForBill]
    @BillSummaryId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get all collection history for this bill
    SELECT 
        scd.SampleDetailId,
        scd.InvMasterId,
        scd.InvestigationName,
        scd.SpecimenType,
        scd.ContainerType,
        scd.SampleBarcode,
        scd.SampleStatus,
        scd.CollectionDate,
        scd.CollectionTime,
        scd.CollectedQuantity,
        scd.RejectionReason,
        scd.RejectionDate,
        scd.IsCollected,
        scd.IsRejected,
        sc.SampleCollectionId,
        sc.CollectionBarcode,
        sc.CollectedBy,
        ROW_NUMBER() OVER (PARTITION BY scd.InvMasterId ORDER BY scd.SampleDetailId DESC) as RowNum
    FROM SampleCollectionDetail scd
    INNER JOIN SampleCollection sc ON scd.SampleCollectionId = sc.SampleCollectionId
    WHERE sc.BillSummaryId = @BillSummaryId
    ORDER BY scd.InvMasterId, scd.SampleDetailId DESC;
END
GO

-- ===================================================================
-- 3. NEW PROCEDURE: usp_GetPendingSamplesForBill
--    Purpose: Get only samples that are still pending collection
-- ===================================================================

IF OBJECT_ID('dbo.usp_GetPendingSamplesForBill', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetPendingSamplesForBill
GO

CREATE PROCEDURE [dbo].[usp_GetPendingSamplesForBill]
    @BillSummaryId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get all tests from bill
    SELECT 
        bd.InvId,
        bd.InvName,
        i.Id as InvMasterId,
        i.SpecimenId,
        s.SpecimenName,
        i.VacutainerId,
        c.ContainerName,
        i.FastingRequired,
        i.GuideLines as SpecialInstructions
    FROM BillDetail bd
    LEFT JOIN Investigations i ON bd.InvId = i.Id
    LEFT JOIN LookupTable s ON i.SpecimenId = s.Id AND s.ItemType = 'Specimen'
    LEFT JOIN ContainerMaster c ON i.VacutainerId = c.ContainerId
    WHERE bd.BillSummaryId = @BillSummaryId
    -- Exclude if already collected
    AND NOT EXISTS (
        SELECT 1 FROM SampleCollectionDetail scd
        INNER JOIN SampleCollection sc ON scd.SampleCollectionId = sc.SampleCollectionId
        WHERE sc.BillSummaryId = @BillSummaryId
        AND scd.InvMasterId = i.Id
        AND scd.SampleStatus = 'Collected'
    )
    ORDER BY bd.InvId;
END
GO

-- ===================================================================
-- 4. UPDATE: Ensure SampleCollectionDetail properly tracks status
-- ===================================================================

-- Create indices for better performance
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SampleCollectionDetail_Status')
CREATE NONCLUSTERED INDEX IX_SampleCollectionDetail_Status 
    ON [dbo].[SampleCollectionDetail] ([SampleCollectionId], [SampleStatus])
    INCLUDE ([InvMasterId], [IsCollected], [IsRejected])
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SampleCollection_Bill_Desc')
CREATE NONCLUSTERED INDEX IX_SampleCollection_Bill_Desc 
    ON [dbo].[SampleCollection] ([BillSummaryId], [SampleCollectionId] DESC)
GO

-- ===================================================================
-- 5. VERIFICATION QUERY
--    Run this to test the fix (replace 1 with actual BillSummaryId)
-- ===================================================================

/*
DECLARE @TestBillId INT = 1;

-- Test 1: Get only PENDING samples
SELECT 'PENDING SAMPLES' as Test
EXEC usp_GetBillForSampleCollection @TestBillId, @OnlyPending = 1

-- Test 2: Get all samples including collected
SELECT 'ALL SAMPLES' as Test
EXEC usp_GetBillForSampleCollection @TestBillId, @OnlyPending = 0

-- Test 3: Get collection summary
SELECT 'COLLECTION SUMMARY' as Test
EXEC usp_GetCollectionSummaryForBill @TestBillId

-- Test 4: Get truly pending samples
SELECT 'TRULY PENDING' as Test
EXEC usp_GetPendingSamplesForBill @TestBillId
*/

PRINT 'SQL Fix applied successfully!'
PRINT 'Updated Procedures:'
PRINT '  - usp_GetBillForSampleCollection (UPDATED)'
PRINT '  - usp_GetCollectionSummaryForBill (NEW)'
PRINT '  - usp_GetPendingSamplesForBill (NEW)'
PRINT 'Indices: Created for optimal performance'
