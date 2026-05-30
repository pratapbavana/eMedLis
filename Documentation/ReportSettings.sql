SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[ReportSettings]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ReportSettings]
    (
        [Id] INT NOT NULL PRIMARY KEY,
        [PrintMode] VARCHAR(20) NOT NULL CONSTRAINT [DF_ReportSettings_PrintMode] DEFAULT ('PlainPaper'),
        [PrintHeader] BIT NOT NULL CONSTRAINT [DF_ReportSettings_PrintHeader] DEFAULT (1),
        [HeaderHeightPx] INT NOT NULL CONSTRAINT [DF_ReportSettings_HeaderHeightPx] DEFAULT (120),
        [ShowLogo] BIT NOT NULL CONSTRAINT [DF_ReportSettings_ShowLogo] DEFAULT (1),
        [ShowLabDetails] BIT NOT NULL CONSTRAINT [DF_ReportSettings_ShowLabDetails] DEFAULT (1),
        [PrintFooter] BIT NOT NULL CONSTRAINT [DF_ReportSettings_PrintFooter] DEFAULT (1),
        [FooterHeightPx] INT NOT NULL CONSTRAINT [DF_ReportSettings_FooterHeightPx] DEFAULT (60),
        [FooterText] NVARCHAR(500) NULL,
        [TopMarginPx] INT NOT NULL CONSTRAINT [DF_ReportSettings_TopMarginPx] DEFAULT (38),
        [LeftMarginPx] INT NOT NULL CONSTRAINT [DF_ReportSettings_LeftMarginPx] DEFAULT (38),
        [RightMarginPx] INT NOT NULL CONSTRAINT [DF_ReportSettings_RightMarginPx] DEFAULT (38),
        [BottomMarginPx] INT NOT NULL CONSTRAINT [DF_ReportSettings_BottomMarginPx] DEFAULT (38),
        [ContentStartPx] INT NOT NULL CONSTRAINT [DF_ReportSettings_ContentStartPx] DEFAULT (0),
        [LabName] NVARCHAR(200) NULL,
        [LabAddress] NVARCHAR(300) NULL,
        [LabPhone] NVARCHAR(50) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        [UpdatedOn] DATETIME NOT NULL CONSTRAINT [DF_ReportSettings_UpdatedOn] DEFAULT (GETDATE())
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[ReportSettings] WHERE [Id] = 1)
BEGIN
    INSERT INTO [dbo].[ReportSettings]
    (
        [Id], [PrintMode], [PrintHeader], [HeaderHeightPx], [ShowLogo], [ShowLabDetails],
        [PrintFooter], [FooterHeightPx], [FooterText], [TopMarginPx], [LeftMarginPx], [RightMarginPx], [BottomMarginPx],
        [ContentStartPx], [LabName], [LabAddress], [LabPhone], [UpdatedBy], [UpdatedOn]
    )
    VALUES
    (
        1, 'PlainPaper', 1, 120, 1, 1,
        1, 60, N'This is a system generated report.', 38, 38, 38, 38,
        0, N'SSK Diagnostics', N'', N'', N'system', GETDATE()
    );
END
GO

IF OBJECT_ID('[dbo].[Usp_ReportSettings]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[Usp_ReportSettings]
GO

CREATE PROCEDURE [dbo].[Usp_ReportSettings]
    @PrintMode VARCHAR(20) = NULL,
    @PrintHeader BIT = NULL,
    @HeaderHeightPx INT = NULL,
    @ShowLogo BIT = NULL,
    @ShowLabDetails BIT = NULL,
    @PrintFooter BIT = NULL,
    @FooterHeightPx INT = NULL,
    @FooterText NVARCHAR(500) = NULL,
    @TopMarginPx INT = NULL,
    @LeftMarginPx INT = NULL,
    @RightMarginPx INT = NULL,
    @BottomMarginPx INT = NULL,
    @ContentStartPx INT = NULL,
    @LabName NVARCHAR(200) = NULL,
    @LabAddress NVARCHAR(300) = NULL,
    @LabPhone NVARCHAR(50) = NULL,
    @UpdatedBy NVARCHAR(100) = NULL,
    @Action VARCHAR(20),
    @StatusCode INT OUTPUT,
    @StatusMsg VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @StatusCode = 0;
    SET @StatusMsg = '';

    IF @Action = 'Get'
    BEGIN
        SELECT TOP 1
            [PrintMode],
            [PrintHeader],
            [HeaderHeightPx],
            [ShowLogo],
            [ShowLabDetails],
            [PrintFooter],
            [FooterHeightPx],
            [FooterText],
            [TopMarginPx],
            [LeftMarginPx],
            [RightMarginPx],
            [BottomMarginPx],
            [ContentStartPx],
            [LabName],
            [LabAddress],
            [LabPhone]
        FROM [dbo].[ReportSettings]
        WHERE [Id] = 1;

        SET @StatusCode = 1;
        SET @StatusMsg = 'Loaded';
        RETURN;
    END

    IF @Action = 'Save'
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[ReportSettings] WHERE [Id] = 1)
        BEGIN
            INSERT INTO [dbo].[ReportSettings] ([Id]) VALUES (1);
        END

        UPDATE [dbo].[ReportSettings]
        SET
            [PrintMode] = CASE WHEN @PrintMode = 'PrePrinted' THEN 'PrePrinted' ELSE 'PlainPaper' END,
            [PrintHeader] = ISNULL(@PrintHeader, 1),
            [HeaderHeightPx] = ISNULL(@HeaderHeightPx, 120),
            [ShowLogo] = ISNULL(@ShowLogo, 1),
            [ShowLabDetails] = ISNULL(@ShowLabDetails, 1),
            [PrintFooter] = ISNULL(@PrintFooter, 1),
            [FooterHeightPx] = ISNULL(@FooterHeightPx, 60),
            [FooterText] = ISNULL(@FooterText, ''),
            [TopMarginPx] = ISNULL(@TopMarginPx, 38),
            [LeftMarginPx] = ISNULL(@LeftMarginPx, 38),
            [RightMarginPx] = ISNULL(@RightMarginPx, 38),
            [BottomMarginPx] = ISNULL(@BottomMarginPx, 38),
            [ContentStartPx] = ISNULL(@ContentStartPx, 0),
            [LabName] = ISNULL(@LabName, 'SSK Diagnostics'),
            [LabAddress] = ISNULL(@LabAddress, ''),
            [LabPhone] = ISNULL(@LabPhone, ''),
            [UpdatedBy] = ISNULL(@UpdatedBy, ''),
            [UpdatedOn] = GETDATE()
        WHERE [Id] = 1;

        SET @StatusCode = 1;
        SET @StatusMsg = 'Report settings saved';
        RETURN;
    END

    SET @StatusCode = 0;
    SET @StatusMsg = 'Invalid action';
END
GO
