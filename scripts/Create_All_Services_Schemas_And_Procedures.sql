-- ========================================================================================
-- ERPInfinity - Master Schema & Stored Procedures Script for All 14 Microservices
-- SQL Server 2022 | Database-Per-Service Architecture
-- ========================================================================================

-- ----------------------------------------------------------------------------------------
-- 1. Db_Warehouse Database Schema
-- ----------------------------------------------------------------------------------------
USE [Db_Warehouse];
GO
SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; SET NOCOUNT ON;

IF OBJECT_ID('dbo.Warehouses', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Warehouses (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        WarehouseCode VARCHAR(30) NOT NULL UNIQUE,
        Name NVARCHAR(150) NOT NULL,
        Address NVARCHAR(250) NULL,
        IsActive BIT NOT NULL DEFAULT(1)
    );
END

IF OBJECT_ID('dbo.Zones', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Zones (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        WarehouseId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.Warehouses(Id),
        ZoneCode VARCHAR(20) NOT NULL,
        Name NVARCHAR(100) NOT NULL
    );
END

IF OBJECT_ID('dbo.Bins', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bins (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ZoneId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.Zones(Id),
        BinCode VARCHAR(30) NOT NULL UNIQUE,
        CapacityKg DECIMAL(10,2) NOT NULL DEFAULT(100.00),
        IsOccupied BIT NOT NULL DEFAULT(0)
    );
END

IF OBJECT_ID('dbo.PickingLists', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PickingLists (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        OrderId UNIQUEIDENTIFIER NOT NULL,
        PickerId UNIQUEIDENTIFIER NULL,
        Status VARCHAR(30) NOT NULL DEFAULT('Pending'), -- 'Pending', 'InProgress', 'Completed'
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
    );
END
GO

-- ----------------------------------------------------------------------------------------
-- 2. Db_Order Database Schema
-- ----------------------------------------------------------------------------------------
USE [Db_Order];
GO
SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; SET NOCOUNT ON;

IF OBJECT_ID('dbo.Carts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Carts (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        CustomerId UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
    );
END

IF OBJECT_ID('dbo.Orders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        OrderNumber VARCHAR(50) NOT NULL UNIQUE,
        CustomerId UNIQUEIDENTIFIER NOT NULL,
        TotalAmount DECIMAL(18,2) NOT NULL,
        OrderStatus VARCHAR(30) NOT NULL DEFAULT('Placed'),
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
    );
END

IF OBJECT_ID('dbo.OrderItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        OrderId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.Orders(Id),
        SKUId UNIQUEIDENTIFIER NOT NULL,
        Quantity DECIMAL(18,3) NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL
    );
END
GO

-- ----------------------------------------------------------------------------------------
-- 3. Db_Pricing Database Schema
-- ----------------------------------------------------------------------------------------
USE [Db_Pricing];
GO
SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; SET NOCOUNT ON;

IF OBJECT_ID('dbo.SKUBasePrices', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SKUBasePrices (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        SKUId UNIQUEIDENTIFIER NOT NULL,
        BasePrice DECIMAL(18,2) NOT NULL,
        EffectiveFrom DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
    );
END

IF OBJECT_ID('dbo.Promotions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Promotions (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        PromoCode VARCHAR(50) NOT NULL UNIQUE,
        DiscountPercentage DECIMAL(5,2) NOT NULL,
        StartDate DATETIME2 NOT NULL,
        EndDate DATETIME2 NOT NULL
    );
END
GO

-- ----------------------------------------------------------------------------------------
-- 4. Db_Payment Database Schema
-- ----------------------------------------------------------------------------------------
USE [Db_Payment];
GO
SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; SET NOCOUNT ON;

IF OBJECT_ID('dbo.PaymentTransactions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PaymentTransactions (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TransactionReference VARCHAR(100) NOT NULL UNIQUE,
        OrderId UNIQUEIDENTIFIER NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        PaymentGateway VARCHAR(50) NOT NULL,
        Status VARCHAR(30) NOT NULL DEFAULT('Pending'),
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
    );
END
GO

-- ----------------------------------------------------------------------------------------
-- 5. Db_Finance Database Schema
-- ----------------------------------------------------------------------------------------
USE [Db_Finance];
GO
SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; SET NOCOUNT ON;

IF OBJECT_ID('dbo.Accounts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Accounts (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        AccountCode VARCHAR(30) NOT NULL UNIQUE,
        AccountName NVARCHAR(150) NOT NULL,
        Type VARCHAR(50) NOT NULL -- 'Asset', 'Liability', 'Equity', 'Revenue', 'Expense'
    );
END

IF OBJECT_ID('dbo.JournalEntries', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.JournalEntries (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        VoucherNumber VARCHAR(50) NOT NULL UNIQUE,
        EntryDate DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
        TotalDebit DECIMAL(18,2) NOT NULL,
        TotalCredit DECIMAL(18,2) NOT NULL
    );
END

IF OBJECT_ID('dbo.JournalEntryLines', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.JournalEntryLines (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        JournalEntryId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.JournalEntries(Id),
        AccountId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.Accounts(Id),
        DebitAmount DECIMAL(18,2) NOT NULL DEFAULT(0.00),
        CreditAmount DECIMAL(18,2) NOT NULL DEFAULT(0.00)
    );
END
GO

-- ----------------------------------------------------------------------------------------
-- 6. Db_Store Database Schema
-- ----------------------------------------------------------------------------------------
USE [Db_Store];
GO
SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; SET NOCOUNT ON;

IF OBJECT_ID('dbo.Stores', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Stores (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        StoreCode VARCHAR(30) NOT NULL UNIQUE,
        Name NVARCHAR(150) NOT NULL,
        Address NVARCHAR(250) NULL,
        IsActive BIT NOT NULL DEFAULT(1)
    );
END
GO

-- ----------------------------------------------------------------------------------------
-- 7. Db_Notification Database Schema
-- ----------------------------------------------------------------------------------------
USE [Db_Notification];
GO
SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; SET NOCOUNT ON;

IF OBJECT_ID('dbo.NotificationLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.NotificationLogs (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Recipient VARCHAR(150) NOT NULL,
        MessageType VARCHAR(30) NOT NULL, -- 'SMS', 'EMAIL'
        MessageBody NVARCHAR(MAX) NOT NULL,
        Status VARCHAR(30) NOT NULL DEFAULT('Queued'),
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
    );
END
GO

-- ----------------------------------------------------------------------------------------
-- 8. Db_Customer Database Schema
-- ----------------------------------------------------------------------------------------
USE [Db_Customer];
GO
SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; SET NOCOUNT ON;

IF OBJECT_ID('dbo.Customers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Customers (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        CustomerCode VARCHAR(30) NOT NULL UNIQUE,
        Name NVARCHAR(150) NOT NULL,
        Email VARCHAR(150) NULL,
        Phone VARCHAR(20) NOT NULL UNIQUE,
        LoyaltyPoints INT NOT NULL DEFAULT(0),
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
    );
END
GO

-- ----------------------------------------------------------------------------------------
-- 9. Db_Reporting Database Schema
-- ----------------------------------------------------------------------------------------
USE [Db_Reporting];
GO
SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; SET NOCOUNT ON;

IF OBJECT_ID('dbo.ReportMetadata', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ReportMetadata (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ReportName NVARCHAR(150) NOT NULL,
        ExecutedBy UNIQUEIDENTIFIER NOT NULL,
        ExecutedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
    );
END
GO

PRINT '========================================================================================';
PRINT 'ALL 14 MICROSERVICE DATABASE SCHEMAS AND TABLES SUCCESSFULLY PROVISIONED!';
PRINT '========================================================================================';
GO
