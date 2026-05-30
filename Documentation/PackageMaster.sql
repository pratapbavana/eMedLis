USE [eMedLis]
GO

IF OBJECT_ID('[dbo].[PackageMaster]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PackageMaster](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [PackageCode] [varchar](30) NULL,
        [PackageName] [nvarchar](200) NOT NULL,
        [ReportingName] [nvarchar](200) NULL,
        [Price] [decimal](18,2) NOT NULL CONSTRAINT [DF_PackageMaster_Price] DEFAULT ((0)),
        [DiscountAmount] [decimal](18,2) NOT NULL CONSTRAINT [DF_PackageMaster_DiscountAmount] DEFAULT ((0)),
        [Description] [nvarchar](500) NULL,
        [Active] [bit] NOT NULL CONSTRAINT [DF_PackageMaster_Active] DEFAULT ((1)),
        [CreatedOn] [datetime] NOT NULL CONSTRAINT [DF_PackageMaster_CreatedOn] DEFAULT (GETDATE()),
        [UpdatedOn] [datetime] NULL,
        CONSTRAINT [PK_PackageMaster] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) ON [PRIMARY]
END
GO

IF OBJECT_ID('[dbo].[PackageDetail]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PackageDetail](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [PackageId] [int] NOT NULL,
        [InvestigationId] [varchar](50) NOT NULL,
        [DisplayOrder] [int] NOT NULL CONSTRAINT [DF_PackageDetail_DisplayOrder] DEFAULT ((0)),
        [Active] [bit] NOT NULL CONSTRAINT [DF_PackageDetail_Active] DEFAULT ((1)),
        CONSTRAINT [PK_PackageDetail] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) ON [PRIMARY]
END
GO

IF OBJECT_ID('[dbo].[PackageDetail]', 'U') IS NOT NULL AND OBJECT_ID('[dbo].[Investigations]', 'U') IS NOT NULL
BEGIN
    DECLARE @InvIdRefType SYSNAME;
    DECLARE @InvIdPkgType SYSNAME;
    DECLARE @InvIdRefMaxLen INT;
    SELECT @InvIdRefType = t.name
    FROM sys.columns c
    INNER JOIN sys.types t ON t.user_type_id = c.user_type_id
    WHERE c.object_id = OBJECT_ID('[dbo].[Investigations]')
      AND c.name = 'Id';

    SELECT @InvIdRefMaxLen = c.max_length
    FROM sys.columns c
    WHERE c.object_id = OBJECT_ID('[dbo].[Investigations]')
      AND c.name = 'Id';

    SELECT @InvIdPkgType = t.name
    FROM sys.columns c
    INNER JOIN sys.types t ON t.user_type_id = c.user_type_id
    WHERE c.object_id = OBJECT_ID('[dbo].[PackageDetail]')
      AND c.name = 'InvestigationId';

    IF @InvIdRefType IS NOT NULL AND @InvIdPkgType IS NOT NULL
    BEGIN
        IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PackageDetail_Investigations')
            ALTER TABLE [dbo].[PackageDetail] DROP CONSTRAINT [FK_PackageDetail_Investigations];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_PackageDetail_Package_Inv' AND object_id = OBJECT_ID('[dbo].[PackageDetail]'))
            DROP INDEX [UX_PackageDetail_Package_Inv] ON [dbo].[PackageDetail];

        DECLARE @Sql NVARCHAR(MAX);
        IF @InvIdRefType IN ('varchar', 'char', 'nvarchar', 'nchar')
        BEGIN
            DECLARE @TypeLength NVARCHAR(30);
            SET @TypeLength = CASE 
                WHEN @InvIdRefMaxLen = -1 THEN '(MAX)'
                WHEN @InvIdRefType IN ('nvarchar', 'nchar') THEN '(' + CAST(@InvIdRefMaxLen / 2 AS VARCHAR(10)) + ')'
                ELSE '(' + CAST(@InvIdRefMaxLen AS VARCHAR(10)) + ')'
            END;
            SET @Sql = N'ALTER TABLE [dbo].[PackageDetail] ALTER COLUMN [InvestigationId] ' + UPPER(@InvIdRefType) + @TypeLength + N' NOT NULL;';
        END
        ELSE
        BEGIN
            SET @Sql = N'ALTER TABLE [dbo].[PackageDetail] ALTER COLUMN [InvestigationId] ' + UPPER(@InvIdRefType) + N' NOT NULL;';
        END
        EXEC sp_executesql @Sql;
    END
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_PackageDetail_Package_Inv' AND object_id = OBJECT_ID('[dbo].[PackageDetail]'))
BEGIN
    CREATE UNIQUE INDEX [UX_PackageDetail_Package_Inv] ON [dbo].[PackageDetail]([PackageId], [InvestigationId])
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PackageDetail_PackageMaster')
BEGIN
    ALTER TABLE [dbo].[PackageDetail]  WITH CHECK ADD  CONSTRAINT [FK_PackageDetail_PackageMaster] FOREIGN KEY([PackageId])
    REFERENCES [dbo].[PackageMaster] ([Id])
    ALTER TABLE [dbo].[PackageDetail] CHECK CONSTRAINT [FK_PackageDetail_PackageMaster]
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PackageDetail_Investigations')
BEGIN
    ALTER TABLE [dbo].[PackageDetail]  WITH CHECK ADD  CONSTRAINT [FK_PackageDetail_Investigations] FOREIGN KEY([InvestigationId])
    REFERENCES [dbo].[Investigations] ([Id])
    ALTER TABLE [dbo].[PackageDetail] CHECK CONSTRAINT [FK_PackageDetail_Investigations]
END
GO

IF OBJECT_ID('[dbo].[Usp_PackageMaster]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[Usp_PackageMaster]
GO

CREATE PROCEDURE [dbo].[Usp_PackageMaster]
(
    @Id INT = NULL,
    @PackageCode VARCHAR(30) = NULL,
    @PackageName NVARCHAR(200) = NULL,
    @ReportingName NVARCHAR(200) = NULL,
    @Price DECIMAL(18,2) = NULL,
    @DiscountAmount DECIMAL(18,2) = NULL,
    @Description NVARCHAR(500) = NULL,
    @Active BIT = NULL,
    @InvestigationIds VARCHAR(MAX) = NULL,
    @Action VARCHAR(30),
    @StatusCode INT = 0 OUTPUT,
    @StatusMsg VARCHAR(200) = NULL OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'Add'
    BEGIN
        IF ISNULL(LTRIM(RTRIM(@PackageName)), '') = ''
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = 'Package Name is required';
            RETURN;
        END

        DECLARE @AddInv TABLE (InvestigationId VARCHAR(50) PRIMARY KEY, Seq INT IDENTITY(1,1));
        IF ISNULL(@InvestigationIds, '') <> ''
        BEGIN
            DECLARE @AddXml XML;
            SET @AddXml = CAST('<x><i>' + REPLACE(REPLACE(@InvestigationIds, '&', '&amp;'), ',', '</i><i>') + '</i></x>' AS XML);
            INSERT INTO @AddInv(InvestigationId)
            SELECT DISTINCT LTRIM(RTRIM(T.C.value('.', 'varchar(50)')))
            FROM @AddXml.nodes('/x/i') AS T(C)
            WHERE LTRIM(RTRIM(T.C.value('.', 'varchar(50)'))) <> ''
              AND EXISTS (SELECT 1 FROM Investigations I WHERE CAST(I.Id AS VARCHAR(50)) = LTRIM(RTRIM(T.C.value('.', 'varchar(50)'))));
        END

        IF NOT EXISTS (SELECT 1 FROM @AddInv)
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = 'Select at least one valid investigation';
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM PackageMaster WHERE PackageName = @PackageName)
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = 'Package Name already exists';
            RETURN;
        END

        BEGIN TRY
            BEGIN TRANSACTION;
            INSERT INTO PackageMaster(PackageCode, PackageName, ReportingName, Price, DiscountAmount, Description, Active, CreatedOn, UpdatedOn)
            VALUES(@PackageCode, @PackageName, @ReportingName, ISNULL(@Price,0), ISNULL(@DiscountAmount,0), @Description, ISNULL(@Active,1), GETDATE(), GETDATE());

            SET @Id = SCOPE_IDENTITY();

            INSERT INTO PackageDetail(PackageId, InvestigationId, DisplayOrder, Active)
            SELECT @Id, InvestigationId, Seq, 1
            FROM @AddInv
            ORDER BY Seq;

            COMMIT TRANSACTION;
            SELECT @StatusCode = 1, @StatusMsg = 'Package created successfully';
        END TRY
        BEGIN CATCH
            IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
            SELECT @StatusCode = 0, @StatusMsg = ERROR_MESSAGE();
        END CATCH
        RETURN;
    END

    IF @Action = 'Update'
    BEGIN
        IF @Id IS NULL OR @Id <= 0
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = 'Invalid package';
            RETURN;
        END

        DECLARE @UpdInv TABLE (InvestigationId VARCHAR(50) PRIMARY KEY, Seq INT IDENTITY(1,1));
        IF ISNULL(@InvestigationIds, '') <> ''
        BEGIN
            DECLARE @UpdXml XML;
            SET @UpdXml = CAST('<x><i>' + REPLACE(REPLACE(@InvestigationIds, '&', '&amp;'), ',', '</i><i>') + '</i></x>' AS XML);
            INSERT INTO @UpdInv(InvestigationId)
            SELECT DISTINCT LTRIM(RTRIM(T.C.value('.', 'varchar(50)')))
            FROM @UpdXml.nodes('/x/i') AS T(C)
            WHERE LTRIM(RTRIM(T.C.value('.', 'varchar(50)'))) <> ''
              AND EXISTS (SELECT 1 FROM Investigations I WHERE CAST(I.Id AS VARCHAR(50)) = LTRIM(RTRIM(T.C.value('.', 'varchar(50)'))));
        END

        IF NOT EXISTS (SELECT 1 FROM @UpdInv)
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = 'Select at least one valid investigation';
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM PackageMaster WHERE PackageName = @PackageName AND Id <> @Id)
        BEGIN
            SELECT @StatusCode = 0, @StatusMsg = 'Package Name already exists';
            RETURN;
        END

        BEGIN TRY
            BEGIN TRANSACTION;
            UPDATE PackageMaster
            SET PackageCode = @PackageCode,
                PackageName = @PackageName,
                ReportingName = @ReportingName,
                Price = ISNULL(@Price,0),
                DiscountAmount = ISNULL(@DiscountAmount,0),
                Description = @Description,
                Active = ISNULL(@Active, Active),
                UpdatedOn = GETDATE()
            WHERE Id = @Id;

            DELETE FROM PackageDetail WHERE PackageId = @Id;

            INSERT INTO PackageDetail(PackageId, InvestigationId, DisplayOrder, Active)
            SELECT @Id, InvestigationId, Seq, 1
            FROM @UpdInv
            ORDER BY Seq;

            COMMIT TRANSACTION;
            SELECT @StatusCode = 1, @StatusMsg = 'Package updated successfully';
        END TRY
        BEGIN CATCH
            IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
            SELECT @StatusCode = 0, @StatusMsg = ERROR_MESSAGE();
        END CATCH
        RETURN;
    END

    IF @Action = 'SetActive'
    BEGIN
        UPDATE PackageMaster SET Active = @Active, UpdatedOn = GETDATE() WHERE Id = @Id;
        SELECT @StatusCode = 1, @StatusMsg = CASE WHEN @Active = 1 THEN 'Package activated successfully' ELSE 'Package deactivated successfully' END;
        RETURN;
    END

    IF @Action = 'GetList'
    BEGIN
        SELECT
            PM.Id, PM.PackageCode, PM.PackageName, PM.ReportingName, PM.Price, PM.DiscountAmount, PM.Description, PM.Active,
            STUFF((
                SELECT ', ' + I.InvName
                FROM PackageDetail PD
                INNER JOIN Investigations I ON I.Id = PD.InvestigationId
                WHERE PD.PackageId = PM.Id
                ORDER BY PD.DisplayOrder
                FOR XML PATH(''), TYPE
            ).value('.', 'nvarchar(max)'), 1, 2, '') AS Investigations,
            (SELECT COUNT(*) FROM PackageDetail X WHERE X.PackageId = PM.Id) AS InvestigationCount
        FROM PackageMaster PM
        ORDER BY PM.Id DESC;
        RETURN;
    END

    IF @Action = 'GetActive'
    BEGIN
        SELECT
            PM.Id, PM.PackageCode, PM.PackageName, PM.ReportingName, PM.Price, PM.DiscountAmount, PM.Active
        FROM PackageMaster PM
        WHERE PM.Active = 1
        ORDER BY PM.PackageName;
        RETURN;
    END

    IF @Action = 'GetById'
    BEGIN
        SELECT
            PM.Id, PM.PackageCode, PM.PackageName, PM.ReportingName, PM.Price, PM.DiscountAmount, PM.Description, PM.Active,
            STUFF((
                SELECT ',' + CAST(PD.InvestigationId AS VARCHAR(50))
                FROM PackageDetail PD
                WHERE PD.PackageId = PM.Id
                ORDER BY PD.DisplayOrder
                FOR XML PATH(''), TYPE
            ).value('.', 'nvarchar(max)'), 1, 1, '') AS InvestigationIds
        FROM PackageMaster PM
        WHERE PM.Id = @Id;
        RETURN;
    END

    IF @Action = 'GetInvestigations'
    BEGIN
        SELECT
            CAST(I.Id AS VARCHAR(50)) AS Id,
            I.InvCode,
            I.InvName,
            I.Rate
        FROM PackageDetail PD
        INNER JOIN Investigations I ON I.Id = PD.InvestigationId
        INNER JOIN PackageMaster PM ON PM.Id = PD.PackageId
        WHERE PD.PackageId = @Id
          AND PM.Active = 1
          AND ISNULL(I.Active, 1) = 1
        ORDER BY PD.DisplayOrder, I.InvName;
        RETURN;
    END

    SELECT @StatusCode = 0, @StatusMsg = 'Invalid action';
END
GO
