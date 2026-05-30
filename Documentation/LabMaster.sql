USE [eMedLis]
GO

IF OBJECT_ID('[dbo].[LabMaster]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[LabMaster]
    (
        [Id] INT NOT NULL PRIMARY KEY,
        [LabName] NVARCHAR(200) NOT NULL,
        [ShortName] NVARCHAR(100) NULL,
        [Tagline] NVARCHAR(200) NULL,
        [AddressLine1] NVARCHAR(200) NULL,
        [AddressLine2] NVARCHAR(200) NULL,
        [City] NVARCHAR(80) NULL,
        [State] NVARCHAR(80) NULL,
        [Pincode] NVARCHAR(20) NULL,
        [Country] NVARCHAR(80) NULL,
        [MobileNumber] NVARCHAR(20) NULL,
        [AlternateMobile] NVARCHAR(20) NULL,
        [Landline] NVARCHAR(20) NULL,
        [Email] NVARCHAR(150) NULL,
        [Website] NVARCHAR(150) NULL,
        [GSTNumber] NVARCHAR(30) NULL,
        [PANNumber] NVARCHAR(20) NULL,
        [LabRegistrationNumber] NVARCHAR(50) NULL,
        [NABLNumber] NVARCHAR(50) NULL,
        [DrugLicenseNumber] NVARCHAR(50) NULL,
        [ShowLogoInReport] BIT NOT NULL CONSTRAINT [DF_LabMaster_ShowLogoInReport] DEFAULT (1),
        [ShowGSTInReport] BIT NOT NULL CONSTRAINT [DF_LabMaster_ShowGSTInReport] DEFAULT (1),
        [ShowAccreditationInReport] BIT NOT NULL CONSTRAINT [DF_LabMaster_ShowAccreditationInReport] DEFAULT (1),
        [ReceiptFooter] NVARCHAR(500) NULL,
        [BranchName] NVARCHAR(100) NULL,
        [BranchCode] NVARCHAR(30) NULL,
        [Logo] VARBINARY(MAX) NULL,
        [LogoMimeType] VARCHAR(50) NULL,
        [ReportHeaderImage] VARBINARY(MAX) NULL,
        [ReportHeaderMimeType] VARCHAR(50) NULL,
        [ReportFooterImage] VARBINARY(MAX) NULL,
        [ReportFooterMimeType] VARCHAR(50) NULL,
        [Active] BIT NOT NULL CONSTRAINT [DF_LabMaster_Active] DEFAULT (1),
        [UpdatedBy] NVARCHAR(100) NULL,
        [UpdatedOn] DATETIME NOT NULL CONSTRAINT [DF_LabMaster_UpdatedOn] DEFAULT (GETDATE())
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[LabMaster] WHERE [Id] = 1)
BEGIN
    INSERT INTO [dbo].[LabMaster]
    (
        [Id], [LabName], [ShowLogoInReport], [ShowGSTInReport], [ShowAccreditationInReport],
        [ReceiptFooter], [Active], [UpdatedBy], [UpdatedOn]
    )
    VALUES
    (
        1, N'SSK Diagnostics', 1, 1, 1,
        N'Thank you for choosing our laboratory.', 1, N'system', GETDATE()
    );
END
GO

IF OBJECT_ID('[dbo].[Usp_LabMaster]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[Usp_LabMaster]
GO

CREATE PROCEDURE [dbo].[Usp_LabMaster]
(
    @LabName NVARCHAR(200) = NULL,
    @ShortName NVARCHAR(100) = NULL,
    @Tagline NVARCHAR(200) = NULL,
    @AddressLine1 NVARCHAR(200) = NULL,
    @AddressLine2 NVARCHAR(200) = NULL,
    @City NVARCHAR(80) = NULL,
    @State NVARCHAR(80) = NULL,
    @Pincode NVARCHAR(20) = NULL,
    @Country NVARCHAR(80) = NULL,
    @MobileNumber NVARCHAR(20) = NULL,
    @AlternateMobile NVARCHAR(20) = NULL,
    @Landline NVARCHAR(20) = NULL,
    @Email NVARCHAR(150) = NULL,
    @Website NVARCHAR(150) = NULL,
    @GSTNumber NVARCHAR(30) = NULL,
    @PANNumber NVARCHAR(20) = NULL,
    @LabRegistrationNumber NVARCHAR(50) = NULL,
    @NABLNumber NVARCHAR(50) = NULL,
    @DrugLicenseNumber NVARCHAR(50) = NULL,
    @ShowLogoInReport BIT = NULL,
    @ShowGSTInReport BIT = NULL,
    @ShowAccreditationInReport BIT = NULL,
    @ReceiptFooter NVARCHAR(500) = NULL,
    @BranchName NVARCHAR(100) = NULL,
    @BranchCode NVARCHAR(30) = NULL,
    @Logo VARBINARY(MAX) = NULL,
    @LogoMimeType VARCHAR(50) = NULL,
    @ReportHeaderImage VARBINARY(MAX) = NULL,
    @ReportHeaderMimeType VARCHAR(50) = NULL,
    @ReportFooterImage VARBINARY(MAX) = NULL,
    @ReportFooterMimeType VARCHAR(50) = NULL,
    @Active BIT = NULL,
    @UpdatedBy NVARCHAR(100) = NULL,
    @ImageType VARCHAR(20) = NULL,
    @Action VARCHAR(30),
    @StatusCode INT = 0 OUTPUT,
    @StatusMsg VARCHAR(200) = NULL OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'Get'
    BEGIN
        SELECT TOP 1
            [Id], [LabName], [ShortName], [Tagline], [AddressLine1], [AddressLine2], [City], [State], [Pincode], [Country],
            [MobileNumber], [AlternateMobile], [Landline], [Email], [Website],
            [GSTNumber], [PANNumber], [LabRegistrationNumber], [NABLNumber], [DrugLicenseNumber],
            [ShowLogoInReport], [ShowGSTInReport], [ShowAccreditationInReport], [ReceiptFooter], [BranchName], [BranchCode],
            [Active],
            CASE WHEN [Logo] IS NULL THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END AS [HasLogo],
            CASE WHEN [ReportHeaderImage] IS NULL THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END AS [HasReportHeaderImage],
            CASE WHEN [ReportFooterImage] IS NULL THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END AS [HasReportFooterImage]
        FROM [dbo].[LabMaster]
        WHERE [Id] = 1;

        SELECT @StatusCode = 1, @StatusMsg = 'Loaded';
        RETURN;
    END

    IF @Action = 'Save'
    BEGIN
        IF ISNULL(LTRIM(RTRIM(@LabName)), '') = ''
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = 'Lab Name is required';
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM [dbo].[LabMaster] WHERE [Id] = 1)
        BEGIN
            INSERT INTO [dbo].[LabMaster] ([Id], [LabName], [Active], [UpdatedOn])
            VALUES (1, @LabName, 1, GETDATE());
        END

        UPDATE [dbo].[LabMaster]
        SET
            [LabName] = @LabName,
            [ShortName] = @ShortName,
            [Tagline] = @Tagline,
            [AddressLine1] = @AddressLine1,
            [AddressLine2] = @AddressLine2,
            [City] = @City,
            [State] = @State,
            [Pincode] = @Pincode,
            [Country] = @Country,
            [MobileNumber] = @MobileNumber,
            [AlternateMobile] = @AlternateMobile,
            [Landline] = @Landline,
            [Email] = @Email,
            [Website] = @Website,
            [GSTNumber] = @GSTNumber,
            [PANNumber] = @PANNumber,
            [LabRegistrationNumber] = @LabRegistrationNumber,
            [NABLNumber] = @NABLNumber,
            [DrugLicenseNumber] = @DrugLicenseNumber,
            [ShowLogoInReport] = ISNULL(@ShowLogoInReport, 1),
            [ShowGSTInReport] = ISNULL(@ShowGSTInReport, 1),
            [ShowAccreditationInReport] = ISNULL(@ShowAccreditationInReport, 1),
            [ReceiptFooter] = @ReceiptFooter,
            [BranchName] = @BranchName,
            [BranchCode] = @BranchCode,
            [Logo] = CASE WHEN @Logo IS NULL THEN [Logo] ELSE @Logo END,
            [LogoMimeType] = CASE WHEN @Logo IS NULL THEN [LogoMimeType] ELSE @LogoMimeType END,
            [ReportHeaderImage] = CASE WHEN @ReportHeaderImage IS NULL THEN [ReportHeaderImage] ELSE @ReportHeaderImage END,
            [ReportHeaderMimeType] = CASE WHEN @ReportHeaderImage IS NULL THEN [ReportHeaderMimeType] ELSE @ReportHeaderMimeType END,
            [ReportFooterImage] = CASE WHEN @ReportFooterImage IS NULL THEN [ReportFooterImage] ELSE @ReportFooterImage END,
            [ReportFooterMimeType] = CASE WHEN @ReportFooterImage IS NULL THEN [ReportFooterMimeType] ELSE @ReportFooterMimeType END,
            [Active] = ISNULL(@Active, [Active]),
            [UpdatedBy] = @UpdatedBy,
            [UpdatedOn] = GETDATE()
        WHERE [Id] = 1;

        SELECT @StatusCode = 1, @StatusMsg = 'Lab profile saved successfully';
        RETURN;
    END

    IF @Action = 'GetImage'
    BEGIN
        IF @ImageType = 'Header'
        BEGIN
            SELECT TOP 1 [ReportHeaderImage] AS ImageData, [ReportHeaderMimeType] AS ImageMimeType
            FROM [dbo].[LabMaster]
            WHERE [Id] = 1;
            RETURN;
        END

        IF @ImageType = 'Footer'
        BEGIN
            SELECT TOP 1 [ReportFooterImage] AS ImageData, [ReportFooterMimeType] AS ImageMimeType
            FROM [dbo].[LabMaster]
            WHERE [Id] = 1;
            RETURN;
        END

        SELECT TOP 1 [Logo] AS ImageData, [LogoMimeType] AS ImageMimeType
        FROM [dbo].[LabMaster]
        WHERE [Id] = 1;
        RETURN;
    END

    SELECT @StatusCode = 0, @StatusMsg = 'Invalid action';
END
GO
