USE [eMedLis]
GO

IF OBJECT_ID('[dbo].[ParameterMaster]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ParameterMaster](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ParameterHeaderId] [int] NULL,
        [ParameterName] [varchar](100) NOT NULL,
        [ShortName] [varchar](50) NULL,
        [Unit] [varchar](50) NULL,
        [ResultType] [varchar](50) NOT NULL,
        [DecimalPrecision] [int] NULL,
        [AllowRange] [bit] NOT NULL,
        [AllowCriticalRange] [bit] NOT NULL,
        [IsCalculated] [bit] NOT NULL,
        [Formula] [varchar](500) NULL,
        [Active] [bit] NOT NULL,
     CONSTRAINT [PK_ParameterMaster] PRIMARY KEY CLUSTERED
    (
        [Id] ASC
    ) ON [PRIMARY],
     CONSTRAINT [IX_ParameterMaster_ParameterName] UNIQUE NONCLUSTERED
    (
        [ParameterName] ASC
    ) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ParameterMaster')
      AND name = 'ParameterHeaderId'
      AND is_nullable = 0
)
BEGIN
    ALTER TABLE dbo.ParameterMaster ALTER COLUMN ParameterHeaderId INT NULL
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_ParameterMaster_Active')
BEGIN
    ALTER TABLE [dbo].[ParameterMaster] ADD CONSTRAINT [DF_ParameterMaster_Active] DEFAULT ((0)) FOR [Active]
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_ParameterMaster_AllowRange')
BEGIN
    ALTER TABLE [dbo].[ParameterMaster] ADD CONSTRAINT [DF_ParameterMaster_AllowRange] DEFAULT ((0)) FOR [AllowRange]
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_ParameterMaster_AllowCriticalRange')
BEGIN
    ALTER TABLE [dbo].[ParameterMaster] ADD CONSTRAINT [DF_ParameterMaster_AllowCriticalRange] DEFAULT ((0)) FOR [AllowCriticalRange]
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_ParameterMaster_IsCalculated')
BEGIN
    ALTER TABLE [dbo].[ParameterMaster] ADD CONSTRAINT [DF_ParameterMaster_IsCalculated] DEFAULT ((0)) FOR [IsCalculated]
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ParameterMaster_ParameterHeader')
BEGIN
    ALTER TABLE [dbo].[ParameterMaster] WITH CHECK ADD CONSTRAINT [FK_ParameterMaster_ParameterHeader] FOREIGN KEY([ParameterHeaderId])
    REFERENCES [dbo].[ParameterHeader] ([Id])
    ALTER TABLE [dbo].[ParameterMaster] CHECK CONSTRAINT [FK_ParameterMaster_ParameterHeader]
END
GO

IF OBJECT_ID('[dbo].[ParameterDropdownValue]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ParameterDropdownValue](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ParameterId] [int] NOT NULL,
        [ValueText] [varchar](100) NOT NULL,
        [DisplayOrder] [int] NOT NULL,
        [Active] [bit] NOT NULL,
        CONSTRAINT [PK_ParameterDropdownValue] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ParameterDropdownValue_ParameterMaster')
BEGIN
    ALTER TABLE [dbo].[ParameterDropdownValue] WITH CHECK ADD CONSTRAINT [FK_ParameterDropdownValue_ParameterMaster] FOREIGN KEY([ParameterId])
    REFERENCES [dbo].[ParameterMaster]([Id])
    ALTER TABLE [dbo].[ParameterDropdownValue] CHECK CONSTRAINT [FK_ParameterDropdownValue_ParameterMaster]
END
GO

IF OBJECT_ID('[dbo].[Usp_ParameterDropdownValue]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[Usp_ParameterDropdownValue]
GO

CREATE PROCEDURE [dbo].[Usp_ParameterDropdownValue]
(
   @Id INTEGER = NULL,
   @ParameterId INTEGER = NULL,
   @ValueText VARCHAR(100) = NULL,
   @DisplayOrder INT = NULL,
   @Active BIT = NULL,
   @Action VARCHAR(30)
)
AS
BEGIN

IF @Action = 'Add'
BEGIN
    INSERT INTO ParameterDropdownValue(ParameterId, ValueText, DisplayOrder, Active)
    VALUES(@ParameterId, @ValueText, ISNULL(@DisplayOrder,1), ISNULL(@Active,1))
END

IF @Action = 'DeleteByParameter'
BEGIN
    DELETE FROM ParameterDropdownValue WHERE ParameterId = @ParameterId
END

IF @Action = 'GetByParameter'
BEGIN
    SELECT Id, ParameterId, ValueText, DisplayOrder, ISNULL(Active,1) Active
    FROM ParameterDropdownValue
    WHERE ParameterId = @ParameterId
    ORDER BY DisplayOrder, Id
END

END
GO

IF OBJECT_ID('[dbo].[Usp_ParameterMaster]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[Usp_ParameterMaster]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Usp_ParameterMaster]
(
   @Id INTEGER = NULL,
   @ParameterHeaderId INTEGER = NULL,
   @ParameterName VARCHAR(100) = NULL,
   @ShortName VARCHAR(50) = NULL,
   @Unit VARCHAR(50) = NULL,
   @ResultType VARCHAR(50) = NULL,
   @DecimalPrecision INT = NULL,
   @AllowRange BIT = NULL,
   @AllowCriticalRange BIT = NULL,
   @IsCalculated BIT = NULL,
   @Formula VARCHAR(500) = NULL,
   @Active BIT = NULL,
   @Action VARCHAR(20),
   @StatusCode INT = 0 OUTPUT,
   @StatusMsg VARCHAR(100) = NULL OUTPUT
)
AS
BEGIN

IF @Action = 'Add'
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ParameterMaster WHERE ParameterName = @ParameterName)
    BEGIN
        INSERT INTO ParameterMaster
            (ParameterHeaderId, ParameterName, ShortName, Unit, ResultType, DecimalPrecision, AllowRange, AllowCriticalRange, IsCalculated, Formula, Active)
        VALUES
            (@ParameterHeaderId, @ParameterName, @ShortName, @Unit, @ResultType, @DecimalPrecision, @AllowRange, @AllowCriticalRange,
             CASE WHEN @ResultType = 'Calculated' THEN 1 ELSE 0 END,
             CASE WHEN @ResultType = 'Calculated' THEN @Formula ELSE NULL END,
             @Active)
        SELECT @StatusCode = 1, @StatusMsg = 'Parameter Created Successfully'
    END
    ELSE
    BEGIN
        SELECT @StatusCode = 0, @StatusMsg = '"' + @ParameterName + '" Already Exist'
    END
END

IF @Action='Update'
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ParameterMaster WHERE ParameterName = @ParameterName AND Id <> @Id)
    BEGIN
        UPDATE ParameterMaster
        SET ParameterHeaderId = @ParameterHeaderId,
            ParameterName = @ParameterName,
            ShortName = @ShortName,
            Unit = @Unit,
            ResultType = @ResultType,
            DecimalPrecision = @DecimalPrecision,
            AllowRange = @AllowRange,
            AllowCriticalRange = @AllowCriticalRange,
            IsCalculated = CASE WHEN @ResultType = 'Calculated' THEN 1 ELSE 0 END,
            Formula = CASE WHEN @ResultType = 'Calculated' THEN @Formula ELSE NULL END,
            Active = @Active
        WHERE Id = @Id

        SELECT @StatusCode = 1, @StatusMsg = 'Parameter Updated Successfully'
    END
    ELSE
    BEGIN
        SELECT @StatusCode = 0, @StatusMsg = '"' + @ParameterName + '" Already Exist'
    END
END

IF @Action='SetActive'
BEGIN
    UPDATE ParameterMaster SET Active = @Active WHERE Id = @Id
    SELECT @StatusCode = 1, @StatusMsg = CASE WHEN @Active = 1 THEN 'Parameter Activated Successfully' ELSE 'Parameter Deactivated Successfully' END
END

IF @Action='GetParameter'
BEGIN
    SELECT PM.Id,
           PM.ParameterHeaderId,
           ISNULL(PH.HeaderName,'') HeaderName,
           PM.ParameterName,
           PM.ShortName,
           PM.Unit,
           PM.ResultType,
           ISNULL(PM.DecimalPrecision,0) DecimalPrecision,
           ISNULL(PM.AllowRange,0) AllowRange,
           ISNULL(PM.AllowCriticalRange,0) AllowCriticalRange,
           ISNULL(PM.IsCalculated,0) IsCalculated,
           PM.Formula,
           ISNULL(PM.Active,0) Active,
           STUFF((
              SELECT ', ' + DV.ValueText
              FROM ParameterDropdownValue DV
              WHERE DV.ParameterId = PM.Id
                AND ISNULL(DV.Active,1) = 1
              ORDER BY DV.DisplayOrder
              FOR XML PATH(''), TYPE
           ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS DropdownDisplayValues
    FROM ParameterMaster PM
    LEFT JOIN ParameterHeader PH ON PH.Id = PM.ParameterHeaderId
    ORDER BY PM.Id DESC
END

IF @Action='GetParameterById'
BEGIN
    SELECT PM.Id,
           PM.ParameterHeaderId,
           PM.ParameterName,
           PM.ShortName,
           PM.Unit,
           PM.ResultType,
           ISNULL(PM.DecimalPrecision,0) DecimalPrecision,
           ISNULL(PM.AllowRange,0) AllowRange,
           ISNULL(PM.AllowCriticalRange,0) AllowCriticalRange,
           ISNULL(PM.IsCalculated,0) IsCalculated,
           PM.Formula,
           ISNULL(PM.Active,0) Active
    FROM ParameterMaster PM
    WHERE PM.Id = @Id
END

END
GO
