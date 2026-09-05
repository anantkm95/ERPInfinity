-- ========================================================================================
-- ERPInfinity - Master Database Creation Script for Microservices (SQL Server 2022)
-- Architecture: Database-Per-Service Pattern
-- Description: Creates isolated relational databases for all microservices in the ERP system.
-- ========================================================================================

USE [master];
GO

PRINT '----------------------------------------------------------------------------------------';
PRINT 'Starting ERPInfinity Microservice Databases Creation...';
PRINT '----------------------------------------------------------------------------------------';

-- 1. Identity & Access Management Service Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Db_Identity')
BEGIN
    CREATE DATABASE [Db_Identity];
    PRINT '✓ Database [Db_Identity] created successfully (Identity Service).';
END
ELSE
    PRINT '-> Database [Db_Identity] already exists.';
GO

-- Alias check/create for default connection string compatibility
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Identity')
BEGIN
    CREATE DATABASE [Identity];
    PRINT '✓ Database [Identity] alias created successfully.';
END
GO

-- 2. Product Master & Catalog Service Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Db_Product')
BEGIN
    CREATE DATABASE [Db_Product];
    PRINT '✓ Database [Db_Product] created successfully (Product Service).';
END
ELSE
    PRINT '-> Database [Db_Product] already exists.';
GO

-- 3. Stock Ledger & Inventory Service Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Db_Inventory')
BEGIN
    CREATE DATABASE [Db_Inventory];
    PRINT '✓ Database [Db_Inventory] created successfully (Inventory Service).';
END
ELSE
    PRINT '-> Database [Db_Inventory] already exists.';
GO

-- 4. Sales & POS Billing Service Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Db_Sales')
BEGIN
    CREATE DATABASE [Db_Sales];
    PRINT '✓ Database [Db_Sales] created successfully (Sales/POS Service).';
END
ELSE
    PRINT '-> Database [Db_Sales] already exists.';
GO

-- 5. Procurement & Vendor PO Service Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Db_Purchase')
BEGIN
    CREATE DATABASE [Db_Purchase];
    PRINT '✓ Database [Db_Purchase] created successfully (Purchase Service).';
END
ELSE
    PRINT '-> Database [Db_Purchase] already exists.';
GO

-- 6. Warehouse & Fulfillment Service Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Db_Warehouse')
BEGIN
    CREATE DATABASE [Db_Warehouse];
    PRINT '✓ Database [Db_Warehouse] created successfully (Warehouse Service).';
END
ELSE
    PRINT '-> Database [Db_Warehouse] already exists.';
GO

-- 7. Online Cart & Order Management Service Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Db_Order')
BEGIN
    CREATE DATABASE [Db_Order];
    PRINT '✓ Database [Db_Order] created successfully (Order Service).';
END
ELSE
    PRINT '-> Database [Db_Order] already exists.';
GO

-- 8. Dynamic Pricing & Promotion Service Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Db_Pricing')
BEGIN
    CREATE DATABASE [Db_Pricing];
    PRINT '✓ Database [Db_Pricing] created successfully (Pricing Service).';
END
ELSE
    PRINT '-> Database [Db_Pricing] already exists.';
GO

-- 9. Payment Gateway & Transaction Service Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Db_Payment')
BEGIN
    CREATE DATABASE [Db_Payment];
    PRINT '✓ Database [Db_Payment] created successfully (Payment Service).';
END
ELSE
    PRINT '-> Database [Db_Payment] already exists.';
GO

-- 10. General Ledger & Accounting Service Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Db_Finance')
BEGIN
    CREATE DATABASE [Db_Finance];
    PRINT '✓ Database [Db_Finance] created successfully (Finance Service).';
END
ELSE
    PRINT '-> Database [Db_Finance] already exists.';
GO

-- 11. Store & Infrastructure Service Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Db_Store')
BEGIN
    CREATE DATABASE [Db_Store];
    PRINT '✓ Database [Db_Store] created successfully (Store Service).';
END
ELSE
    PRINT '-> Database [Db_Store] already exists.';
GO

-- 12. Notification & Alerts Service Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Db_Notification')
BEGIN
    CREATE DATABASE [Db_Notification];
    PRINT '✓ Database [Db_Notification] created successfully (Notification Service).';
END
ELSE
    PRINT '-> Database [Db_Notification] already exists.';
GO

-- 13. Customer & CRM Service Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Db_Customer')
BEGIN
    CREATE DATABASE [Db_Customer];
    PRINT '✓ Database [Db_Customer] created successfully (Customer Service).';
END
ELSE
    PRINT '-> Database [Db_Customer] already exists.';
GO

-- 14. Reporting & Analytics Service Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Db_Reporting')
BEGIN
    CREATE DATABASE [Db_Reporting];
    PRINT '✓ Database [Db_Reporting] created successfully (Reporting Service).';
END
ELSE
    PRINT '-> Database [Db_Reporting] already exists.';
GO

PRINT '----------------------------------------------------------------------------------------';
PRINT 'ALL ERPInfinity MICROSERVICE DATABASES CREATED SUCCESSFULLY!';
PRINT '----------------------------------------------------------------------------------------';
GO
