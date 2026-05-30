USE [eMedLis]
GO

IF OBJECT_ID('[dbo].[Usp_ReportEntry]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[Usp_ReportEntry]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Usp_ReportEntry]
(
    @SampleCollectionId INT = NULL,
    @SampleDetailId INT = NULL,
    @BillNo VARCHAR(30) = NULL,
    @SampleBarcode VARCHAR(50) = NULL,
    @PatientName VARCHAR(100) = NULL,
    @MobileNo VARCHAR(20) = NULL,
    @DateFrom DATE = NULL,
    @DateTo DATE = NULL,
    @Action VARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'SearchOrders'
    BEGIN
        SELECT
            SC.SampleCollectionId,
            SC.CollectionBarcode,
            SC.CollectionDate,
            BS.BillSummaryId,
            BS.BillNo,
            BS.BillDate,
            PI.PatientInfoId,
            PI.PatName,
            PI.Age,
            PI.AgeType,
            PI.Gender,
            PI.MobileNo,
            PI.UHID,
            COUNT(DISTINCT SCD.InvMasterId) AS InvestigationCount
        FROM SampleCollection SC
        INNER JOIN BillSummary BS ON BS.BillSummaryId = SC.BillSummaryId
        INNER JOIN PatientInfo PI ON PI.PatientInfoId = SC.PatientInfoId
        INNER JOIN SampleCollectionDetail SCD ON SCD.SampleCollectionId = SC.SampleCollectionId
        WHERE SCD.SampleStatus = 'Collected'
          AND (ISNULL(@BillNo, '') = '' OR BS.BillNo LIKE @BillNo + '%')
          AND (ISNULL(@SampleBarcode, '') = '' OR SCD.SampleBarcode = @SampleBarcode OR SC.CollectionBarcode = @SampleBarcode)
          AND (ISNULL(@PatientName, '') = '' OR PI.PatName LIKE '%' + @PatientName + '%')
          AND (ISNULL(@MobileNo, '') = '' OR PI.MobileNo LIKE '%' + @MobileNo + '%')
          AND (@DateFrom IS NULL OR CAST(SC.CollectionDate AS DATE) >= @DateFrom)
          AND (@DateTo IS NULL OR CAST(SC.CollectionDate AS DATE) <= @DateTo)
        GROUP BY
            SC.SampleCollectionId,
            SC.CollectionBarcode,
            SC.CollectionDate,
            BS.BillSummaryId,
            BS.BillNo,
            BS.BillDate,
            PI.PatientInfoId,
            PI.PatName,
            PI.Age,
            PI.AgeType,
            PI.Gender,
            PI.MobileNo,
            PI.UHID
        ORDER BY SC.CollectionDate DESC, SC.SampleCollectionId DESC;
    END

    IF @Action = 'GetInvestigations'
    BEGIN
        SELECT
            SCD.SampleDetailId,
            SCD.InvMasterId,
            SCD.InvestigationName,
            SCD.SampleBarcode,
            SCD.SpecimenType,
            CASE
                WHEN EXISTS (
                    SELECT 1
                    FROM InvestigationTemplate IT
                    WHERE IT.InvestigationId = CAST(SCD.InvMasterId AS VARCHAR(50))
                      AND IT.ItemType = 'Parameter'
                      AND ISNULL(IT.Active, 1) = 1
                ) THEN CAST(1 AS BIT)
                ELSE CAST(0 AS BIT)
            END AS HasTemplate
        FROM SampleCollectionDetail SCD
        WHERE SCD.SampleCollectionId = @SampleCollectionId
          AND SCD.SampleStatus = 'Collected'
        ORDER BY SCD.SampleDetailId;
    END

    IF @Action = 'GetOrderSummary'
    BEGIN
        SELECT TOP 1
            SC.SampleCollectionId,
            SC.CollectionBarcode,
            SC.CollectionDate,
            BS.BillSummaryId,
            BS.BillNo,
            BS.BillDate,
            PI.PatientInfoId,
            PI.PatName,
            PI.Age,
            PI.AgeType,
            PI.Gender,
            PI.MobileNo,
            PI.UHID,
            (
                SELECT COUNT(1)
                FROM SampleCollectionDetail X
                WHERE X.SampleCollectionId = SC.SampleCollectionId
                  AND X.SampleStatus = 'Collected'
            ) AS InvestigationCount
        FROM SampleCollection SC
        INNER JOIN BillSummary BS ON BS.BillSummaryId = SC.BillSummaryId
        INNER JOIN PatientInfo PI ON PI.PatientInfoId = SC.PatientInfoId
        WHERE SC.SampleCollectionId = @SampleCollectionId;
    END

    IF @Action = 'GetPatientContext'
    BEGIN
        SELECT TOP 1
            SCD.SampleDetailId,
            SCD.InvMasterId,
            SCD.InvestigationName,
            SCD.SampleBarcode,
            BS.BillNo,
            PI.PatName,
            PI.Age,
            PI.AgeType,
            PI.Gender
        FROM SampleCollectionDetail SCD
        INNER JOIN SampleCollection SC ON SC.SampleCollectionId = SCD.SampleCollectionId
        INNER JOIN BillSummary BS ON BS.BillSummaryId = SC.BillSummaryId
        INNER JOIN PatientInfo PI ON PI.PatientInfoId = SC.PatientInfoId
        WHERE SCD.SampleDetailId = @SampleDetailId;
    END
END
GO
