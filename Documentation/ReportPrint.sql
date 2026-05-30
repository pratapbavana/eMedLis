USE [eMedLis]
GO

IF OBJECT_ID('[dbo].[Usp_ReportPrint]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[Usp_ReportPrint]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Usp_ReportPrint]
(
    @BillSummaryId INT = NULL,
    @PatientName VARCHAR(100) = NULL,
    @MobileNo VARCHAR(20) = NULL,
    @SampleBarcode VARCHAR(50) = NULL,
    @BillNo VARCHAR(30) = NULL,
    @DateFrom DATE = NULL,
    @DateTo DATE = NULL,
    @SubDepartment VARCHAR(100) = NULL,
    @SampleDetailIds VARCHAR(MAX) = NULL,
    @Action VARCHAR(30)
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'SearchBills'
    BEGIN
        SELECT
            BS.BillSummaryId,
            SC.SampleCollectionId,
            BS.BillNo,
            SC.CollectionBarcode,
            PI.PatName,
            PI.Age,
            PI.AgeType,
            PI.Gender,
            PI.MobileNo,
            SC.CollectionDate,
            COUNT(1) AS InvestigationCount
        FROM ReportResultHeader RRH
        INNER JOIN SampleCollectionDetail SCD ON SCD.SampleDetailId = RRH.SampleDetailId
        INNER JOIN SampleCollection SC ON SC.SampleCollectionId = SCD.SampleCollectionId
        INNER JOIN BillSummary BS ON BS.BillSummaryId = SC.BillSummaryId
        INNER JOIN PatientInfo PI ON PI.PatientInfoId = SC.PatientInfoId
        INNER JOIN Investigations I ON I.Id = SCD.InvMasterId
        INNER JOIN SubDepartment SD ON SD.Id = I.SubDeptId
        WHERE RRH.ResultStatus = 'Authorized'
          AND (ISNULL(@PatientName, '') = '' OR PI.PatName LIKE '%' + @PatientName + '%')
          AND (ISNULL(@MobileNo, '') = '' OR PI.MobileNo LIKE '%' + @MobileNo + '%')
          AND (ISNULL(@SampleBarcode, '') = '' OR SCD.SampleBarcode LIKE '%' + @SampleBarcode + '%' OR SC.CollectionBarcode LIKE '%' + @SampleBarcode + '%')
          AND (ISNULL(@BillNo, '') = '' OR BS.BillNo LIKE '%' + @BillNo + '%')
          AND (@DateFrom IS NULL OR CAST(SC.CollectionDate AS DATE) >= @DateFrom)
          AND (@DateTo IS NULL OR CAST(SC.CollectionDate AS DATE) <= @DateTo)
          AND (ISNULL(@SubDepartment, '') = '' OR SD.SubDeptName LIKE '%' + @SubDepartment + '%')
        GROUP BY BS.BillSummaryId, SC.SampleCollectionId, BS.BillNo, SC.CollectionBarcode, PI.PatName, PI.Age, PI.AgeType, PI.Gender, PI.MobileNo, SC.CollectionDate
        ORDER BY SC.CollectionDate DESC, BS.BillSummaryId DESC;
        RETURN;
    END

    IF @Action = 'GetBillInvestigations'
    BEGIN
        SELECT
            SCD.SampleDetailId,
            SCD.SampleCollectionId,
            BS.BillSummaryId,
            SCD.SampleBarcode,
            BS.BillNo,
            PI.PatName,
            PI.Age,
            PI.AgeType,
            PI.Gender,
            PI.MobileNo,
            SCD.InvestigationName,
            SD.SubDeptName AS DepartmentName,
            SC.CollectionDate,
            RRH.ResultStatus
        FROM ReportResultHeader RRH
        INNER JOIN SampleCollectionDetail SCD ON SCD.SampleDetailId = RRH.SampleDetailId
        INNER JOIN SampleCollection SC ON SC.SampleCollectionId = SCD.SampleCollectionId
        INNER JOIN BillSummary BS ON BS.BillSummaryId = SC.BillSummaryId
        INNER JOIN PatientInfo PI ON PI.PatientInfoId = SC.PatientInfoId
        INNER JOIN Investigations I ON I.Id = SCD.InvMasterId
        INNER JOIN SubDepartment SD ON SD.Id = I.SubDeptId
        WHERE RRH.ResultStatus = 'Authorized'
          AND BS.BillSummaryId = @BillSummaryId
        ORDER BY SC.CollectionDate DESC, SCD.SampleDetailId DESC;
        RETURN;
    END

    IF @Action = 'GetByIds'
    BEGIN
        DECLARE @Ids TABLE (SampleDetailId INT PRIMARY KEY);

        IF ISNULL(@SampleDetailIds, '') <> ''
        BEGIN
            DECLARE @XmlIds XML;
            SET @XmlIds = CAST('<x><i>' + REPLACE(REPLACE(@SampleDetailIds, '&', '&amp;'), ',', '</i><i>') + '</i></x>' AS XML);

            INSERT INTO @Ids (SampleDetailId)
            SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(T.C.value('.', 'varchar(20)'))) AS INT)
            FROM @XmlIds.nodes('/x/i') AS T(C)
            WHERE TRY_CAST(LTRIM(RTRIM(T.C.value('.', 'varchar(20)'))) AS INT) IS NOT NULL;
        END

        SELECT
            SCD.SampleDetailId,
            SCD.SampleCollectionId,
            BS.BillSummaryId,
            SCD.SampleBarcode,
            BS.BillNo,
            PI.PatName,
            PI.Age,
            PI.AgeType,
            PI.Gender,
            PI.MobileNo,
            SCD.InvestigationName,
            SD.SubDeptName AS DepartmentName,
            SC.CollectionDate,
            RRH.ResultStatus
        FROM @Ids X
        INNER JOIN ReportResultHeader RRH ON RRH.SampleDetailId = X.SampleDetailId AND RRH.ResultStatus = 'Authorized'
        INNER JOIN SampleCollectionDetail SCD ON SCD.SampleDetailId = RRH.SampleDetailId
        INNER JOIN SampleCollection SC ON SC.SampleCollectionId = SCD.SampleCollectionId
        INNER JOIN BillSummary BS ON BS.BillSummaryId = SC.BillSummaryId
        INNER JOIN PatientInfo PI ON PI.PatientInfoId = SC.PatientInfoId
        INNER JOIN Investigations I ON I.Id = SCD.InvMasterId
        INNER JOIN SubDepartment SD ON SD.Id = I.SubDeptId
        WHERE BS.BillSummaryId = @BillSummaryId
        ORDER BY SD.SubDeptName, SCD.InvestigationName, SCD.SampleDetailId;
        RETURN;
    END

    IF @Action = 'GetPreviewData'
    BEGIN
        DECLARE @IdsPreview TABLE (SampleDetailId INT PRIMARY KEY);

        IF ISNULL(@SampleDetailIds, '') <> ''
        BEGIN
            DECLARE @XmlIdsPreview XML;
            SET @XmlIdsPreview = CAST('<x><i>' + REPLACE(REPLACE(@SampleDetailIds, '&', '&amp;'), ',', '</i><i>') + '</i></x>' AS XML);

            INSERT INTO @IdsPreview (SampleDetailId)
            SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(T.C.value('.', 'varchar(20)'))) AS INT)
            FROM @XmlIdsPreview.nodes('/x/i') AS T(C)
            WHERE TRY_CAST(LTRIM(RTRIM(T.C.value('.', 'varchar(20)'))) AS INT) IS NOT NULL;
        END

        SELECT
            SCD.SampleDetailId,
            BS.BillSummaryId,
            BS.BillNo,
            BS.BillDate,
            SCD.SampleBarcode,
            PI.PatName,
            PI.Age,
            PI.AgeType,
            PI.Gender,
            PI.MobileNo,
            PI.Ref AS ReferralDoctor,
            SCD.InvestigationName,
            SD.SubDeptName AS DepartmentName,
            SC.CollectionDate,
            RRH.ResultStatus,
            RRH.DoctorInterpretation,
            RRH.AuthorizedOn,
            RRH.AuthorizedByDoctorId,
            CASE WHEN DM.SignatureImage IS NULL THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END AS HasSignature,
            LTRIM(RTRIM(ISNULL(U.FirstName, '') + ' ' + ISNULL(U.LastName, ''))) AS AuthorizedDoctor
        FROM @IdsPreview X
        INNER JOIN ReportResultHeader RRH ON RRH.SampleDetailId = X.SampleDetailId AND RRH.ResultStatus = 'Authorized'
        INNER JOIN SampleCollectionDetail SCD ON SCD.SampleDetailId = RRH.SampleDetailId
        INNER JOIN SampleCollection SC ON SC.SampleCollectionId = SCD.SampleCollectionId
        INNER JOIN BillSummary BS ON BS.BillSummaryId = SC.BillSummaryId
        INNER JOIN PatientInfo PI ON PI.PatientInfoId = SC.PatientInfoId
        INNER JOIN Investigations I ON I.Id = SCD.InvMasterId
        INNER JOIN SubDepartment SD ON SD.Id = I.SubDeptId
        LEFT JOIN DoctorMaster DM ON DM.Id = RRH.AuthorizedByDoctorId
        LEFT JOIN Users U ON U.UserId = DM.UserId
        WHERE BS.BillSummaryId = @BillSummaryId
        ORDER BY SD.SubDeptName, SCD.InvestigationName, SCD.SampleDetailId;

        SELECT
            RRH.SampleDetailId,
            ISNULL(PH.HeaderName, '') AS HeaderName,
            PM.ParameterName,
            ISNULL(TM.MethodName, CASE WHEN RRD.MethodId IS NULL THEN 'No Method' ELSE '' END) AS MethodName,
            RRD.ResultValue,
            RRD.Unit,
            CASE
                WHEN ISNULL(LTRIM(RTRIM(RRD.RangeText)), '') <> '' THEN RRD.RangeText
                WHEN RRD.NormalMin IS NULL AND RRD.NormalMax IS NULL THEN '-'
                ELSE CONVERT(VARCHAR(30), RRD.NormalMin) + ' - ' + CONVERT(VARCHAR(30), RRD.NormalMax)
            END AS NormalRange,
            ISNULL(RRD.Flag, '') AS Flag,
            ISNULL(RRD.IsCritical, 0) AS IsCritical,
            ISNULL(TRY_CAST(IT.DisplayOrder AS INT), ISNULL(RRD.DisplayOrder, 0)) AS DisplayOrder
        FROM @IdsPreview X
        INNER JOIN ReportResultHeader RRH ON RRH.SampleDetailId = X.SampleDetailId AND RRH.ResultStatus = 'Authorized'
        INNER JOIN SampleCollectionDetail SCD ON SCD.SampleDetailId = RRH.SampleDetailId
        INNER JOIN ReportResultDetail RRD ON RRD.ResultHeaderId = RRH.Id
        INNER JOIN ParameterMaster PM ON PM.Id = RRD.ParameterId
        LEFT JOIN TestMethod TM ON TM.Id = RRD.MethodId
        LEFT JOIN InvestigationTemplate IT ON IT.InvestigationId = CAST(SCD.InvMasterId AS VARCHAR(50))
                                          AND IT.ItemType = 'Parameter'
                                          AND IT.ParameterId = CAST(RRD.ParameterId AS VARCHAR(50))
                                          AND ISNULL(IT.Active, 1) = 1
        LEFT JOIN ParameterHeader PH ON PH.Id = TRY_CAST(IT.HeaderId AS INT)
        INNER JOIN SampleCollection SC ON SC.SampleCollectionId = SCD.SampleCollectionId
        INNER JOIN BillSummary BS ON BS.BillSummaryId = SC.BillSummaryId
        WHERE BS.BillSummaryId = @BillSummaryId
        ORDER BY RRH.SampleDetailId, ISNULL(TRY_CAST(IT.DisplayOrder AS INT), ISNULL(RRD.DisplayOrder, 0)), RRD.ParameterId;
        RETURN;
    END
END
GO
