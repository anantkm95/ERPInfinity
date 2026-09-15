-- ========================================================================================
-- ERPInfinity - Sales & POS Service Database Creation Script (SQL Server 2022)
-- Database: Db_Sales
-- Description: POS Counters, Invoicing, Billing Items, Returns & Outbox.
-- ========================================================================================

USE [Db_Sales];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

-- ========================================================================================
-- 1. TABLES
-- ========================================================================================

-- POSRegisters Table
IF OBJECT_ID('dbo.POSRegisters', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.POSRegisters (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        StoreId UNIQUEIDENTIFIER NOT NULL,
        TerminalCode VARCHAR(20) NOT NULL,
        CashierId UNIQUEIDENTIFIER NOT NULL,
        OpenedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
        ClosedAt DATETIME2 NULL,
        OpeningBalance DECIMAL(18,2) NOT NULL DEFAULT(0.00),
        ClosingCashCollected DECIMAL(18,2) NULL,
        ClosingCardCollected DECIMAL(18,2) NULL,
        ClosingUPICollected DECIMAL(18,2) NULL,
        Status VARCHAR(20) NOT NULL DEFAULT('OPEN') -- 'OPEN', 'CLOSED'
    );
    CREATE NONCLUSTERED INDEX IX_POSRegisters_StoreId_TerminalCode ON dbo.POSRegisters(StoreId, TerminalCode);
    PRINT '✓ Created Table dbo.POSRegisters';
END
GO

-- SalesInvoices Table
IF OBJECT_ID('dbo.SalesInvoices', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesInvoices (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        InvoiceNumber VARCHAR(50) NOT NULL,
        RegisterId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.POSRegisters(Id),
        StoreId UNIQUEIDENTIFIER NOT NULL,
        CustomerId UNIQUEIDENTIFIER NULL,
        SubTotal DECIMAL(18,2) NOT NULL,
        DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT(0.00),
        TaxAmount DECIMAL(18,2) NOT NULL DEFAULT(0.00),
        TotalAmount DECIMAL(18,2) NOT NULL,
        PaymentMode VARCHAR(30) NOT NULL DEFAULT('CASH'), -- 'CASH', 'CARD', 'UPI', 'SPLIT'
        Status VARCHAR(20) NOT NULL DEFAULT('COMPLETED'), -- 'COMPLETED', 'CANCELLED', 'REFUNDED'
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
    );
    CREATE UNIQUE INDEX IX_SalesInvoices_InvoiceNumber ON dbo.SalesInvoices(InvoiceNumber);
    CREATE NONCLUSTERED INDEX IX_SalesInvoices_StoreId_CreatedAt ON dbo.SalesInvoices(StoreId, CreatedAt);
    PRINT '✓ Created Table dbo.SalesInvoices';
END
GO

-- SalesInvoiceItems Table
IF OBJECT_ID('dbo.SalesInvoiceItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesInvoiceItems (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        InvoiceId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.SalesInvoices(Id),
        SKUId UNIQUEIDENTIFIER NOT NULL,
        ProductName NVARCHAR(200) NOT NULL,
        Quantity DECIMAL(18,3) NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        TaxPercentage DECIMAL(5,2) NOT NULL,
        LineTotal DECIMAL(18,2) NOT NULL
    );
    CREATE NONCLUSTERED INDEX IX_SalesInvoiceItems_InvoiceId ON dbo.SalesInvoiceItems(InvoiceId);
    CREATE NONCLUSTERED INDEX IX_SalesInvoiceItems_SKUId ON dbo.SalesInvoiceItems(SKUId);
    PRINT '✓ Created Table dbo.SalesInvoiceItems';
END
GO

-- SalesReturns Table
IF OBJECT_ID('dbo.SalesReturns', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesReturns (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ReturnNumber VARCHAR(50) NOT NULL,
        OriginalInvoiceId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.SalesInvoices(Id),
        RefundAmount DECIMAL(18,2) NOT NULL,
        Reason NVARCHAR(200) NOT NULL,
        ProcessedBy UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
    );
    CREATE UNIQUE INDEX IX_SalesReturns_ReturnNumber ON dbo.SalesReturns(ReturnNumber);
    CREATE NONCLUSTERED INDEX IX_SalesReturns_OriginalInvoiceId ON dbo.SalesReturns(OriginalInvoiceId);
    PRINT '✓ Created Table dbo.SalesReturns';
END
GO

-- SalesOutbox Table
IF OBJECT_ID('dbo.SalesOutbox', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesOutbox (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        EventType VARCHAR(100) NOT NULL,
        Payload NVARCHAR(MAX) NOT NULL,
        ProcessedAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
    );
    CREATE NONCLUSTERED INDEX IX_SalesOutbox_ProcessedAt ON dbo.SalesOutbox(ProcessedAt, CreatedAt);
    PRINT '✓ Created Table dbo.SalesOutbox';
END
GO

-- ========================================================================================
-- 2. VIEWS
-- ========================================================================================

-- vw_DailyStoreSalesSummary
CREATE OR ALTER VIEW dbo.vw_DailyStoreSalesSummary
AS
SELECT 
    StoreId,
    CAST(CreatedAt AS DATE) AS SalesDate,
    COUNT(Id) AS TotalInvoicesCount,
    SUM(TotalAmount) AS TotalRevenue,
    SUM(TaxAmount) AS TotalTaxCollected,
    SUM(DiscountAmount) AS TotalDiscountsGiven
FROM dbo.SalesInvoices
WHERE Status = 'COMPLETED'
GROUP BY StoreId, CAST(CreatedAt AS DATE);
GO
PRINT '✓ Created View dbo.vw_DailyStoreSalesSummary';
GO

-- ========================================================================================
-- 3. STORED PROCEDURES
-- ========================================================================================

-- 1. sp_OpenPOSRegister
CREATE OR ALTER PROCEDURE dbo.sp_OpenPOSRegister
    @StoreId UNIQUEIDENTIFIER,
    @TerminalCode VARCHAR(20),
    @CashierId UNIQUEIDENTIFIER,
    @OpeningBalance DECIMAL(18,2),
    @RegisterId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @RegisterId = NEWID();

    INSERT INTO dbo.POSRegisters (Id, StoreId, TerminalCode, CashierId, OpeningBalance, Status)
    VALUES (@RegisterId, @StoreId, @TerminalCode, @CashierId, @OpeningBalance, 'OPEN');

    PRINT '✓ POS Register Session Opened.';
END;
GO
PRINT '✓ Created Stored Procedure dbo.sp_OpenPOSRegister';
GO

-- 2. sp_CreateSalesInvoice
CREATE OR ALTER PROCEDURE dbo.sp_CreateSalesInvoice
    @RegisterId UNIQUEIDENTIFIER,
    @StoreId UNIQUEIDENTIFIER,
    @CustomerId UNIQUEIDENTIFIER = NULL,
    @SubTotal DECIMAL(18,2),
    @DiscountAmount DECIMAL(18,2),
    @TaxAmount DECIMAL(18,2),
    @TotalAmount DECIMAL(18,2),
    @PaymentMode VARCHAR(30),
    @InvoiceId UNIQUEIDENTIFIER OUTPUT,
    @InvoiceNumber VARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        SET @InvoiceId = NEWID();
        SET @InvoiceNumber = CONCAT('INV-', FORMAT(GETUTCDATE(), 'yyyyMMdd'), '-', ABS(CHECKSUM(NEWID())) % 899999 + 100000);

        INSERT INTO dbo.SalesInvoices (Id, InvoiceNumber, RegisterId, StoreId, CustomerId, SubTotal, DiscountAmount, TaxAmount, TotalAmount, PaymentMode)
        VALUES (@InvoiceId, @InvoiceNumber, @RegisterId, @StoreId, @CustomerId, @SubTotal, @DiscountAmount, @TaxAmount, @TotalAmount, @PaymentMode);

        -- Write event to Outbox for Inventory & Analytics sync
        INSERT INTO dbo.SalesOutbox (Id, EventType, Payload, CreatedAt)
        VALUES (
            NEWID(),
            'SalesInvoiceCreatedEvent',
            CONCAT('{"InvoiceId":"', @InvoiceId, '","InvoiceNumber":"', @InvoiceNumber, '","StoreId":"', @StoreId, '","TotalAmount":', @TotalAmount, '}'),
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
PRINT '✓ Created Stored Procedure dbo.sp_CreateSalesInvoice';
GO

PRINT '========================================================================================';
PRINT 'Sales Database (Db_Sales) Schema & Procedures Execution Complete!';
PRINT '========================================================================================';
GO
