-- ========================================================================================
-- ERPInfinity - Comprehensive Client Demo Seed Data Script
-- Populates rich product master catalog, stock levels, warehouses, vendors, POs, and users.
-- ========================================================================================

SET NOCOUNT ON;
GO

-- 1. Seed Product Master Data (Db_Product)
USE [Db_Product];
GO

-- Brands
IF NOT EXISTS (SELECT 1 FROM dbo.Brands WHERE Name = 'Tata Consumer')
    INSERT INTO dbo.Brands (Name, Manufacturer, IsActive) VALUES ('Tata Consumer', 'Tata Consumer Products Ltd', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Brands WHERE Name = 'Amul')
    INSERT INTO dbo.Brands (Name, Manufacturer, IsActive) VALUES ('Amul', 'Gujarat Cooperative Milk Marketing', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Brands WHERE Name = 'Nestle')
    INSERT INTO dbo.Brands (Name, Manufacturer, IsActive) VALUES ('Nestle', 'Nestle India Ltd', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Brands WHERE Name = 'Adani Wilmar')
    INSERT INTO dbo.Brands (Name, Manufacturer, IsActive) VALUES ('Adani Wilmar', 'Fortune Foods India', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Brands WHERE Name = 'Britannia')
    INSERT INTO dbo.Brands (Name, Manufacturer, IsActive) VALUES ('Britannia', 'Britannia Industries Ltd', 1);

-- Categories
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryCode = 'CAT-GROCERY')
    INSERT INTO dbo.Categories (CategoryCode, Name, IsActive) VALUES ('CAT-GROCERY', 'Grocery & Essentials', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryCode = 'CAT-DAIRY')
    INSERT INTO dbo.Categories (CategoryCode, Name, IsActive) VALUES ('CAT-DAIRY', 'Dairy & Refrigerated', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryCode = 'CAT-FOOD')
    INSERT INTO dbo.Categories (CategoryCode, Name, IsActive) VALUES ('CAT-FOOD', 'Packaged Snacks & Foods', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryCode = 'CAT-CARE')
    INSERT INTO dbo.Categories (CategoryCode, Name, IsActive) VALUES ('CAT-CARE', 'Personal Care & Hygiene', 1);

-- Helper procedure to insert product with SKU and barcode
DECLARE @B_Tata INT = (SELECT TOP 1 Id FROM dbo.Brands WHERE Name = 'Tata Consumer');
DECLARE @B_Amul INT = (SELECT TOP 1 Id FROM dbo.Brands WHERE Name = 'Amul');
DECLARE @B_Nestle INT = (SELECT TOP 1 Id FROM dbo.Brands WHERE Name = 'Nestle');
DECLARE @B_Fortune INT = (SELECT TOP 1 Id FROM dbo.Brands WHERE Name = 'Adani Wilmar');
DECLARE @B_Britannia INT = (SELECT TOP 1 Id FROM dbo.Brands WHERE Name = 'Britannia');

DECLARE @C_Grocery INT = (SELECT TOP 1 Id FROM dbo.Categories WHERE CategoryCode = 'CAT-GROCERY');
DECLARE @C_Dairy INT = (SELECT TOP 1 Id FROM dbo.Categories WHERE CategoryCode = 'CAT-DAIRY');
DECLARE @C_Food INT = (SELECT TOP 1 Id FROM dbo.Categories WHERE CategoryCode = 'CAT-FOOD');
DECLARE @C_Care INT = (SELECT TOP 1 Id FROM dbo.Categories WHERE CategoryCode = 'CAT-CARE');

-- 1. Tata Salt
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE ProductCode = 'PRD-TATA-SALT-1KG')
BEGIN
    DECLARE @P1 UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
    DECLARE @S1 UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
    INSERT INTO dbo.Products (Id, ProductCode, Name, BrandId, CategoryId, HSNCode, TaxPercentage, IsActive)
    VALUES (@P1, 'PRD-TATA-SALT-1KG', 'Tata Salt Vacuum Evaporated 1kg', @B_Tata, @C_Grocery, '25010010', 5.00, 1);

    INSERT INTO dbo.ProductSKUs (Id, ProductId, SKUCode, UnitOfMeasure, PackSize, WeightKg, MRP, SellingPrice)
    VALUES (@S1, @P1, 'SKU-TATA-SALT-1KG', 'PCS', 1.00, 1.000, 28.00, 25.00);

    INSERT INTO dbo.Barcodes (SKUId, BarcodeNumber, IsPrimary)
    VALUES (@S1, '8901058000101', 1);
END

-- 2. Amul Butter
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE ProductCode = 'PRD-AMUL-BUTTER-500G')
BEGIN
    DECLARE @P2 UNIQUEIDENTIFIER = '11111111-1111-1111-1111-333333333333';
    DECLARE @S2 UNIQUEIDENTIFIER = '22222222-2222-2222-2222-444444444444';
    INSERT INTO dbo.Products (Id, ProductCode, Name, BrandId, CategoryId, HSNCode, TaxPercentage, IsActive)
    VALUES (@P2, 'PRD-AMUL-BUTTER-500G', 'Amul Pasteurised Butter 500g', @B_Amul, @C_Dairy, '04051000', 12.00, 1);

    INSERT INTO dbo.ProductSKUs (Id, ProductId, SKUCode, UnitOfMeasure, PackSize, WeightKg, MRP, SellingPrice)
    VALUES (@S2, @P2, 'SKU-AMUL-BUTTER-500G', 'PCS', 1.00, 0.500, 275.00, 260.00);

    INSERT INTO dbo.Barcodes (SKUId, BarcodeNumber, IsPrimary)
    VALUES (@S2, '8901262010054', 1);
END

-- 3. Maggi Noodles
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE ProductCode = 'PRD-MAGGI-NOODLES-280G')
BEGIN
    DECLARE @P3 UNIQUEIDENTIFIER = '11111111-1111-1111-1111-555555555555';
    DECLARE @S3 UNIQUEIDENTIFIER = '22222222-2222-2222-2222-666666666666';
    INSERT INTO dbo.Products (Id, ProductCode, Name, BrandId, CategoryId, HSNCode, TaxPercentage, IsActive)
    VALUES (@P3, 'PRD-MAGGI-NOODLES-280G', 'Maggi 2-Minute Masala Instant Noodles 280g', @B_Nestle, @C_Food, '19023010', 18.00, 1);

    INSERT INTO dbo.ProductSKUs (Id, ProductId, SKUCode, UnitOfMeasure, PackSize, WeightKg, MRP, SellingPrice)
    VALUES (@S3, @P3, 'SKU-MAGGI-NOODLES-280G', 'PCS', 1.00, 0.280, 54.00, 50.00);

    INSERT INTO dbo.Barcodes (SKUId, BarcodeNumber, IsPrimary)
    VALUES (@S3, '8901058852311', 1);
END

-- 4. Fortune Oil
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE ProductCode = 'PRD-FORTUNE-OIL-1L')
BEGIN
    DECLARE @P4 UNIQUEIDENTIFIER = '11111111-1111-1111-1111-777777777777';
    DECLARE @S4 UNIQUEIDENTIFIER = '22222222-2222-2222-2222-888888888888';
    INSERT INTO dbo.Products (Id, ProductCode, Name, BrandId, CategoryId, HSNCode, TaxPercentage, IsActive)
    VALUES (@P4, 'PRD-FORTUNE-OIL-1L', 'Fortune Refined Sunflower Oil 1L Pouch', @B_Fortune, @C_Grocery, '15121910', 5.00, 1);

    INSERT INTO dbo.ProductSKUs (Id, ProductId, SKUCode, UnitOfMeasure, PackSize, WeightKg, MRP, SellingPrice)
    VALUES (@S4, @P4, 'SKU-FORTUNE-OIL-1L', 'PCS', 1.00, 0.910, 160.00, 145.00);

    INSERT INTO dbo.Barcodes (SKUId, BarcodeNumber, IsPrimary)
    VALUES (@S4, '8901030678912', 1);
END

-- 5. Britannia Good Day
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE ProductCode = 'PRD-GOOD-DAY-200G')
BEGIN
    DECLARE @P5 UNIQUEIDENTIFIER = '11111111-1111-1111-1111-999999999999';
    DECLARE @S5 UNIQUEIDENTIFIER = '22222222-2222-2222-2222-999999999999';
    INSERT INTO dbo.Products (Id, ProductCode, Name, BrandId, CategoryId, HSNCode, TaxPercentage, IsActive)
    VALUES (@P5, 'PRD-GOOD-DAY-200G', 'Britannia Good Day Butter Cookies 200g', @B_Britannia, @C_Food, '19053100', 18.00, 1);

    INSERT INTO dbo.ProductSKUs (Id, ProductId, SKUCode, UnitOfMeasure, PackSize, WeightKg, MRP, SellingPrice)
    VALUES (@S5, @P5, 'SKU-GOOD-DAY-BISCUIT', 'PCS', 1.00, 0.200, 35.00, 30.00);

    INSERT INTO dbo.Barcodes (SKUId, BarcodeNumber, IsPrimary)
    VALUES (@S5, '8901014002105', 1);
END
GO

-- 2. Seed Inventory Location & Stocks (Db_Inventory)
USE [Db_Inventory];
GO

DECLARE @WH1 UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';
DECLARE @ST1 UNIQUEIDENTIFIER = '44444444-4444-4444-4444-444444444444';

IF NOT EXISTS (SELECT 1 FROM dbo.Locations WHERE LocationCode = 'WH-CENTRAL-01')
    INSERT INTO dbo.Locations (Id, LocationCode, Name, Type, IsActive)
    VALUES (@WH1, 'WH-CENTRAL-01', 'Central Distribution Hub #1', 'Warehouse', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Locations WHERE LocationCode = 'STORE-MUMBAI-01')
    INSERT INTO dbo.Locations (Id, LocationCode, Name, Type, IsActive)
    VALUES (@ST1, 'STORE-MUMBAI-01', 'Mumbai Supercenter Store #101', 'Store', 1);

-- Seed Stock Records
DECLARE @S1 UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @S2 UNIQUEIDENTIFIER = '22222222-2222-2222-2222-444444444444';
DECLARE @S3 UNIQUEIDENTIFIER = '22222222-2222-2222-2222-666666666666';

IF NOT EXISTS (SELECT 1 FROM dbo.Stocks WHERE LocationId = @WH1 AND SKUId = @S1)
    INSERT INTO dbo.Stocks (Id, LocationId, SKUId, OnHandQuantity, ReservedQuantity, ReorderPoint)
    VALUES (NEWID(), @WH1, @S1, 500.000, 0.000, 50.000);

IF NOT EXISTS (SELECT 1 FROM dbo.Stocks WHERE LocationId = @ST1 AND SKUId = @S2)
    INSERT INTO dbo.Stocks (Id, LocationId, SKUId, OnHandQuantity, ReservedQuantity, ReorderPoint)
    VALUES (NEWID(), @ST1, @S2, 150.000, 0.000, 20.000);

IF NOT EXISTS (SELECT 1 FROM dbo.Stocks WHERE LocationId = @WH1 AND SKUId = @S3)
    INSERT INTO dbo.Stocks (Id, LocationId, SKUId, OnHandQuantity, ReservedQuantity, ReorderPoint)
    VALUES (NEWID(), @WH1, @S3, 300.000, 0.000, 40.000);
GO

-- 3. Seed Purchase Orders & Procurement (Db_Purchase)
USE [Db_Purchase];
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Vendors WHERE VendorCode = 'VND-TATA-01')
    INSERT INTO dbo.Vendors (Id, VendorCode, Name, ContactPerson, Email, Phone, IsActive)
    VALUES (NEWID(), 'VND-TATA-01', 'Tata Consumer Products Ltd.', 'Ramesh Iyer', 'orders@tataconsumer.com', '+91 22 6600 1122', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Vendors WHERE VendorCode = 'VND-AMUL-01')
    INSERT INTO dbo.Vendors (Id, VendorCode, Name, ContactPerson, Email, Phone, IsActive)
    VALUES (NEWID(), 'VND-AMUL-01', 'Gujarat Cooperative Milk Marketing (Amul)', 'Suresh Patel', 'sales@amul.coop', '+91 2692 258500', 1);
GO

PRINT '========================================================================================';
PRINT 'MASTER CLIENT DEMO DATA SEEDED SUCCESSFULLY ON MSSQLSERVER01!';
PRINT '========================================================================================';
GO
