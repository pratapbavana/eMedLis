USE [eMedLis]
GO

IF COL_LENGTH('dbo.ReportResultHeader', 'DoctorInterpretation') IS NULL
BEGIN
    ALTER TABLE [dbo].[ReportResultHeader] ADD [DoctorInterpretation] NVARCHAR(MAX) NULL;
END
GO

IF COL_LENGTH('dbo.ReportResultHeader', 'RejectedReason') IS NULL
BEGIN
    ALTER TABLE [dbo].[ReportResultHeader] ADD [RejectedReason] NVARCHAR(500) NULL;
END
GO

IF COL_LENGTH('dbo.ReportResultHeader', 'AuthorizedByDoctorId') IS NULL
BEGIN
    ALTER TABLE [dbo].[ReportResultHeader] ADD [AuthorizedByDoctorId] INT NULL;
END
GO

IF COL_LENGTH('dbo.ReportResultHeader', 'ReviewedByDoctorId') IS NULL
BEGIN
    ALTER TABLE [dbo].[ReportResultHeader] ADD [ReviewedByDoctorId] INT NULL;
END
GO

IF COL_LENGTH('dbo.ReportResultHeader', 'ReviewedOn') IS NULL
BEGIN
    ALTER TABLE [dbo].[ReportResultHeader] ADD [ReviewedOn] DATETIME NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ReportResultHeader_AuthorizedDoctor')
BEGIN
    ALTER TABLE [dbo].[ReportResultHeader] WITH CHECK ADD CONSTRAINT [FK_ReportResultHeader_AuthorizedDoctor]
    FOREIGN KEY([AuthorizedByDoctorId]) REFERENCES [dbo].[DoctorMaster]([Id]);
    ALTER TABLE [dbo].[ReportResultHeader] CHECK CONSTRAINT [FK_ReportResultHeader_AuthorizedDoctor];
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ReportResultHeader_ReviewedDoctor')
BEGIN
    ALTER TABLE [dbo].[ReportResultHeader] WITH CHECK ADD CONSTRAINT [FK_ReportResultHeader_ReviewedDoctor]
    FOREIGN KEY([ReviewedByDoctorId]) REFERENCES [dbo].[DoctorMaster]([Id]);
    ALTER TABLE [dbo].[ReportResultHeader] CHECK CONSTRAINT [FK_ReportResultHeader_ReviewedDoctor];
END
GO

IF OBJECT_ID('[dbo].[Usp_ReportAuthorization]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[Usp_ReportAuthorization]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Usp_ReportAuthorization]
(
    @UserName VARCHAR(100) = NULL,
    @SampleDetailId INT = NULL,
    @DateFrom DATE = NULL,
    @DateTo DATE = NULL,
    @PatientName VARCHAR(100) = NULL,
    @SampleBarcode VARCHAR(50) = NULL,
    @Investigation VARCHAR(100) = NULL,
    @CriticalOnly BIT = 0,
    @DoctorInterpretation NVARCHAR(MAX) = NULL,
    @RejectedReason NVARCHAR(500) = NULL,
    @Action VARCHAR(40),
    @StatusCode INT = 0 OUTPUT,
    @StatusMsg VARCHAR(300) = NULL OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DoctorId INT;
    SELECT TOP 1 @DoctorId = DM.Id
    FROM DoctorMaster DM
    INNER JOIN Users U ON U.UserId = DM.UserId
    WHERE U.Username = @UserName
      AND ISNULL(DM.Active, 0) = 1
      AND ISNULL(U.IsActive, 1) = 1
      AND ISNULL(U.IsLocked, 0) = 0;

    IF @DoctorId IS NULL
    BEGIN
        SELECT @StatusCode = 0, @StatusMsg = 'Doctor profile not configured for this user';
        IF @Action = 'GetReview'
            SELECT CAST(0 AS INT) AS Id WHERE 1 = 0;
        RETURN;
    END

    IF @Action = 'SearchPending'
    BEGIN
        SELECT
            SCD.SampleDetailId,
            SC.SampleCollectionId,
            SCD.SampleBarcode,
            PI.PatName,
            PI.Age,
            PI.AgeType,
            PI.Gender,
            SCD.InvestigationName,
            SD.SubDeptName AS DepartmentName,
            SC.CollectionDate,
            RRH.ResultStatus,
            CASE WHEN EXISTS (
                SELECT 1
                FROM ReportResultDetail RRD
                WHERE RRD.ResultHeaderId = RRH.Id
                  AND (ISNULL(RRD.IsCritical, 0) = 1 OR ISNULL(RRD.Flag, '') = 'C')
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS HasCritical
        FROM ReportResultHeader RRH
        INNER JOIN SampleCollectionDetail SCD ON SCD.SampleDetailId = RRH.SampleDetailId
        INNER JOIN SampleCollection SC ON SC.SampleCollectionId = SCD.SampleCollectionId
        INNER JOIN PatientInfo PI ON PI.PatientInfoId = SC.PatientInfoId
        INNER JOIN Investigations I ON I.Id = SCD.InvMasterId
        INNER JOIN SubDepartment SD ON SD.Id = I.SubDeptId
        INNER JOIN DoctorSubDepartment DSD ON DSD.SubDepartmentId = SD.Id AND DSD.DoctorId = @DoctorId
        WHERE RRH.ResultStatus = 'Pending Authorization'
          AND (@DateFrom IS NULL OR CAST(SC.CollectionDate AS DATE) >= @DateFrom)
          AND (@DateTo IS NULL OR CAST(SC.CollectionDate AS DATE) <= @DateTo)
          AND (ISNULL(@PatientName, '') = '' OR PI.PatName LIKE '%' + @PatientName + '%')
          AND (ISNULL(@SampleBarcode, '') = '' OR SCD.SampleBarcode LIKE '%' + @SampleBarcode + '%' OR SC.CollectionBarcode LIKE '%' + @SampleBarcode + '%')
          AND (ISNULL(@Investigation, '') = '' OR SCD.InvestigationName LIKE '%' + @Investigation + '%')
          AND (
                ISNULL(@CriticalOnly, 0) = 0
                OR EXISTS (
                    SELECT 1
                    FROM ReportResultDetail RRD
                    WHERE RRD.ResultHeaderId = RRH.Id
                      AND (ISNULL(RRD.IsCritical, 0) = 1 OR ISNULL(RRD.Flag, '') = 'C')
                )
          )
        ORDER BY SC.CollectionDate DESC, SCD.SampleDetailId DESC;
        RETURN;
    END

    IF @Action = 'GetReview'
    BEGIN
        IF NOT EXISTS (
            SELECT 1
            FROM ReportResultHeader RRH
            INNER JOIN SampleCollectionDetail SCD ON SCD.SampleDetailId = RRH.SampleDetailId
            INNER JOIN Investigations I ON I.Id = SCD.InvMasterId
            INNER JOIN DoctorSubDepartment DSD ON DSD.SubDepartmentId = I.SubDeptId AND DSD.DoctorId = @DoctorId
            WHERE RRH.SampleDetailId = @SampleDetailId
        )
        BEGIN
            SELECT CAST(0 AS INT) AS Id WHERE 1 = 0;
            RETURN;
        END

        SELECT TOP 1
            RRH.ResultStatus,
            RRH.DoctorInterpretation,
            RRH.RejectedReason,
            RRH.AuthorizedOn,
            DAuth.Id AS DoctorId,
            CASE WHEN DAuth.SignatureImage IS NULL THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END AS HasSignature,
            LTRIM(RTRIM(ISNULL(UAuth.FirstName, '') + ' ' + ISNULL(UAuth.LastName, ''))) AS AuthorizedDoctor,
            CASE WHEN RRH.ResultStatus = 'Pending Authorization' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS CanAuthorize,
            BS.BillNo,
            PI.PatName,
            PI.Age,
            PI.AgeType,
            PI.Gender,
            SCD.SampleBarcode,
            SCD.InvestigationName,
            SD.SubDeptName AS DepartmentName
        FROM ReportResultHeader RRH
        INNER JOIN SampleCollectionDetail SCD ON SCD.SampleDetailId = RRH.SampleDetailId
        INNER JOIN SampleCollection SC ON SC.SampleCollectionId = SCD.SampleCollectionId
        INNER JOIN BillSummary BS ON BS.BillSummaryId = SC.BillSummaryId
        INNER JOIN PatientInfo PI ON PI.PatientInfoId = SC.PatientInfoId
        INNER JOIN Investigations I ON I.Id = SCD.InvMasterId
        INNER JOIN SubDepartment SD ON SD.Id = I.SubDeptId
        LEFT JOIN DoctorMaster DAuth ON DAuth.Id = RRH.AuthorizedByDoctorId
        LEFT JOIN Users UAuth ON UAuth.UserId = DAuth.UserId
        WHERE RRH.SampleDetailId = @SampleDetailId;

        SELECT
            RRD.ParameterId,
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
            ISNULL(RRD.DisplayOrder, 0) AS DisplayOrder
        FROM ReportResultHeader RRH
        INNER JOIN ReportResultDetail RRD ON RRD.ResultHeaderId = RRH.Id
        INNER JOIN ParameterMaster PM ON PM.Id = RRD.ParameterId
        LEFT JOIN TestMethod TM ON TM.Id = RRD.MethodId
        LEFT JOIN InvestigationTemplate IT ON IT.InvestigationId = CAST((SELECT TOP 1 InvMasterId FROM SampleCollectionDetail WHERE SampleDetailId = RRH.SampleDetailId) AS VARCHAR(50))
                                          AND IT.ItemType = 'Parameter'
                                          AND IT.ParameterId = CAST(RRD.ParameterId AS VARCHAR(50))
                                          AND ISNULL(IT.Active, 1) = 1
        LEFT JOIN ParameterHeader PH ON PH.Id = TRY_CAST(IT.HeaderId AS INT)
        WHERE RRH.SampleDetailId = @SampleDetailId
        ORDER BY ISNULL(RRD.DisplayOrder, 0), RRD.ParameterId;
        RETURN;
    END

    IF @Action = 'SaveReview'
    BEGIN
        IF NOT EXISTS (
            SELECT 1
            FROM ReportResultHeader RRH
            INNER JOIN SampleCollectionDetail SCD ON SCD.SampleDetailId = RRH.SampleDetailId
            INNER JOIN Investigations I ON I.Id = SCD.InvMasterId
            INNER JOIN DoctorSubDepartment DSD ON DSD.SubDepartmentId = I.SubDeptId AND DSD.DoctorId = @DoctorId
            WHERE RRH.SampleDetailId = @SampleDetailId
              AND RRH.ResultStatus = 'Pending Authorization'
        )
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = 'Only pending reports can be updated';
            RETURN;
        END

        UPDATE ReportResultHeader
        SET DoctorInterpretation = @DoctorInterpretation,
            ReviewedByDoctorId = @DoctorId,
            ReviewedOn = GETDATE()
        WHERE SampleDetailId = @SampleDetailId;

        SELECT @StatusCode = 1, @StatusMsg = 'Review saved';
        RETURN;
    END

    IF @Action = 'Authorize'
    BEGIN
        IF NOT EXISTS (
            SELECT 1
            FROM ReportResultHeader RRH
            INNER JOIN SampleCollectionDetail SCD ON SCD.SampleDetailId = RRH.SampleDetailId
            INNER JOIN Investigations I ON I.Id = SCD.InvMasterId
            INNER JOIN DoctorSubDepartment DSD ON DSD.SubDepartmentId = I.SubDeptId AND DSD.DoctorId = @DoctorId
            WHERE RRH.SampleDetailId = @SampleDetailId
              AND RRH.ResultStatus = 'Pending Authorization'
        )
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = 'Only pending reports can be authorized';
            RETURN;
        END

        UPDATE ReportResultHeader
        SET ResultStatus = 'Authorized',
            DoctorInterpretation = @DoctorInterpretation,
            RejectedReason = NULL,
            AuthorizedByDoctorId = @DoctorId,
            AuthorizedOn = GETDATE(),
            ReviewedByDoctorId = @DoctorId,
            ReviewedOn = GETDATE()
        WHERE SampleDetailId = @SampleDetailId;

        SELECT @StatusCode = 1, @StatusMsg = 'Report authorized successfully';
        RETURN;
    END

    IF @Action = 'Reject'
    BEGIN
        IF ISNULL(LTRIM(RTRIM(@RejectedReason)), '') = ''
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = 'Reject reason is required';
            RETURN;
        END

        IF NOT EXISTS (
            SELECT 1
            FROM ReportResultHeader RRH
            INNER JOIN SampleCollectionDetail SCD ON SCD.SampleDetailId = RRH.SampleDetailId
            INNER JOIN Investigations I ON I.Id = SCD.InvMasterId
            INNER JOIN DoctorSubDepartment DSD ON DSD.SubDepartmentId = I.SubDeptId AND DSD.DoctorId = @DoctorId
            WHERE RRH.SampleDetailId = @SampleDetailId
              AND RRH.ResultStatus = 'Pending Authorization'
        )
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = 'Only pending reports can be rejected';
            RETURN;
        END

        UPDATE ReportResultHeader
        SET ResultStatus = 'Rejected',
            DoctorInterpretation = @DoctorInterpretation,
            RejectedReason = @RejectedReason,
            ReviewedByDoctorId = @DoctorId,
            ReviewedOn = GETDATE()
        WHERE SampleDetailId = @SampleDetailId;

        SELECT @StatusCode = 1, @StatusMsg = 'Report rejected and sent for correction';
        RETURN;
    END
END
GO
