USE [eMedLis]
GO

IF OBJECT_ID('[dbo].[InvestigationTemplate]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[InvestigationTemplate](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [InvestigationId] [varchar](50) NOT NULL,
        [ItemType] [varchar](20) NOT NULL,
        [HeaderId] [int] NULL,
        [ParameterId] [int] NULL,
        [MethodId] [int] NULL,
        [DisplayOrder] [int] NOT NULL,
        [Active] [bit] NOT NULL,
     CONSTRAINT [PK_InvestigationTemplate] PRIMARY KEY CLUSTERED 
    (
        [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_InvestigationTemplate_Active')
BEGIN
    ALTER TABLE [dbo].[InvestigationTemplate] ADD CONSTRAINT [DF_InvestigationTemplate_Active] DEFAULT ((1)) FOR [Active]
END
GO

IF COL_LENGTH('dbo.InvestigationTemplate','MethodId') IS NULL
BEGIN
    ALTER TABLE [dbo].[InvestigationTemplate] ADD [MethodId] [int] NULL
END
GO

IF OBJECT_ID('[dbo].[InvestigationTemplateMeta]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[InvestigationTemplateMeta](
        [InvestigationId] [varchar](50) NOT NULL,
        [InterpretationHtml] [nvarchar](max) NULL,
        [UpdatedOn] [datetime] NOT NULL CONSTRAINT [DF_InvestigationTemplateMeta_UpdatedOn] DEFAULT (GETDATE()),
        CONSTRAINT [PK_InvestigationTemplateMeta] PRIMARY KEY CLUSTERED ([InvestigationId] ASC)
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO

-- InvestigationId is varchar(50) in Investigations, keep no FK to avoid mismatch.

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_InvestigationTemplate_Header')
BEGIN
    ALTER TABLE [dbo].[InvestigationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_InvestigationTemplate_Header] FOREIGN KEY([HeaderId])
    REFERENCES [dbo].[ParameterHeader] ([Id])
    ALTER TABLE [dbo].[InvestigationTemplate] CHECK CONSTRAINT [FK_InvestigationTemplate_Header]
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_InvestigationTemplate_Parameter')
BEGIN
    ALTER TABLE [dbo].[InvestigationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_InvestigationTemplate_Parameter] FOREIGN KEY([ParameterId])
    REFERENCES [dbo].[ParameterMaster] ([Id])
    ALTER TABLE [dbo].[InvestigationTemplate] CHECK CONSTRAINT [FK_InvestigationTemplate_Parameter]
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_InvestigationTemplate_Method')
BEGIN
    ALTER TABLE [dbo].[InvestigationTemplate]  WITH CHECK ADD  CONSTRAINT [FK_InvestigationTemplate_Method] FOREIGN KEY([MethodId])
    REFERENCES [dbo].[TestMethod] ([Id])
    ALTER TABLE [dbo].[InvestigationTemplate] CHECK CONSTRAINT [FK_InvestigationTemplate_Method]
END
GO

IF OBJECT_ID('[dbo].[Usp_InvestigationTemplate]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[Usp_InvestigationTemplate]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Usp_InvestigationTemplate]
(  
   @InvestigationId VARCHAR(50) = NULL,
   @ItemType VARCHAR(20) = NULL,
   @HeaderId INTEGER = NULL,
   @ParameterId INTEGER = NULL,
   @MethodId INTEGER = NULL,
   @InterpretationHtml NVARCHAR(MAX) = NULL,
   @DisplayOrder INTEGER = NULL,
   @Active BIT = NULL,
   @Action VARCHAR(30),
   @StatusCode INT = 0 OUTPUT,
   @StatusMsg VARCHAR(100) = NULL OUTPUT
)  
AS
BEGIN

IF @Action = 'AddItem'
BEGIN
    INSERT INTO InvestigationTemplate
        (InvestigationId, ItemType, HeaderId, ParameterId, MethodId, DisplayOrder, Active)
    VALUES
        (@InvestigationId, @ItemType, @HeaderId, @ParameterId, @MethodId, @DisplayOrder, @Active)
    SELECT @StatusCode = 1, @StatusMsg = 'Template Item Added Successfully'
END

IF @Action = 'DeleteByInvestigation'
BEGIN
    DELETE FROM InvestigationTemplate WHERE InvestigationId = @InvestigationId
END

IF @Action = 'GetByInvestigation'
BEGIN
    SELECT IT.Id, IT.InvestigationId, INV.InvName InvestigationName, IT.ItemType,
           IT.HeaderId, ISNULL(PH.HeaderName,'') HeaderName,
           IT.ParameterId, ISNULL(PM.ParameterName,'') ParameterName,
           IT.MethodId, ISNULL(TM.MethodName,'') MethodName, ISNULL(PM.Unit,'') Unit,
           ISNULL(PM.ResultType,'') ResultType, ISNULL(PM.Formula,'') Formula, ISNULL(PM.DecimalPrecision,0) DecimalPrecision,
           IT.DisplayOrder, ISNULL(IT.Active,0) Active
    FROM InvestigationTemplate IT
    INNER JOIN Investigations INV ON INV.Id = IT.InvestigationId
    LEFT JOIN ParameterHeader PH ON PH.Id = IT.HeaderId
    LEFT JOIN ParameterMaster PM ON PM.Id = IT.ParameterId
    LEFT JOIN TestMethod TM ON TM.Id = IT.MethodId
    WHERE IT.InvestigationId = @InvestigationId
    ORDER BY IT.DisplayOrder
END

IF @Action = 'SaveInterpretation'
BEGIN
    IF EXISTS (SELECT 1 FROM InvestigationTemplateMeta WHERE InvestigationId = @InvestigationId)
    BEGIN
        UPDATE InvestigationTemplateMeta
        SET InterpretationHtml = @InterpretationHtml,
            UpdatedOn = GETDATE()
        WHERE InvestigationId = @InvestigationId
    END
    ELSE
    BEGIN
        INSERT INTO InvestigationTemplateMeta (InvestigationId, InterpretationHtml, UpdatedOn)
        VALUES (@InvestigationId, @InterpretationHtml, GETDATE())
    END
END

IF @Action = 'GetInterpretation'
BEGIN
    SELECT TOP 1 ISNULL(InterpretationHtml, '') AS InterpretationHtml
    FROM InvestigationTemplateMeta
    WHERE InvestigationId = @InvestigationId
END

END
GO
