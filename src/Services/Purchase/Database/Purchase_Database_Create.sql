-- ========================================================================================
-- ERPInfinity - Purchase & Procurement Service Database Creation Script (SQL Server 2022)
-- Database: Db_Purchase
-- Description: Vendors, Purchase Orders, Goods Received Notes (GRN) & Outbox.
-- ========================================================================================

USE [Db_Purchase];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

-- ========================================================================================
-- 1. TABLES
-- ========================================================================================

-- Suppliers Table
IF OBJECT_ID('dbo.Suppliers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Suppliers (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        VendorCode VARCHAR(30) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        GSTIN VARCHAR(15) NULL,
        Email VARCHAR(150) NULL,
        Phone VARCHAR(20) NULL,
        CreditDays INT NOT NULL DEFAULT(30),
        IsActive BIT NOT NULL DEFAULT(1)
    );
    CREATE UNIQUE INDEX IX_Suppliers_VendorCode ON dbo.Suppliers(VendorCode);
    PRINT '✓ Created Table dbo.Suppliers';
END
GO

-- PurchaseOrders Table
IF OBJECT_ID('dbo.PurchaseOrders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PurchaseOrders (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        PONumber VARCHAR(50) NOT NULL,
        SupplierId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.Suppliers(Id),
        WarehouseId UNIQUEIDENTIFIER NOT NULL,
        Status VARCHAR(30) NOT NULL DEFAULT('Draft'), -- 'Draft', 'Submitted', 'Approved', 'PartiallyReceived', 'Completed', 'Cancelled'
        TotalAmount DECIMAL(18,2) NOT NULL DEFAULT(0.00),
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        ApprovedBy UNIQUEIDENTIFIER NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
    );
    CREATE UNIQUE INDEX IX_PurchaseOrders_PONumber ON dbo.PurchaseOrders(PONumber);
    CREATE NONCLUSTERED INDEX IX_PurchaseOrders_SupplierId ON dbo.PurchaseOrders(SupplierId);
    CREATE NONCLUSTERED INDEX IX_PurchaseOrders_Status ON dbo.PurchaseOrders(Status);
    PRINT '✓ Created Table dbo.PurchaseOrders';
END
GO

-- PurchaseOrderItems Table
IF OBJECT_ID('dbo.PurchaseOrderItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PurchaseOrderItems (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        POId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.PurchaseOrders(Id),
        SKUId UNIQUEIDENTIFIER NOT NULL,
        OrderedQuantity DECIMAL(18,3) NOT NULL,
        ReceivedQuantity DECIMAL(18,3) NOT NULL DEFAULT(0.000),
        UnitPrice DECIMAL(18,2) NOT NULL,
        Total DECIMAL(18,2) NOT NULL
    );
    CREATE NONCLUSTERED INDEX IX_PurchaseOrderItems_POId ON dbo.PurchaseOrderItems(POId);
    PRINT '✓ Created Table dbo.PurchaseOrderItems';
END
GO

-- GoodsReceivedNotes (GRN) Table
IF OBJECT_ID('dbo.GoodsReceivedNotes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.GoodsReceivedNotes (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        GRNNumber VARCHAR(50) NOT NULL,
        POId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.PurchaseOrders(Id),
        WarehouseId UNIQUEIDENTIFIER NOT NULL,
        ChallanNumber VARCHAR(50) NULL,
        VehicleNumber VARCHAR(30) NULL,
        ReceivedDate DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
        ReceivedBy UNIQUEIDENTIFIER NOT NULL
    );
    CREATE UNIQUE INDEX IX_GoodsReceivedNotes_GRNNumber ON dbo.GoodsReceivedNotes(GRNNumber);
    CREATE NONCLUSTERED INDEX IX_GoodsReceivedNotes_POId ON dbo.GoodsReceivedNotes(POId);
    PRINT '✓ Created Table dbo.GoodsReceivedNotes';
END
GO

-- GRNItems Table
IF OBJECT_ID('dbo.GRNItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.GRNItems (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        GRNId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.GoodsReceivedNotes(Id),
        SKUId UNIQUEIDENTIFIER NOT NULL,
        AcceptedQty DECIMAL(18,3) NOT NULL,
        RejectedQty DECIMAL(18,3) NOT NULL DEFAULT(0.000),
        UnitPrice DECIMAL(18,2) NOT NULL
    );
    CREATE NONCLUSTERED INDEX IX_GRNItems_GRNId ON dbo.GRNItems(GRNId);
    PRINT '✓ Created Table dbo.GRNItems';
END
GO

-- PurchaseOutbox Table
IF OBJECT_ID('dbo.PurchaseOutbox', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PurchaseOutbox (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        EventType VARCHAR(100) NOT NULL,
        Payload NVARCHAR(MAX) NOT NULL,
        ProcessedAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
    );
    CREATE NONCLUSTERED INDEX IX_PurchaseOutbox_ProcessedAt ON dbo.PurchaseOutbox(ProcessedAt, CreatedAt);
    PRINT '✓ Created Table dbo.PurchaseOutbox';
END
GO

-- ========================================================================================
-- 2. VIEWS
-- ========================================================================================

-- vw_POFulfillmentStatus
CREATE OR ALTER VIEW dbo.vw_POFulfillmentStatus
AS
SELECT 
    po.Id AS POId,
    po.PONumber,
    s.Name AS SupplierName,
    po.Status,
    SUM(poi.OrderedQuantity) AS TotalOrderedQty,
    SUM(poi.ReceivedQuantity) AS TotalReceivedQty,
    po.CreatedAt
FROM dbo.PurchaseOrders po
INNER JOIN dbo.Suppliers s ON po.SupplierId = s.Id
INNER JOIN dbo.PurchaseOrderItems poi ON poi.POId = po.Id
GROUP BY po.Id, po.PONumber, s.Name, po.Status, po.CreatedAt;
GO
PRINT '✓ Created View dbo.vw_POFulfillmentStatus';
GO

-- ========================================================================================
-- 3. STORED PROCEDURES
-- ========================================================================================

-- 1. sp_CreatePurchaseOrder
CREATE OR ALTER PROCEDURE dbo.sp_CreatePurchaseOrder
    @SupplierId UNIQUEIDENTIFIER,
    @WarehouseId UNIQUEIDENTIFIER,
    @TotalAmount DECIMAL(18,2),
    @CreatedBy UNIQUEIDENTIFIER,
    @POId UNIQUEIDENTIFIER OUTPUT,
    @PONumber VARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @POId = NEWID();
    SET @PONumber = CONCAT('PO-', FORMAT(GETUTCDATE(), 'yyyyMMdd'), '-', ABS(CHECKSUM(NEWID())) % 89999 + 10000);

    INSERT INTO dbo.PurchaseOrders (Id, PONumber, SupplierId, WarehouseId, Status, TotalAmount, CreatedBy)
    VALUES (@POId, @PONumber, @SupplierId, @WarehouseId, 'Draft', @TotalAmount, @CreatedBy);

    PRINT '✓ Purchase Order Created.';
END;
GO
PRINT '✓ Created Stored Procedure dbo.sp_CreatePurchaseOrder';
GO

-- 2. sp_ApprovePurchaseOrder
CREATE OR ALTER PROCEDURE dbo.sp_ApprovePurchaseOrder
    @POId UNIQUEIDENTIFIER,
    @ApprovedBy UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        UPDATE dbo.PurchaseOrders
        SET Status = 'Approved',
            ApprovedBy = @ApprovedBy
        WHERE Id = @POId;

        INSERT INTO dbo.PurchaseOutbox (Id, EventType, Payload, CreatedAt)
        VALUES (
            NEWID(),
            'PurchaseOrderApprovedEvent',
            CONCAT('{"POId":"', @POId, '","ApprovedBy":"', @ApprovedBy, '"}'),
            GETUTCDATE()
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
PRINT '✓ Created Stored Procedure dbo.sp_ApprovePurchaseOrder';
GO

PRINT '========================================================================================';
PRINT 'Purchase Database (Db_Purchase) Schema & Procedures Execution Complete!';
PRINT '========================================================================================';
GO
