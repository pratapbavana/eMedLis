USE [eMedLis]
GO

IF COL_LENGTH('dbo.BillDetail', 'IsFromPackage') IS NULL
BEGIN
    ALTER TABLE dbo.BillDetail ADD IsFromPackage BIT NOT NULL CONSTRAINT DF_BillDetail_IsFromPackage DEFAULT ((0))
END
GO

IF COL_LENGTH('dbo.BillDetail', 'PackageId') IS NULL
BEGIN
    ALTER TABLE dbo.BillDetail ADD PackageId INT NULL
END
GO

IF COL_LENGTH('dbo.BillDetail', 'ParentPackageCode') IS NULL
BEGIN
    ALTER TABLE dbo.BillDetail ADD ParentPackageCode VARCHAR(30) NULL
END
GO

IF COL_LENGTH('dbo.BillDetail', 'ParentPackageName') IS NULL
BEGIN
    ALTER TABLE dbo.BillDetail ADD ParentPackageName NVARCHAR(200) NULL
END
GO

IF COL_LENGTH('dbo.BillDetail', 'PackagePrice') IS NULL
BEGIN
    ALTER TABLE dbo.BillDetail ADD PackagePrice DECIMAL(18,2) NOT NULL CONSTRAINT DF_BillDetail_PackagePrice DEFAULT ((0))
END
GO

IF COL_LENGTH('dbo.BillDetail', 'IsPackageChargeOwner') IS NULL
BEGIN
    ALTER TABLE dbo.BillDetail ADD IsPackageChargeOwner BIT NOT NULL CONSTRAINT DF_BillDetail_IsPackageChargeOwner DEFAULT ((0))
END
GO

IF OBJECT_ID('[dbo].[usp_InsertBillDetail]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[usp_InsertBillDetail]
GO

CREATE PROCEDURE [dbo].[usp_InsertBillDetail]
    @BillSummaryId INT,
    @InvId NVARCHAR(200),
    @InvName NVARCHAR(200),
    @Rate DECIMAL(18, 2),
    @DiscountAmount DECIMAL(18, 2),
    @DiscountPercent DECIMAL(18, 2),
    @NetAmount DECIMAL(18, 2),
    @IsFromPackage BIT = 0,
    @PackageId INT = NULL,
    @ParentPackageCode VARCHAR(30) = NULL,
    @ParentPackageName NVARCHAR(200) = NULL,
    @PackagePrice DECIMAL(18,2) = 0,
    @IsPackageChargeOwner BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO BillDetail
    (
        BillSummaryId, InvId, InvName, Rate, DiscountAmount, DiscountPercent, NetAmount,
        IsFromPackage, PackageId, ParentPackageCode, ParentPackageName, PackagePrice, IsPackageChargeOwner
    )
    VALUES
    (
        @BillSummaryId, @InvId, @InvName, @Rate, @DiscountAmount, @DiscountPercent, @NetAmount,
        @IsFromPackage, @PackageId, @ParentPackageCode, @ParentPackageName, @PackagePrice, @IsPackageChargeOwner
    );
END
GO

IF OBJECT_ID('[dbo].[usp_GetBillDetails]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[usp_GetBillDetails]
GO

CREATE PROCEDURE [dbo].[usp_GetBillDetails]
    @BillSummaryId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        InvId, InvName, Rate, DiscountAmount, DiscountPercent, NetAmount,
        ISNULL(IsFromPackage, 0) AS IsFromPackage,
        PackageId,
        ParentPackageCode,
        ParentPackageName,
        ISNULL(PackagePrice, 0) AS PackagePrice,
        ISNULL(IsPackageChargeOwner, 0) AS IsPackageChargeOwner
    FROM BillDetail
    WHERE BillSummaryId = @BillSummaryId
    ORDER BY BillDetailId;
END
GO
