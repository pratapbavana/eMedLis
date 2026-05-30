USE [eMedLis]
GO

IF OBJECT_ID('[dbo].[ReportResultHeader]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ReportResultHeader](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SampleDetailId] [int] NOT NULL,
        [ResultStatus] [varchar](30) NOT NULL,
        [SavedOn] [datetime] NULL,
        [SubmittedOn] [datetime] NULL,
        [AuthorizedOn] [datetime] NULL,
        CONSTRAINT [PK_ReportResultHeader] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_ReportResultHeader_SampleDetailId] UNIQUE ([SampleDetailId])
    ) ON [PRIMARY]
END
GO

IF OBJECT_ID('[dbo].[ReportResultDetail]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ReportResultDetail](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ResultHeaderId] [int] NOT NULL,
        [ParameterId] [int] NOT NULL,
        [MethodId] [int] NULL,
        [ResultValue] [nvarchar](200) NULL,
        [ResultType] [varchar](30) NULL,
        [Unit] [varchar](50) NULL,
        [NormalMin] [decimal](18,4) NULL,
        [NormalMax] [decimal](18,4) NULL,
        [CriticalMin] [decimal](18,4) NULL,
        [CriticalMax] [decimal](18,4) NULL,
        [RangeText] [nvarchar](500) NULL,
        [Flag] [varchar](2) NULL,
        [IsCritical] [bit] NOT NULL CONSTRAINT [DF_ReportResultDetail_IsCritical] DEFAULT ((0)),
        [DisplayOrder] [int] NOT NULL CONSTRAINT [DF_ReportResultDetail_DisplayOrder] DEFAULT ((0)),
        CONSTRAINT [PK_ReportResultDetail] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) ON [PRIMARY]
END
GO

IF COL_LENGTH('dbo.ReportResultDetail', 'RangeText') IS NULL
BEGIN
    ALTER TABLE [dbo].[ReportResultDetail] ADD [RangeText] NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ReportResultDetail_Header')
BEGIN
    ALTER TABLE [dbo].[ReportResultDetail] WITH CHECK ADD CONSTRAINT [FK_ReportResultDetail_Header]
    FOREIGN KEY([ResultHeaderId]) REFERENCES [dbo].[ReportResultHeader]([Id])
    ALTER TABLE [dbo].[ReportResultDetail] CHECK CONSTRAINT [FK_ReportResultDetail_Header]
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ReportResultDetail_Parameter')
BEGIN
    ALTER TABLE [dbo].[ReportResultDetail] WITH CHECK ADD CONSTRAINT [FK_ReportResultDetail_Parameter]
    FOREIGN KEY([ParameterId]) REFERENCES [dbo].[ParameterMaster]([Id])
    ALTER TABLE [dbo].[ReportResultDetail] CHECK CONSTRAINT [FK_ReportResultDetail_Parameter]
END
GO

IF OBJECT_ID('[dbo].[Usp_ReportResult]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[Usp_ReportResult]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Usp_ReportResult]
(
    @ResultHeaderId INT = NULL,
    @SampleDetailId INT = NULL,
    @ParameterId INT = NULL,
    @MethodId INT = NULL,
    @ResultValue NVARCHAR(200) = NULL,
    @ResultType VARCHAR(30) = NULL,
    @Unit VARCHAR(50) = NULL,
    @NormalMin DECIMAL(18,4) = NULL,
    @NormalMax DECIMAL(18,4) = NULL,
    @CriticalMin DECIMAL(18,4) = NULL,
    @CriticalMax DECIMAL(18,4) = NULL,
    @RangeText NVARCHAR(500) = NULL,
    @Flag VARCHAR(2) = NULL,
    @IsCritical BIT = NULL,
    @DisplayOrder INT = NULL,
    @ResultStatus VARCHAR(30) = NULL,
    @Action VARCHAR(40),
    @StatusCode INT = 0 OUTPUT,
    @StatusMsg VARCHAR(200) = NULL OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'GetHeader'
    BEGIN
        SELECT TOP 1 Id, SampleDetailId, ResultStatus, SavedOn, SubmittedOn, AuthorizedOn
        FROM ReportResultHeader
        WHERE SampleDetailId = @SampleDetailId;
    END

    IF @Action = 'UpsertHeader'
    BEGIN
        IF EXISTS (SELECT 1 FROM ReportResultHeader WHERE SampleDetailId = @SampleDetailId)
        BEGIN
            UPDATE ReportResultHeader
            SET ResultStatus = @ResultStatus,
                SavedOn = CASE WHEN @ResultStatus = 'Draft' THEN GETDATE() ELSE SavedOn END,
                SubmittedOn = CASE WHEN @ResultStatus = 'Pending Authorization' THEN GETDATE() ELSE SubmittedOn END
            WHERE SampleDetailId = @SampleDetailId;

            SELECT @ResultHeaderId = Id FROM ReportResultHeader WHERE SampleDetailId = @SampleDetailId;
        END
        ELSE
        BEGIN
            INSERT INTO ReportResultHeader (SampleDetailId, ResultStatus, SavedOn, SubmittedOn, AuthorizedOn)
            VALUES (@SampleDetailId, @ResultStatus, CASE WHEN @ResultStatus = 'Draft' THEN GETDATE() ELSE NULL END,
                    CASE WHEN @ResultStatus = 'Pending Authorization' THEN GETDATE() ELSE NULL END, NULL);

            SET @ResultHeaderId = SCOPE_IDENTITY();
        END

        SELECT @StatusCode = 1, @StatusMsg = 'Header saved';
        SELECT @ResultHeaderId AS ResultHeaderId;
    END

    IF @Action = 'DeleteDetails'
    BEGIN
        DELETE FROM ReportResultDetail WHERE ResultHeaderId = @ResultHeaderId;
        SELECT @StatusCode = 1, @StatusMsg = 'Details cleared';
    END

    IF @Action = 'AddDetail'
    BEGIN
        INSERT INTO ReportResultDetail
        (
            ResultHeaderId, ParameterId, MethodId, ResultValue, ResultType, Unit,
            NormalMin, NormalMax, CriticalMin, CriticalMax, RangeText, Flag, IsCritical, DisplayOrder
        )
        VALUES
        (
            @ResultHeaderId, @ParameterId, @MethodId, @ResultValue, @ResultType, @Unit,
            @NormalMin, @NormalMax, @CriticalMin, @CriticalMax, @RangeText, @Flag, ISNULL(@IsCritical, 0), ISNULL(@DisplayOrder, 0)
        );

        SELECT @StatusCode = 1, @StatusMsg = 'Detail added';
    END

    IF @Action = 'GetDetailsBySampleDetail'
    BEGIN
        SELECT
            H.Id AS ResultHeaderId,
            H.SampleDetailId,
            H.ResultStatus,
            D.ParameterId,
            D.MethodId,
            D.ResultValue,
            D.ResultType,
            D.Unit,
            D.NormalMin,
            D.NormalMax,
            D.CriticalMin,
            D.CriticalMax,
            D.RangeText,
            D.Flag,
            D.IsCritical,
            D.DisplayOrder
        FROM ReportResultHeader H
        INNER JOIN ReportResultDetail D ON D.ResultHeaderId = H.Id
        WHERE H.SampleDetailId = @SampleDetailId
        ORDER BY D.DisplayOrder, D.ParameterId;
    END
END
GO
