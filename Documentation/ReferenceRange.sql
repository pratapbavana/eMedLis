USE [eMedLis]
GO

IF OBJECT_ID('[dbo].[ReferenceRange]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ReferenceRange](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ParameterId] [int] NOT NULL,
        [MethodId] [int] NOT NULL,
        [Gender] [varchar](20) NOT NULL,
        [MethodName] [varchar](100) NULL,
        [AgeFromValue] [decimal](10,2) NOT NULL,
        [AgeFromUnit] [varchar](20) NOT NULL,
        [AgeToValue] [decimal](10,2) NOT NULL,
        [AgeToUnit] [varchar](20) NOT NULL,
        [AgeFromDays] [int] NOT NULL,
        [AgeToDays] [int] NOT NULL,
        [NormalMin] [decimal](18,4) NULL,
        [NormalMax] [decimal](18,4) NULL,
        [CriticalMin] [decimal](18,4) NULL,
        [CriticalMax] [decimal](18,4) NULL,
        [RangeText] [nvarchar](500) NULL,
        [Active] [bit] NOT NULL,
     CONSTRAINT [PK_ReferenceRange] PRIMARY KEY CLUSTERED 
    (
        [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

IF COL_LENGTH('dbo.ReferenceRange','MethodId') IS NULL
BEGIN
    ALTER TABLE [dbo].[ReferenceRange] ADD [MethodId] [int] NULL
END
GO

IF COL_LENGTH('dbo.ReferenceRange','MethodName') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[ReferenceRange] DROP COLUMN [MethodName]
END
GO

IF COL_LENGTH('dbo.ReferenceRange','RangeText') IS NULL
BEGIN
    ALTER TABLE [dbo].[ReferenceRange] ADD [RangeText] [nvarchar](500) NULL
END
GO

ALTER TABLE [dbo].[ReferenceRange] ALTER COLUMN [NormalMin] [decimal](18,4) NULL
GO
ALTER TABLE [dbo].[ReferenceRange] ALTER COLUMN [NormalMax] [decimal](18,4) NULL
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TestMethod')
BEGIN
    DECLARE @NoneMethodId INT
    SELECT @NoneMethodId = Id FROM TestMethod WHERE MethodName = 'None'
    IF @NoneMethodId IS NOT NULL
    BEGIN
        UPDATE ReferenceRange SET MethodId = @NoneMethodId WHERE MethodId IS NULL
    END
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_ReferenceRange_Active')
BEGIN
    ALTER TABLE [dbo].[ReferenceRange] ADD CONSTRAINT [DF_ReferenceRange_Active] DEFAULT ((0)) FOR [Active]
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ReferenceRange_ParameterMaster')
BEGIN
    ALTER TABLE [dbo].[ReferenceRange]  WITH CHECK ADD  CONSTRAINT [FK_ReferenceRange_ParameterMaster] FOREIGN KEY([ParameterId])
    REFERENCES [dbo].[ParameterMaster] ([Id])
    ALTER TABLE [dbo].[ReferenceRange] CHECK CONSTRAINT [FK_ReferenceRange_ParameterMaster]
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ReferenceRange_TestMethod')
BEGIN
    ALTER TABLE [dbo].[ReferenceRange]  WITH CHECK ADD  CONSTRAINT [FK_ReferenceRange_TestMethod] FOREIGN KEY([MethodId])
    REFERENCES [dbo].[TestMethod] ([Id])
    ALTER TABLE [dbo].[ReferenceRange] CHECK CONSTRAINT [FK_ReferenceRange_TestMethod]
END
GO

IF OBJECT_ID('[dbo].[Usp_ReferenceRange]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[Usp_ReferenceRange]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Usp_ReferenceRange]
(  
   @Id INTEGER = NULL,
   @ParameterId INTEGER = NULL,
   @MethodId INTEGER = NULL,
   @Gender VARCHAR(20) = NULL,
   @AgeFromValue DECIMAL(10,2) = NULL,
   @AgeFromUnit VARCHAR(20) = NULL,
   @AgeToValue DECIMAL(10,2) = NULL,
   @AgeToUnit VARCHAR(20) = NULL,
   @AgeFromDays INT = NULL,
   @AgeToDays INT = NULL,
   @NormalMin DECIMAL(18,4) = NULL,
   @NormalMax DECIMAL(18,4) = NULL,
   @CriticalMin DECIMAL(18,4) = NULL,
   @CriticalMax DECIMAL(18,4) = NULL,
   @RangeText NVARCHAR(500) = NULL,
   @Active BIT = NULL,
   @Action VARCHAR(40),
   @StatusCode INT = 0 OUTPUT,
   @StatusMsg VARCHAR(100) = NULL OUTPUT
)  
AS
BEGIN

IF @Action = 'Add'
BEGIN
    IF ((@NormalMin IS NULL OR @NormalMax IS NULL) AND ISNULL(LTRIM(RTRIM(@RangeText)), '') = '')
    BEGIN
        SELECT @StatusCode = 0, @StatusMsg = 'Provide numeric range or descriptive range text'
        RETURN
    END

    IF ((@NormalMin IS NULL AND @NormalMax IS NOT NULL) OR (@NormalMin IS NOT NULL AND @NormalMax IS NULL))
    BEGIN
        SELECT @StatusCode = 0, @StatusMsg = 'Both Normal Min and Normal Max are required for numeric range'
        RETURN
    END

    IF (@NormalMin IS NOT NULL AND @NormalMax IS NOT NULL AND @NormalMin >= @NormalMax)
    BEGIN
        SELECT @StatusCode = 0, @StatusMsg = 'Normal Min must be less than Normal Max'
        RETURN
    END

    IF EXISTS (
        SELECT 1 FROM ReferenceRange
        WHERE ParameterId = @ParameterId
          AND MethodId = @MethodId
          AND Gender = @Gender
          AND @AgeFromDays <= AgeToDays
          AND @AgeToDays >= AgeFromDays
    )
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = 'Overlapping age range for the same gender is not allowed'
        END
    ELSE
        BEGIN
            INSERT INTO ReferenceRange
                (ParameterId, MethodId, Gender, AgeFromValue, AgeFromUnit, AgeToValue, AgeToUnit, AgeFromDays, AgeToDays,
                 NormalMin, NormalMax, CriticalMin, CriticalMax, RangeText, Active)
            VALUES
                (@ParameterId, @MethodId, @Gender, @AgeFromValue, @AgeFromUnit, @AgeToValue, @AgeToUnit, @AgeFromDays, @AgeToDays,
                 @NormalMin, @NormalMax, @CriticalMin, @CriticalMax, @RangeText, @Active)
            SELECT @StatusCode = 1, @StatusMsg = 'Reference Range Created Successfully'
        END
END

IF @Action = 'AddBatch'
BEGIN
    IF ((@NormalMin IS NULL OR @NormalMax IS NULL) AND ISNULL(LTRIM(RTRIM(@RangeText)), '') = '')
    BEGIN
        SELECT @StatusCode = 0, @StatusMsg = 'Provide numeric range or descriptive range text'
        RETURN
    END

    IF ((@NormalMin IS NULL AND @NormalMax IS NOT NULL) OR (@NormalMin IS NOT NULL AND @NormalMax IS NULL))
    BEGIN
        SELECT @StatusCode = 0, @StatusMsg = 'Both Normal Min and Normal Max are required for numeric range'
        RETURN
    END

    IF (@NormalMin IS NOT NULL AND @NormalMax IS NOT NULL AND @NormalMin >= @NormalMax)
    BEGIN
        SELECT @StatusCode = 0, @StatusMsg = 'Normal Min must be less than Normal Max'
        RETURN
    END

    INSERT INTO ReferenceRange
        (ParameterId, MethodId, Gender, AgeFromValue, AgeFromUnit, AgeToValue, AgeToUnit, AgeFromDays, AgeToDays,
         NormalMin, NormalMax, CriticalMin, CriticalMax, RangeText, Active)
    VALUES
        (@ParameterId, @MethodId, @Gender, @AgeFromValue, @AgeFromUnit, @AgeToValue, @AgeToUnit, @AgeFromDays, @AgeToDays,
         @NormalMin, @NormalMax, @CriticalMin, @CriticalMax, @RangeText, @Active)
    SELECT @StatusCode = 1, @StatusMsg = 'Reference Range Created Successfully'
END

IF @Action='Update'
BEGIN
    IF ((@NormalMin IS NULL OR @NormalMax IS NULL) AND ISNULL(LTRIM(RTRIM(@RangeText)), '') = '')
    BEGIN
        SELECT @StatusCode = 0, @StatusMsg = 'Provide numeric range or descriptive range text'
        RETURN
    END

    IF ((@NormalMin IS NULL AND @NormalMax IS NOT NULL) OR (@NormalMin IS NOT NULL AND @NormalMax IS NULL))
    BEGIN
        SELECT @StatusCode = 0, @StatusMsg = 'Both Normal Min and Normal Max are required for numeric range'
        RETURN
    END

    IF (@NormalMin IS NOT NULL AND @NormalMax IS NOT NULL AND @NormalMin >= @NormalMax)
    BEGIN
        SELECT @StatusCode = 0, @StatusMsg = 'Normal Min must be less than Normal Max'
        RETURN
    END

    IF EXISTS (
        SELECT 1 FROM ReferenceRange
        WHERE ParameterId = @ParameterId
          AND MethodId = @MethodId
          AND Gender = @Gender
          AND Id <> @Id
          AND @AgeFromDays <= AgeToDays
          AND @AgeToDays >= AgeFromDays
    )
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = 'Overlapping age range for the same gender is not allowed'
        END
    ELSE
        BEGIN
            UPDATE ReferenceRange
            SET ParameterId = @ParameterId,
                MethodId = @MethodId,
                Gender = @Gender,
                AgeFromValue = @AgeFromValue,
                AgeFromUnit = @AgeFromUnit,
                AgeToValue = @AgeToValue,
                AgeToUnit = @AgeToUnit,
                AgeFromDays = @AgeFromDays,
                AgeToDays = @AgeToDays,
                NormalMin = @NormalMin,
                NormalMax = @NormalMax,
                CriticalMin = @CriticalMin,
                CriticalMax = @CriticalMax,
                RangeText = @RangeText,
                Active = @Active
            WHERE Id = @Id
            SELECT @StatusCode = 1, @StatusMsg = 'Reference Range Updated Successfully'
        END
END

IF @Action='SetActive'
BEGIN
    UPDATE ReferenceRange SET Active = @Active WHERE Id = @Id
    SELECT @StatusCode = 1, @StatusMsg = CASE WHEN @Active = 1 THEN 'Reference Range Activated Successfully' ELSE 'Reference Range Deactivated Successfully' END
END

IF @Action='GetByParameter'
BEGIN
    SELECT RR.Id, RR.ParameterId, PM.ParameterName, RR.MethodId, TM.MethodName, RR.Gender,
           RR.AgeFromValue, RR.AgeFromUnit, RR.AgeToValue, RR.AgeToUnit, RR.AgeFromDays, RR.AgeToDays,
           RR.NormalMin, RR.NormalMax, RR.CriticalMin, RR.CriticalMax, ISNULL(RR.RangeText,'') RangeText,
           ISNULL(RR.Active,0) Active
    FROM ReferenceRange RR
    INNER JOIN ParameterMaster PM ON PM.Id = RR.ParameterId
    INNER JOIN TestMethod TM ON TM.Id = RR.MethodId
    WHERE RR.ParameterId = @ParameterId
    ORDER BY RR.AgeFromDays, RR.AgeToDays
END

IF @Action='GetCombos'
BEGIN
    SELECT PM.Id ParameterId,
           PM.ParameterName,
           RR.MethodId,
           TM.MethodName,
           COUNT(*) RangeCount,
           SUM(CASE WHEN ISNULL(RR.Active,0)=1 THEN 1 ELSE 0 END) ActiveCount
    FROM ReferenceRange RR
    INNER JOIN ParameterMaster PM ON PM.Id = RR.ParameterId
    INNER JOIN TestMethod TM ON TM.Id = RR.MethodId
    GROUP BY PM.Id, PM.ParameterName, RR.MethodId, TM.MethodName
    ORDER BY PM.ParameterName
END

IF @Action='GetByParameterMethod'
BEGIN
    SELECT RR.Id, RR.ParameterId, PM.ParameterName, RR.MethodId, TM.MethodName, RR.Gender,
           RR.AgeFromValue, RR.AgeFromUnit, RR.AgeToValue, RR.AgeToUnit, RR.AgeFromDays, RR.AgeToDays,
           RR.NormalMin, RR.NormalMax, RR.CriticalMin, RR.CriticalMax, ISNULL(RR.RangeText,'') RangeText,
           ISNULL(RR.Active,0) Active
    FROM ReferenceRange RR
    INNER JOIN ParameterMaster PM ON PM.Id = RR.ParameterId
    INNER JOIN TestMethod TM ON TM.Id = RR.MethodId
    WHERE RR.ParameterId = @ParameterId
      AND RR.MethodId = @MethodId
    ORDER BY RR.AgeFromDays, RR.AgeToDays
END

IF @Action IN ('ExistsByParameterMethod', 'ExistsByParamMethod')
BEGIN
    SELECT CASE WHEN EXISTS (
        SELECT 1 FROM ReferenceRange
        WHERE ParameterId = @ParameterId
          AND (MethodId = @MethodId OR MethodId IS NULL)
    ) THEN 1 ELSE 0 END AS ExistsFlag
END

IF @Action IN ('DeleteByParameterMethod', 'DeleteByParamMethod')
BEGIN
    DELETE FROM ReferenceRange
    WHERE ParameterId = @ParameterId
      AND (MethodId = @MethodId OR MethodId IS NULL)
END

IF @Action='GetById'
BEGIN
    SELECT RR.Id, RR.ParameterId, RR.MethodId, TM.MethodName, RR.Gender,
           RR.AgeFromValue, RR.AgeFromUnit, RR.AgeToValue, RR.AgeToUnit, RR.AgeFromDays, RR.AgeToDays,
           RR.NormalMin, RR.NormalMax, RR.CriticalMin, RR.CriticalMax, ISNULL(RR.RangeText,'') RangeText,
           ISNULL(RR.Active,0) Active
    FROM ReferenceRange RR
    INNER JOIN TestMethod TM ON TM.Id = RR.MethodId
    WHERE RR.Id = @Id
END

END
GO
