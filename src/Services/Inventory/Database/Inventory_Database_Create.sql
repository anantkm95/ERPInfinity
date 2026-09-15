-- ========================================================================================
-- ERPInfinity - Inventory Service Database Creation Script (SQL Server 2022)
-- Database: Db_Inventory
-- Description: Creates Stock Ledger, Movement Transactions, Stock Adjustments, and Outbox.
-- ========================================================================================

USE [Db_Inventory];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

-- ========================================================================================
-- 1. TABLES
-- ========================================================================================

-- Locations / Warehouses Table
IF OBJECT_ID('dbo.Locations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Locations (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        LocationCode VARCHAR(30) NOT NULL,
        Name NVARCHAR(150) NOT NULL,
        Type VARCHAR(30) NOT NULL DEFAULT('Warehouse'), -- 'Store', 'Warehouse', 'FulfillmentCenter'
        IsActive BIT NOT NULL DEFAULT(1)
    );
    CREATE UNIQUE INDEX IX_Locations_LocationCode ON dbo.Locations(LocationCode);
    PRINT '✓ Created Table dbo.Locations';
END
GO

-- Stocks Table (Master Stock Balances)
IF OBJECT_ID('dbo.Stocks', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Stocks (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        LocationId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.Locations(Id),
        SKUId UNIQUEIDENTIFIER NOT NULL,
        OnHandQuantity DECIMAL(18,3) NOT NULL DEFAULT(0.000),
        ReservedQuantity DECIMAL(18,3) NOT NULL DEFAULT(0.000),
        AvailableQuantity AS (OnHandQuantity - ReservedQuantity),
        ReorderPoint DECIMAL(18,3) NOT NULL DEFAULT(10.000),
        LastUpdated DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
    );
    CREATE UNIQUE INDEX IX_Stocks_LocationId_SKUId ON dbo.Stocks(LocationId, SKUId);
    CREATE NONCLUSTERED INDEX IX_Stocks_AvailableQuantity ON dbo.Stocks(AvailableQuantity);
    PRINT '✓ Created Table dbo.Stocks';
END
GO

-- StockTransactions Table (Audit Ledger for all Movements)
IF OBJECT_ID('dbo.StockTransactions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockTransactions (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        LocationId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.Locations(Id),
        SKUId UNIQUEIDENTIFIER NOT NULL,
        TransactionType VARCHAR(50) NOT NULL, -- 'POS_DEDUCTION', 'GRN_RECEIPT', 'STOCK_ADJUSTMENT', 'RESERVATION'
        QuantityChange DECIMAL(18,3) NOT NULL,
        PreviousQuantity DECIMAL(18,3) NOT NULL,
        NewQuantity DECIMAL(18,3) NOT NULL,
        ReferenceDocumentNumber VARCHAR(100) NULL,
        CreatedBy UNIQUEIDENTIFIER NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
    );
    CREATE NONCLUSTERED INDEX IX_StockTransactions_LocationId_SKUId ON dbo.StockTransactions(LocationId, SKUId);
    CREATE NONCLUSTERED INDEX IX_StockTransactions_CreatedAt ON dbo.StockTransactions(CreatedAt);
    PRINT '✓ Created Table dbo.StockTransactions';
END
GO

-- StockAdjustments Table
IF OBJECT_ID('dbo.StockAdjustments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockAdjustments (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        LocationId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.Locations(Id),
        SKUId UNIQUEIDENTIFIER NOT NULL,
        AdjustmentReason NVARCHAR(200) NOT NULL, -- 'Damaged', 'Expired', 'Audit Discrepancy'
        QuantityAdjusted DECIMAL(18,3) NOT NULL,
        ApprovedBy UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
    );
    CREATE NONCLUSTERED INDEX IX_StockAdjustments_LocationId ON dbo.StockAdjustments(LocationId);
    PRINT '✓ Created Table dbo.StockAdjustments';
END
GO

-- InventoryOutbox Table
IF OBJECT_ID('dbo.InventoryOutbox', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.InventoryOutbox (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        EventType VARCHAR(100) NOT NULL,
        Payload NVARCHAR(MAX) NOT NULL,
        ProcessedAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
    );
    CREATE NONCLUSTERED INDEX IX_InventoryOutbox_ProcessedAt ON dbo.InventoryOutbox(ProcessedAt, CreatedAt);
    PRINT '✓ Created Table dbo.InventoryOutbox';
END
GO

-- ========================================================================================
-- 2. VIEWS
-- ========================================================================================

-- vw_LocationStockSummary
CREATE OR ALTER VIEW dbo.vw_LocationStockSummary
AS
SELECT 
    l.LocationCode,
    l.Name AS LocationName,
    s.SKUId,
    s.OnHandQuantity,
    s.ReservedQuantity,
    s.AvailableQuantity,
    s.ReorderPoint,
    s.LastUpdated
FROM dbo.Stocks s
INNER JOIN dbo.Locations l ON s.LocationId = l.Id;
GO
PRINT '✓ Created View dbo.vw_LocationStockSummary';
GO

-- vw_LowStockAlertQueue
CREATE OR ALTER VIEW dbo.vw_LowStockAlertQueue
AS
SELECT 
    l.LocationCode,
    l.Name AS LocationName,
    s.SKUId,
    s.AvailableQuantity,
    s.ReorderPoint
FROM dbo.Stocks s
INNER JOIN dbo.Locations l ON s.LocationId = l.Id
WHERE s.AvailableQuantity <= s.ReorderPoint;
GO
PRINT '✓ Created View dbo.vw_LowStockAlertQueue';
GO

-- ========================================================================================
-- 3. STORED PROCEDURES
-- ========================================================================================

-- 1. sp_DeductInventoryForPOS (High speed POS stock reduction)
CREATE OR ALTER PROCEDURE dbo.sp_DeductInventoryForPOS
    @LocationId UNIQUEIDENTIFIER,
    @SKUId UNIQUEIDENTIFIER,
    @Quantity DECIMAL(18,3),
    @InvoiceNumber VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @PrevQty DECIMAL(18,3), @NewQty DECIMAL(18,3);

        SELECT @PrevQty = OnHandQuantity 
        FROM dbo.Stocks WITH (UPDLOCK) 
        WHERE LocationId = @LocationId AND SKUId = @SKUId;

        IF @PrevQty IS NULL
        BEGIN
            RAISERROR('Stock record not found for Location & SKU.', 16, 1);
        END

        SET @NewQty = @PrevQty - @Quantity;

        UPDATE dbo.Stocks
        SET OnHandQuantity = @NewQty,
            LastUpdated = GETUTCDATE()
        WHERE LocationId = @LocationId AND SKUId = @SKUId;

        INSERT INTO dbo.StockTransactions (LocationId, SKUId, TransactionType, QuantityChange, PreviousQuantity, NewQuantity, ReferenceDocumentNumber)
        VALUES (@LocationId, @SKUId, 'POS_DEDUCTION', -@Quantity, @PrevQty, @NewQty, @InvoiceNumber);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
PRINT '✓ Created Stored Procedure dbo.sp_DeductInventoryForPOS';
GO

-- 2. sp_ReceiveGRNStock (Receive stock from Goods Received Note)
CREATE OR ALTER PROCEDURE dbo.sp_ReceiveGRNStock
    @LocationId UNIQUEIDENTIFIER,
    @SKUId UNIQUEIDENTIFIER,
    @ReceivedQuantity DECIMAL(18,3),
    @GRNNumber VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @PrevQty DECIMAL(18,3) = 0.000, @NewQty DECIMAL(18,3);

        IF EXISTS (SELECT 1 FROM dbo.Stocks WHERE LocationId = @LocationId AND SKUId = @SKUId)
        BEGIN
            SELECT @PrevQty = OnHandQuantity FROM dbo.Stocks WITH (UPDLOCK) WHERE LocationId = @LocationId AND SKUId = @SKUId;
            SET @NewQty = @PrevQty + @ReceivedQuantity;

            UPDATE dbo.Stocks
            SET OnHandQuantity = @NewQty,
                LastUpdated = GETUTCDATE()
            WHERE LocationId = @LocationId AND SKUId = @SKUId;
        END
        ELSE
        BEGIN
            SET @NewQty = @ReceivedQuantity;
            INSERT INTO dbo.Stocks (Id, LocationId, SKUId, OnHandQuantity, ReservedQuantity, ReorderPoint)
            VALUES (NEWID(), @LocationId, @SKUId, @NewQty, 0.000, 10.000);
        END

        INSERT INTO dbo.StockTransactions (LocationId, SKUId, TransactionType, QuantityChange, PreviousQuantity, NewQuantity, ReferenceDocumentNumber)
        VALUES (@LocationId, @SKUId, 'GRN_RECEIPT', @ReceivedQuantity, @PrevQty, @NewQty, @GRNNumber);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
PRINT '✓ Created Stored Procedure dbo.sp_ReceiveGRNStock';
GO

PRINT '========================================================================================';
PRINT 'Inventory Database (Db_Inventory) Schema & Procedures Execution Complete!';
PRINT '========================================================================================';
GO
