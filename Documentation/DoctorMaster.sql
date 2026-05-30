USE [eMedLis]
GO

IF OBJECT_ID('[dbo].[DoctorMaster]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DoctorMaster](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [int] NOT NULL,
        [Designation] [varchar](100) NULL,
        [RegistrationNumber] [varchar](50) NULL,
        [SignatureImage] [varbinary](max) NULL,
        [SignatureMimeType] [varchar](50) NULL,
        [Active] [bit] NOT NULL CONSTRAINT [DF_DoctorMaster_Active] DEFAULT ((1)),
        CONSTRAINT [PK_DoctorMaster] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_DoctorMaster_UserId] UNIQUE ([UserId])
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO

IF OBJECT_ID('[dbo].[DoctorSubDepartment]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DoctorSubDepartment](
        [DoctorId] [int] NOT NULL,
        [SubDepartmentId] [bigint] NOT NULL,
        CONSTRAINT [PK_DoctorSubDepartment] PRIMARY KEY CLUSTERED ([DoctorId], [SubDepartmentId])
    ) ON [PRIMARY]
END
GO

IF OBJECT_ID('[dbo].[DoctorSubDepartment]', 'U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM sys.columns c
        INNER JOIN sys.types t ON t.user_type_id = c.user_type_id
        WHERE c.object_id = OBJECT_ID('[dbo].[DoctorSubDepartment]')
          AND c.name = 'SubDepartmentId'
          AND t.name <> 'bigint'
    )
    BEGIN
        DECLARE @PkName NVARCHAR(200);
        SELECT @PkName = kc.name
        FROM sys.key_constraints kc
        WHERE kc.parent_object_id = OBJECT_ID('[dbo].[DoctorSubDepartment]')
          AND kc.[type] = 'PK';

        IF @PkName IS NOT NULL
            EXEC('ALTER TABLE [dbo].[DoctorSubDepartment] DROP CONSTRAINT [' + @PkName + ']');

        ALTER TABLE [dbo].[DoctorSubDepartment] ALTER COLUMN [SubDepartmentId] BIGINT NOT NULL;

        ALTER TABLE [dbo].[DoctorSubDepartment]
        ADD CONSTRAINT [PK_DoctorSubDepartment] PRIMARY KEY CLUSTERED ([DoctorId], [SubDepartmentId]);
    END
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DoctorMaster_User')
BEGIN
    ALTER TABLE [dbo].[DoctorMaster] WITH CHECK ADD CONSTRAINT [FK_DoctorMaster_User]
    FOREIGN KEY([UserId]) REFERENCES [dbo].[Users]([UserId])
    ALTER TABLE [dbo].[DoctorMaster] CHECK CONSTRAINT [FK_DoctorMaster_User]
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DoctorSubDepartment_Doctor')
BEGIN
    ALTER TABLE [dbo].[DoctorSubDepartment] WITH CHECK ADD CONSTRAINT [FK_DoctorSubDepartment_Doctor]
    FOREIGN KEY([DoctorId]) REFERENCES [dbo].[DoctorMaster]([Id])
    ALTER TABLE [dbo].[DoctorSubDepartment] CHECK CONSTRAINT [FK_DoctorSubDepartment_Doctor]
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DoctorSubDepartment_SubDepartment')
BEGIN
    ALTER TABLE [dbo].[DoctorSubDepartment] WITH CHECK ADD CONSTRAINT [FK_DoctorSubDepartment_SubDepartment]
    FOREIGN KEY([SubDepartmentId]) REFERENCES [dbo].[SubDepartment]([Id])
    ALTER TABLE [dbo].[DoctorSubDepartment] CHECK CONSTRAINT [FK_DoctorSubDepartment_SubDepartment]
END
GO

IF OBJECT_ID('[dbo].[Usp_DoctorMaster]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[Usp_DoctorMaster]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Usp_DoctorMaster]
(
    @Id INT = NULL,
    @UserId INT = NULL,
    @Designation VARCHAR(100) = NULL,
    @RegistrationNumber VARCHAR(50) = NULL,
    @SubDepartmentIds VARCHAR(MAX) = NULL,
    @SignatureImage VARBINARY(MAX) = NULL,
    @SignatureMimeType VARCHAR(50) = NULL,
    @Active BIT = NULL,
    @Action VARCHAR(30),
    @StatusCode INT = 0 OUTPUT,
    @StatusMsg VARCHAR(200) = NULL OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'Add'
    BEGIN
        DECLARE @SelectedSubDept TABLE (SubDepartmentId BIGINT PRIMARY KEY);
        IF ISNULL(@SubDepartmentIds, '') <> ''
        BEGIN
            DECLARE @SubDeptXml XML;
            SET @SubDeptXml = CAST('<x><i>' + REPLACE(REPLACE(@SubDepartmentIds, '&', '&amp;'), ',', '</i><i>') + '</i></x>' AS XML);

            INSERT INTO @SelectedSubDept (SubDepartmentId)
            SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(T.C.value('.', 'varchar(30)'))) AS BIGINT)
            FROM @SubDeptXml.nodes('/x/i') AS T(C)
            WHERE TRY_CAST(LTRIM(RTRIM(T.C.value('.', 'varchar(30)'))) AS BIGINT) IS NOT NULL
              AND EXISTS (
                    SELECT 1
                    FROM SubDepartment SD
                    WHERE SD.Id = TRY_CAST(LTRIM(RTRIM(T.C.value('.', 'varchar(30)'))) AS BIGINT)
              );
        END

        IF NOT EXISTS (SELECT 1 FROM @SelectedSubDept)
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = 'At least one valid sub department is required';
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM DoctorMaster WHERE UserId = @UserId)
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = 'Selected user already configured as doctor';
            RETURN;
        END

        BEGIN TRY
            BEGIN TRANSACTION;

            INSERT INTO DoctorMaster (UserId, Designation, RegistrationNumber, SignatureImage, SignatureMimeType, Active)
            VALUES (@UserId, @Designation, @RegistrationNumber, @SignatureImage, @SignatureMimeType, ISNULL(@Active, 1));

            SET @Id = SCOPE_IDENTITY();

            INSERT INTO DoctorSubDepartment (DoctorId, SubDepartmentId)
            SELECT @Id, SubDepartmentId
            FROM @SelectedSubDept;

            COMMIT TRANSACTION;
            SELECT @StatusCode = 1, @StatusMsg = 'Doctor configured successfully';
        END TRY
        BEGIN CATCH
            IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
            SELECT @StatusCode = 0, @StatusMsg = ERROR_MESSAGE();
        END CATCH
        RETURN;
    END

    IF @Action = 'Update'
    BEGIN
        DECLARE @SelectedSubDeptUpd TABLE (SubDepartmentId BIGINT PRIMARY KEY);
        IF ISNULL(@SubDepartmentIds, '') <> ''
        BEGIN
            DECLARE @SubDeptXmlUpd XML;
            SET @SubDeptXmlUpd = CAST('<x><i>' + REPLACE(REPLACE(@SubDepartmentIds, '&', '&amp;'), ',', '</i><i>') + '</i></x>' AS XML);

            INSERT INTO @SelectedSubDeptUpd (SubDepartmentId)
            SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(T.C.value('.', 'varchar(30)'))) AS BIGINT)
            FROM @SubDeptXmlUpd.nodes('/x/i') AS T(C)
            WHERE TRY_CAST(LTRIM(RTRIM(T.C.value('.', 'varchar(30)'))) AS BIGINT) IS NOT NULL
              AND EXISTS (
                    SELECT 1
                    FROM SubDepartment SD
                    WHERE SD.Id = TRY_CAST(LTRIM(RTRIM(T.C.value('.', 'varchar(30)'))) AS BIGINT)
              );
        END

        IF NOT EXISTS (SELECT 1 FROM @SelectedSubDeptUpd)
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = 'At least one valid sub department is required';
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM DoctorMaster WHERE UserId = @UserId AND Id <> @Id)
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = 'Selected user already configured as doctor';
            RETURN;
        END

        BEGIN TRY
            BEGIN TRANSACTION;

            UPDATE DoctorMaster
            SET UserId = @UserId,
                Designation = @Designation,
                RegistrationNumber = @RegistrationNumber,
                SignatureImage = CASE WHEN @SignatureImage IS NULL THEN SignatureImage ELSE @SignatureImage END,
                SignatureMimeType = CASE WHEN @SignatureImage IS NULL THEN SignatureMimeType ELSE @SignatureMimeType END,
                Active = ISNULL(@Active, Active)
            WHERE Id = @Id;

            DELETE FROM DoctorSubDepartment WHERE DoctorId = @Id;

            INSERT INTO DoctorSubDepartment (DoctorId, SubDepartmentId)
            SELECT @Id, SubDepartmentId
            FROM @SelectedSubDeptUpd;

            COMMIT TRANSACTION;
            SELECT @StatusCode = 1, @StatusMsg = 'Doctor updated successfully';
        END TRY
        BEGIN CATCH
            IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
            SELECT @StatusCode = 0, @StatusMsg = ERROR_MESSAGE();
        END CATCH
        RETURN;
    END

    IF @Action = 'SetActive'
    BEGIN
        UPDATE DoctorMaster SET Active = @Active WHERE Id = @Id;
        SELECT @StatusCode = 1, @StatusMsg = CASE WHEN @Active = 1 THEN 'Doctor activated successfully' ELSE 'Doctor deactivated successfully' END;
        RETURN;
    END

    IF @Action = 'GetList'
    BEGIN
        SELECT
            D.Id,
            D.UserId,
            U.Username AS UserName,
            LTRIM(RTRIM(ISNULL(U.FirstName, '') + ' ' + ISNULL(U.LastName, ''))) AS FullName,
            D.Designation,
            D.RegistrationNumber,
            STUFF((
                SELECT ', ' + SD.SubDeptName
                FROM DoctorSubDepartment DS
                INNER JOIN SubDepartment SD ON SD.Id = DS.SubDepartmentId
                WHERE DS.DoctorId = D.Id
                FOR XML PATH(''), TYPE
            ).value('.', 'nvarchar(max)'), 1, 2, '') AS SubDepartments,
            ISNULL(D.Active, 0) AS Active,
            CASE WHEN D.SignatureImage IS NULL THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END AS HasSignature
        FROM DoctorMaster D
        INNER JOIN Users U ON U.UserId = D.UserId
        ORDER BY D.Id DESC;
        RETURN;
    END

    IF @Action = 'GetById'
    BEGIN
        SELECT
            D.Id,
            D.UserId,
            U.Username AS UserName,
            LTRIM(RTRIM(ISNULL(U.FirstName, '') + ' ' + ISNULL(U.LastName, ''))) AS FullName,
            D.Designation,
            D.RegistrationNumber,
            STUFF((
                SELECT ',' + CAST(DS.SubDepartmentId AS VARCHAR(20))
                FROM DoctorSubDepartment DS
                WHERE DS.DoctorId = D.Id
                FOR XML PATH(''), TYPE
            ).value('.', 'nvarchar(max)'), 1, 1, '') AS SubDepartmentIds,
            ISNULL(D.Active, 0) AS Active,
            CASE WHEN D.SignatureImage IS NULL THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END AS HasSignature
        FROM DoctorMaster D
        INNER JOIN Users U ON U.UserId = D.UserId
        WHERE D.Id = @Id;
        RETURN;
    END

    IF @Action = 'GetUsers'
    BEGIN
        SELECT
            U.UserId,
            U.Username AS UserName,
            LTRIM(RTRIM(ISNULL(U.FirstName, '') + ' ' + ISNULL(U.LastName, ''))) AS FullName
        FROM Users U
        WHERE ISNULL(U.IsActive, 1) = 1
          AND ISNULL(U.IsLocked, 0) = 0
        ORDER BY U.Username;
        RETURN;
    END

    IF @Action = 'GetSignature'
    BEGIN
        SELECT TOP 1 SignatureImage, SignatureMimeType
        FROM DoctorMaster
        WHERE Id = @Id;
        RETURN;
    END
END
GO
