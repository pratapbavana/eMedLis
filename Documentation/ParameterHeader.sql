USE [eMedLis]
GO

IF OBJECT_ID('[dbo].[ParameterHeader]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ParameterHeader](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [HeaderName] [varchar](100) NOT NULL,
        [Active] [bit] NOT NULL,
     CONSTRAINT [PK_ParameterHeader] PRIMARY KEY CLUSTERED 
    (
        [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
     CONSTRAINT [IX_ParameterHeader_HeaderName] UNIQUE NONCLUSTERED 
    (
        [HeaderName] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_ParameterHeader_Active')
BEGIN
    ALTER TABLE [dbo].[ParameterHeader] ADD CONSTRAINT [DF_ParameterHeader_Active] DEFAULT ((0)) FOR [Active]
END
GO

IF OBJECT_ID('[dbo].[Usp_ParameterHeader]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[Usp_ParameterHeader]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Usp_ParameterHeader]
(  
   @Id INTEGER = NULL,
   @HeaderName VARCHAR(100) = NULL,
   @Active BIT = NULL,
   @Action VARCHAR(20),
   @StatusCode INT = 0 OUTPUT,
   @StatusMsg VARCHAR(100) = NULL OUTPUT
)  
AS
BEGIN

IF @Action = 'Add'
BEGIN
    IF NOT EXISTS (SELECT HeaderName FROM ParameterHeader WHERE HeaderName = @HeaderName)
        BEGIN
            INSERT INTO ParameterHeader (HeaderName, Active) VALUES (@HeaderName, @Active)
            SELECT @StatusCode = 1, @StatusMsg = 'Parameter Header Created Successfully'
        END 
    ELSE
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = '"' + @HeaderName + '"' + ' ' + 'Already Exist'
        END
END

IF @Action='Update'
BEGIN
    IF NOT EXISTS (SELECT HeaderName FROM ParameterHeader WHERE HeaderName = @HeaderName AND Id <> @Id)
        BEGIN
            UPDATE ParameterHeader SET HeaderName = @HeaderName, Active = @Active WHERE Id = @Id
            SELECT @StatusCode = 1, @StatusMsg = 'Parameter Header Updated Successfully'
        END
    ELSE
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = '"' + @HeaderName + '"' + ' ' + 'Already Exist'
        END
END

IF @Action='Delete'
BEGIN
    DELETE FROM ParameterHeader WHERE Id = @Id
    SELECT @StatusCode = 1, @StatusMsg = 'Parameter Header Deleted Successfully'
END

IF @Action='GetHeader'
BEGIN
    SELECT Id, HeaderName, ISNULL(Active,0) Active FROM ParameterHeader ORDER BY Id DESC
END

IF @Action='GetHeaderById'
BEGIN
    SELECT Id, HeaderName, ISNULL(Active,0) Active FROM ParameterHeader WHERE Id = @Id
END

END
GO
