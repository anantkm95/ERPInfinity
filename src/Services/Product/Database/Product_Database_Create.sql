-- ========================================================================================
-- ERPInfinity - Product Service Database Creation Script (SQL Server 2022)
-- Database: Db_Product
-- Description: Creates Tables, Stored Procedures, Views, Indexes, and Outbox table.
-- ========================================================================================

USE [Db_Product];
GO

SET NOCOUNT ON;

-- ========================================================================================
-- 1. TABLES
-- ========================================================================================

-- Categories Table
IF OBJECT_ID('dbo.Categories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ParentCategoryId INT NULL FOREIGN KEY REFERENCES dbo.Categories(Id),
        CategoryCode VARCHAR(20) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        IsActive BIT NOT NULL DEFAULT(1)
    );
    CREATE UNIQUE INDEX IX_Categories_CategoryCode ON dbo.Categories(CategoryCode);
    PRINT '✓ Created Table dbo.Categories';
END
GO

-- Brands Table
IF OBJECT_ID('dbo.Brands', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Brands (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Manufacturer NVARCHAR(150) NULL,
        IsActive BIT NOT NULL DEFAULT(1)
    );
    PRINT '✓ Created Table dbo.Brands';
END
GO

-- Products Table
IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ProductCode VARCHAR(30) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        BrandId INT NOT NULL FOREIGN KEY REFERENCES dbo.Brands(Id),
        CategoryId INT NOT NULL FOREIGN KEY REFERENCES dbo.Categories(Id),
        HSNCode VARCHAR(20) NOT NULL,
        TaxPercentage DECIMAL(5,2) NOT NULL DEFAULT(0.00),
        IsActive BIT NOT NULL DEFAULT(1),
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
    );
    CREATE UNIQUE INDEX IX_Products_ProductCode ON dbo.Products(ProductCode);
    CREATE NONCLUSTERED INDEX IX_Products_CategoryId ON dbo.Products(CategoryId);
    CREATE NONCLUSTERED INDEX IX_Products_BrandId ON dbo.Products(BrandId);
    CREATE NONCLUSTERED INDEX IX_Products_IsActive_Name ON dbo.Products(IsActive, Name);
    PRINT '✓ Created Table dbo.Products';
END
GO

-- ProductSKUs Table
IF OBJECT_ID('dbo.ProductSKUs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductSKUs (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ProductId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.Products(Id),
        SKUCode VARCHAR(50) NOT NULL,
        UnitOfMeasure VARCHAR(20) NOT NULL DEFAULT('PCS'),
        PackSize DECIMAL(10,2) NOT NULL DEFAULT(1.00),
        WeightKg DECIMAL(10,3) NOT NULL DEFAULT(0.000),
        MRP DECIMAL(18,2) NOT NULL DEFAULT(0.00),
        SellingPrice DECIMAL(18,2) NOT NULL DEFAULT(0.00)
    );
    CREATE UNIQUE INDEX IX_ProductSKUs_SKUCode ON dbo.ProductSKUs(SKUCode);
    CREATE NONCLUSTERED INDEX IX_ProductSKUs_ProductId ON dbo.ProductSKUs(ProductId);
    PRINT '✓ Created Table dbo.ProductSKUs';
END
GO

-- Barcodes Table
IF OBJECT_ID('dbo.Barcodes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Barcodes (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        SKUId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.ProductSKUs(Id),
        BarcodeNumber VARCHAR(50) NOT NULL,
        IsPrimary BIT NOT NULL DEFAULT(1)
    );
    CREATE UNIQUE INDEX IX_Barcodes_BarcodeNumber ON dbo.Barcodes(BarcodeNumber);
    PRINT '✓ Created Table dbo.Barcodes';
END
GO

-- ProductOutbox Table (Transactional Outbox Pattern)
IF OBJECT_ID('dbo.ProductOutbox', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductOutbox (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        EventType VARCHAR(100) NOT NULL,
        Payload NVARCHAR(MAX) NOT NULL,
        ProcessedAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
    );
    CREATE NONCLUSTERED INDEX IX_ProductOutbox_ProcessedAt_CreatedAt ON dbo.ProductOutbox(ProcessedAt, CreatedAt);
    PRINT '✓ Created Table dbo.ProductOutbox';
END
GO

-- ========================================================================================
-- 2. VIEWS
-- ========================================================================================

-- vw_ActivePOSBarcodes: High speed scanning view for POS systems
CREATE OR ALTER VIEW dbo.vw_ActivePOSBarcodes
AS
SELECT 
    b.BarcodeNumber,
    s.SKUCode,
    p.Id AS ProductId,
    p.Name AS ProductName,
    p.HSNCode,
    p.TaxPercentage,
    s.MRP,
    s.SellingPrice,
    b.IsPrimary
FROM dbo.Barcodes b
INNER JOIN dbo.ProductSKUs s ON b.SKUId = s.Id
INNER JOIN dbo.Products p ON s.ProductId = p.Id
WHERE p.IsActive = 1;
GO
PRINT '✓ Created View dbo.vw_ActivePOSBarcodes';
GO

-- vw_ProductCatalogDetail: Full product catalog view for exports and MongoDB sync
CREATE OR ALTER VIEW dbo.vw_ProductCatalogDetail
AS
SELECT 
    p.Id AS ProductId,
    p.ProductCode,
    p.Name AS ProductName,
    b.Name AS BrandName,
    c.Name AS CategoryName,
    p.HSNCode,
    p.TaxPercentage,
    s.Id AS SKUId,
    s.SKUCode,
    s.UnitOfMeasure,
    s.PackSize,
    s.MRP,
    s.SellingPrice,
    bc.BarcodeNumber
FROM dbo.Products p
INNER JOIN dbo.Brands b ON p.BrandId = b.Id
INNER JOIN dbo.Categories c ON p.CategoryId = c.Id
INNER JOIN dbo.ProductSKUs s ON s.ProductId = p.Id
LEFT JOIN dbo.Barcodes bc ON bc.SKUId = s.Id;
GO
PRINT '✓ Created View dbo.vw_ProductCatalogDetail';
GO

-- ========================================================================================
-- 3. STORED PROCEDURES
-- ========================================================================================

-- 1. sp_LookupProductByBarcode (Execution < 2ms for POS Barcode Scan)
CREATE OR ALTER PROCEDURE dbo.sp_LookupProductByBarcode
    @BarcodeNumber VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        p.Id AS ProductId,
        s.Id AS SKUId,
        s.SKUCode,
        p.Name AS ProductName,
        s.MRP,
        s.SellingPrice,
        p.TaxPercentage,
        p.HSNCode,
        b.BarcodeNumber
    FROM dbo.Barcodes b WITH(NOLOCK)
    INNER JOIN dbo.ProductSKUs s WITH(NOLOCK) ON b.SKUId = s.Id
    INNER JOIN dbo.Products p WITH(NOLOCK) ON s.ProductId = p.Id
    WHERE b.BarcodeNumber = @BarcodeNumber AND p.IsActive = 1;
END;
GO
PRINT '✓ Created Stored Procedure dbo.sp_LookupProductByBarcode';
GO

-- 2. sp_UpdateSKUPricing (Updates pricing and writes to ProductOutbox)
CREATE OR ALTER PROCEDURE dbo.sp_UpdateSKUPricing
    @SKUId UNIQUEIDENTIFIER,
    @NewMRP DECIMAL(18,2),
    @NewSellingPrice DECIMAL(18,2),
    @UpdatedBy UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        UPDATE dbo.ProductSKUs
        SET MRP = @NewMRP,
            SellingPrice = @NewSellingPrice
        WHERE Id = @SKUId;

        -- Write event to Outbox for RabbitMQ sync
        INSERT INTO dbo.ProductOutbox (Id, EventType, Payload, CreatedAt)
        VALUES (
            NEWID(),
            'ProductPriceUpdatedEvent',
            CONCAT('{"SKUId":"', @SKUId, '","NewMRP":', @NewMRP, ',"NewSellingPrice":', @NewSellingPrice, ',"UpdatedBy":"', @UpdatedBy, '"}'),
            GETUTCDATE()
        );

        COMMIT TRANSACTION;
        PRINT '✓ SKU Pricing Updated & Outbox Event Emitted.';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
PRINT '✓ Created Stored Procedure dbo.sp_UpdateSKUPricing';
GO

-- 3. sp_GetProductOutboxPendingMessages (Fetches unprocessed outbox items)
CREATE OR ALTER PROCEDURE dbo.sp_GetProductOutboxPendingMessages
    @BatchSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@BatchSize)
        Id,
        EventType,
        Payload,
        CreatedAt
    FROM dbo.ProductOutbox WITH(NOLOCK)
    WHERE ProcessedAt IS NULL
    ORDER BY CreatedAt ASC;
END;
GO
PRINT '✓ Created Stored Procedure dbo.sp_GetProductOutboxPendingMessages';
GO

PRINT '========================================================================================';
PRINT 'Product Database (Db_Product) Schema & Procedures Execution Complete!';
PRINT '========================================================================================';
GO
