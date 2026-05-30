USE [eMedLis]
GO

IF OBJECT_ID('[dbo].[TestMethod]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[TestMethod](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [MethodName] [varchar](100) NOT NULL,
        [Active] [bit] NOT NULL,
     CONSTRAINT [PK_TestMethod] PRIMARY KEY CLUSTERED 
    (
        [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
     CONSTRAINT [IX_TestMethod_MethodName] UNIQUE NONCLUSTERED 
    (
        [MethodName] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_TestMethod_Active')
BEGIN
    ALTER TABLE [dbo].[TestMethod] ADD CONSTRAINT [DF_TestMethod_Active] DEFAULT ((1)) FOR [Active]
END
GO

IF NOT EXISTS (SELECT 1 FROM TestMethod WHERE MethodName = 'None')
BEGIN
    INSERT INTO TestMethod (MethodName, Active) VALUES ('None', 1)
END
GO

IF OBJECT_ID('[dbo].[Usp_TestMethod]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[Usp_TestMethod]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Usp_TestMethod]
(  
   @Id INTEGER = NULL,
   @MethodName VARCHAR(100) = NULL,
   @Active BIT = NULL,
   @Action VARCHAR(20),
   @StatusCode INT = 0 OUTPUT,
   @StatusMsg VARCHAR(100) = NULL OUTPUT
)  
AS
BEGIN

IF @Action = 'Add'
BEGIN
    IF NOT EXISTS (SELECT MethodName FROM TestMethod WHERE MethodName = @MethodName)
        BEGIN
            INSERT INTO TestMethod (MethodName, Active) VALUES (@MethodName, @Active)
            SELECT @StatusCode = 1, @StatusMsg = 'Test Method Created Successfully'
        END 
    ELSE
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = '"' + @MethodName + '"' + ' ' + 'Already Exist'
        END
END

IF @Action='Update'
BEGIN
    IF NOT EXISTS (SELECT MethodName FROM TestMethod WHERE MethodName = @MethodName AND Id <> @Id)
        BEGIN
            UPDATE TestMethod SET MethodName = @MethodName, Active = @Active WHERE Id = @Id
            SELECT @StatusCode = 1, @StatusMsg = 'Test Method Updated Successfully'
        END
    ELSE
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = '"' + @MethodName + '"' + ' ' + 'Already Exist'
        END
END

IF @Action='Delete'
BEGIN
    DELETE FROM TestMethod WHERE Id = @Id
    SELECT @StatusCode = 1, @StatusMsg = 'Test Method Deleted Successfully'
END

IF @Action='GetMethods'
BEGIN
    SELECT Id, MethodName, ISNULL(Active,0) Active FROM TestMethod ORDER BY Id
END

IF @Action='GetMethodById'
BEGIN
    SELECT Id, MethodName, ISNULL(Active,0) Active FROM TestMethod WHERE Id = @Id
END

END
GO
