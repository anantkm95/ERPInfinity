# ERPInfinity – Database Schema, Stored Procedures, Views & Indexing Specification

> **Project Name:** ERPInfinity  
> **Database Engines:** Microsoft SQL Server 2022 (ACID Writes & Transactional Data) + MongoDB 7.0 (CQRS Read Projections)  
> **Scope:** Exhaustive database design documentation per microservice domain.

---

## Executive Summary & Database Inventory

The **ERPInfinity** platform follows a **Database-Per-Service** architecture to enforce strict service boundaries. The write-heavy transactional operations reside in SQL Server databases, while high-speed reporting and read projections reside in MongoDB and Redis.

### System-Wide Inventory Summary

| Metric | Total Count |
|---|---|
| **SQL Server Databases** | 12 Microservice Databases |
| **Relational Tables** | 54 Tables |
| **Stored Procedures** | 41 Procedures (High-throughput & transactional execution) |
| **Database Views** | 22 Views (Transactional reporting & aggregation) |
| **Performance Indexes** | 68 Indexes (Clustered, Non-Clustered & Composite) |
| **MongoDB Collections** | 14 Read Projection Collections |

---

## 1. ERPInfinity_Identity Service Database (`Db_Identity`)

### 1.1 Tables (5 Tables)

#### 1. `Users`
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `Username` (NVARCHAR(100)), `Email` (NVARCHAR(150)), `PasswordHash` (NVARCHAR(256)), `PhoneNumber` (NVARCHAR(20)), `IsActive` (BIT), `CreatedAt` (DATETIME2), `UpdatedAt` (DATETIME2)
- **Constraints:** UNIQUE(`Email`), UNIQUE(`Username`)
- **Purpose:** Stores user authentication profiles for employees, cashiers, store managers, and administrators.

#### 2. `Roles`
- **Columns:** `Id` (INT, PK, IDENTITY), `Name` (NVARCHAR(50)), `Description` (NVARCHAR(200)), `IsSystemRole` (BIT)
- **Constraints:** UNIQUE(`Name`)
- **Purpose:** System role definitions (Admin, StoreManager, Cashier, InventoryManager, SupplierManager).

#### 3. `UserRoles`
- **Columns:** `UserId` (UNIQUEIDENTIFIER, FK $\rightarrow$ Users.Id), `RoleId` (INT, FK $\rightarrow$ Roles.Id), `AssignedAt` (DATETIME2)
- **Constraints:** PK(`UserId`, `RoleId`)
- **Purpose:** Mapping junction table between Users and Roles.

#### 4. `Permissions`
- **Columns:** `Id` (INT, PK, IDENTITY), `PermissionCode` (NVARCHAR(100)), `Module` (NVARCHAR(50)), `Description` (NVARCHAR(200))
- **Constraints:** UNIQUE(`PermissionCode`)
- **Purpose:** Granular permission codes (e.g., `Product.Create`, `Sales.Refund`, `Inventory.Adjust`).

#### 5. `RolePermissions`
- **Columns:** `RoleId` (INT, FK $\rightarrow$ Roles.Id), `PermissionId` (INT, FK $\rightarrow$ Permissions.Id)
- **Constraints:** PK(`RoleId`, `PermissionId`)
- **Purpose:** Authorization matrix mapping permissions to roles.

---

### 1.2 Stored Procedures (3 Procedures)

#### 1. `sp_AuthenticateUser`
- **Inputs:** `@Username` NVARCHAR(100), `@PasswordHash` NVARCHAR(256)
- **Outputs:** User Profile, Role List, and Permission Codes
- **Functionality:** Authenticates user credentials and returns full security claim context for JWT token generation.

#### 2. `sp_AssignUserRole`
- **Inputs:** `@UserId` UNIQUEIDENTIFIER, `@RoleId` INT, `@AssignedBy` UNIQUEIDENTIFIER
- **Functionality:** Safely assigns a role to a user, checking for existing assignments and logging an audit trace.

#### 3. `sp_GetUserPermissions`
- **Inputs:** `@UserId` UNIQUEIDENTIFIER
- **Outputs:** List of active permission strings
- **Functionality:** Flattens user roles and returns distinct active permissions.

---

### 1.3 Views (2 Views)

#### 1. `vw_UserSecurityProfile`
- **Query:** Joins `Users`, `UserRoles`, `Roles`, `RolePermissions`, and `Permissions`.
- **Purpose:** Consolidate user identity, active status, roles, and concatenated permissions for security middleware validation.

#### 2. `vw_ActiveRolePermissionMatrix`
- **Query:** Selects `Roles.Name`, `Permissions.PermissionCode`, `Permissions.Module`.
- **Purpose:** Admin portal dashboard view for role configuration management.

---

### 1.4 Indexes (6 Indexes)

- `IX_Users_Email` (Non-Clustered on `Users.Email`) $\rightarrow$ Accelerates email login lookups.
- `IX_Users_Username` (Non-Clustered on `Users.Username`) $\rightarrow$ Accelerates username login lookups.
- `IX_UserRoles_UserId` (Non-Clustered on `UserRoles.UserId`) $\rightarrow$ Fast role resolution per user.
- `IX_RolePermissions_RoleId` (Non-Clustered on `RolePermissions.RoleId`) $\rightarrow$ Fast permission resolution per role.
- `IX_Permissions_PermissionCode` (Non-Clustered on `Permissions.PermissionCode`).
- `IX_Users_IsActive_CreatedAt` (Composite Non-Clustered on `Users(IsActive, CreatedAt)`).

---

## 2. ERPInfinity_Product Service Database (`Db_Product`)

### 2.1 Tables (6 Tables)

#### 1. `Categories`
- **Columns:** `Id` (INT, PK, IDENTITY), `ParentCategoryId` (INT, NULLable FK), `CategoryCode` (VARCHAR(20)), `Name` (NVARCHAR(100)), `IsActive` (BIT)
- **Purpose:** Hierarchical product categorization (e.g., Grocery $\rightarrow$ Spices $\rightarrow$ Salt).

#### 2. `Brands`
- **Columns:** `Id` (INT, PK, IDENTITY), `Name` (NVARCHAR(100)), `Manufacturer` (NVARCHAR(150)), `IsActive` (BIT)
- **Purpose:** Master brand directory (e.g., Tata, Nestle, Unilever).

#### 3. `Products`
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `ProductCode` (VARCHAR(30)), `Name` (NVARCHAR(200)), `BrandId` (INT, FK), `CategoryId` (INT, FK), `HSNCode` (VARCHAR(20)), `TaxPercentage` (DECIMAL(5,2)), `IsActive` (BIT), `CreatedAt` (DATETIME2)
- **Purpose:** Core product master records.

#### 4. `ProductSKUs`
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `ProductId` (UNIQUEIDENTIFIER, FK), `SKUCode` (VARCHAR(50)), `UnitOfMeasure` (VARCHAR(20)), `PackSize` (DECIMAL(10,2)), `WeightKg` (DECIMAL(10,3)), `MRP` (DECIMAL(18,2)), `SellingPrice` (DECIMAL(18,2))
- **Constraints:** UNIQUE(`SKUCode`)
- **Purpose:** SKU variants for a master product (e.g., 500g vs 1kg pack).

#### 5. `Barcodes`
- **Columns:** `Id` (BIGINT, PK, IDENTITY), `SKUId` (UNIQUEIDENTIFIER, FK), `BarcodeNumber` (VARCHAR(50)), `IsPrimary` (BIT)
- **Constraints:** UNIQUE(`BarcodeNumber`)
- **Purpose:** EAN/UPC barcodes attached to SKUs for fast POS terminal scanning.

#### 6. `ProductOutbox`
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `EventType` (VARCHAR(100)), `Payload` (NVARCHAR(MAX)), `ProcessedAt` (DATETIME2, NULL), `CreatedAt` (DATETIME2)
- **Purpose:** Reliable Outbox Pattern table for publishing `ProductCreatedEvent` & `ProductUpdatedEvent` to RabbitMQ.

---

### 2.2 Stored Procedures (4 Procedures)

#### 1. `sp_LookupProductByBarcode`
- **Inputs:** `@BarcodeNumber` VARCHAR(50)
- **Outputs:** ProductId, SKUCode, Name, MRP, SellingPrice, TaxPercentage, HSNCode
- **Functionality:** High-speed scanning lookup for POS cash counters. Executes in $< 2\text{ms}$.

#### 2. `sp_CreateProductWithSKUs`
- **Inputs:** Product JSON / XML Payload containing Product Master, SKUs, and Barcodes
- **Functionality:** Atomic transaction creating Product, SKUs, Barcodes, and emitting Outbox record in a single SQL batch.

#### 3. `sp_UpdateSKUPricing`
- **Inputs:** `@SKUId` UNIQUEIDENTIFIER, `@NewMRP` DECIMAL(18,2), `@NewSellingPrice` DECIMAL(18,2), `@UpdatedBy` UNIQUEIDENTIFIER
- **Functionality:** Updates SKU pricing and writes an integration event into `ProductOutbox`.

#### 4. `sp_GetProductOutboxPendingMessages`
- **Inputs:** `@BatchSize` INT
- **Outputs:** List of unprocessed Outbox events
- **Functionality:** Fetches unprocessed integration events for the RabbitMQ background publisher worker.

---

### 2.3 Views (2 Views)

#### 1. `vw_ProductCatalogDetail`
- **Query:** Joins `Products`, `Categories`, `Brands`, `ProductSKUs`, and `Barcodes`.
- **Purpose:** Consolidates complete product information for catalog export and MongoDB sync workers.

#### 2. `vw_ActivePOSBarcodes`
- **Query:** Selects `Barcodes.BarcodeNumber`, `ProductSKUs.SKUCode`, `Products.Name`, `ProductSKUs.SellingPrice`, `Products.TaxPercentage` WHERE `IsActive = 1`.
- **Purpose:** Read-optimized dataset used to pre-load store POS local memory caches.

---

### 2.4 Indexes (7 Indexes)

- `IX_Barcodes_BarcodeNumber` (Clustered / Non-Clustered UNIQUE on `Barcodes.BarcodeNumber`) $\rightarrow$ Critical POS scan performance index.
- `IX_ProductSKUs_SKUCode` (Non-Clustered UNIQUE on `ProductSKUs.SKUCode`).
- `IX_Products_CategoryId` (Non-Clustered on `Products.CategoryId`).
- `IX_Products_BrandId` (Non-Clustered on `Products.BrandId`).
- `IX_ProductSKUs_ProductId` (Non-Clustered on `ProductSKUs.ProductId`).
- `IX_ProductOutbox_ProcessedAt_CreatedAt` (Composite Non-Clustered on `ProductOutbox(ProcessedAt, CreatedAt)`).
- `IX_Products_IsActive_Name` (Composite Non-Clustered on `Products(IsActive, Name)`).

---

## 3. ERPInfinity_Inventory Service Database (`Db_Inventory`)

### 3.1 Tables (5 Tables)

#### 1. `Stocks`
- **Columns:** `Id` (BIGINT, PK, IDENTITY), `LocationId` (UNIQUEIDENTIFIER), `LocationType` (VARCHAR(20) - 'Store'/'Warehouse'), `SKUId` (UNIQUEIDENTIFIER), `QuantityOnHand` (DECIMAL(18,3)), `ReservedQuantity` (DECIMAL(18,3)), `AvailableQuantity` AS (`QuantityOnHand` - `ReservedQuantity`), `LastUpdated` (DATETIME2)
- **Constraints:** UNIQUE(`LocationId`, `SKUId`)
- **Purpose:** Current physical and available inventory balances.

#### 2. `StockTransactions`
- **Columns:** `Id` (BIGINT, PK, IDENTITY), `LocationId` (UNIQUEIDENTIFIER), `SKUId` (UNIQUEIDENTIFIER), `TransactionType` (VARCHAR(30) - 'PurchaseGRN', 'POSSale', 'Adjustment', 'TransferIn', 'TransferOut'), `Quantity` (DECIMAL(18,3)), `ReferenceId` (VARCHAR(100)), `CreatedAt` (DATETIME2)
- **Purpose:** Immutable audit ledger for every single stock movement across the enterprise.

#### 3. `StockAdjustments`
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `LocationId` (UNIQUEIDENTIFIER), `SKUId` (UNIQUEIDENTIFIER), `AdjustmentQuantity` (DECIMAL(18,3)), `ReasonCode` (VARCHAR(50)), `ApprovedBy` (UNIQUEIDENTIFIER), `CreatedAt` (DATETIME2)
- **Purpose:** Record inventory corrections (damage, shrinkage, audit discrepancy).

#### 4. `StockTransfers`
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `TransferNumber` (VARCHAR(30)), `SourceLocationId` (UNIQUEIDENTIFIER), `DestinationLocationId` (UNIQUEIDENTIFIER), `Status` (VARCHAR(20)), `DispatchedAt` (DATETIME2, NULL), `ReceivedAt` (DATETIME2, NULL)
- **Purpose:** Inter-store and warehouse-to-store stock transfer tracking.

#### 5. `StockTransferItems`
- **Columns:** `Id` (BIGINT, PK, IDENTITY), `TransferId` (UNIQUEIDENTIFIER, FK $\rightarrow$ StockTransfers.Id), `SKUId` (UNIQUEIDENTIFIER), `RequestedQty` (DECIMAL(18,3)), `DispatchedQty` (DECIMAL(18,3)), `ReceivedQty` (DECIMAL(18,3))
- **Purpose:** Line items contained inside a stock transfer shipment.

---

### 3.2 Stored Procedures (5 Procedures)

#### 1. `sp_DeductInventoryForPOS`
- **Inputs:** `@StoreId` UNIQUEIDENTIFIER, `@SKUId` UNIQUEIDENTIFIER, `@Quantity` DECIMAL(18,3), `@InvoiceNumber` VARCHAR(50)
- **Functionality:** Thread-safe, atomic inventory deduction with row-level locks. Automatically creates a `POSSale` transaction record in `StockTransactions`.

#### 2. `sp_ReceiveGRNStock`
- **Inputs:** `@WarehouseId` UNIQUEIDENTIFIER, `@SKUId` UNIQUEIDENTIFIER, `@Quantity` DECIMAL(18,3), `@GRNNumber` VARCHAR(50)
- **Functionality:** Increments `QuantityOnHand` and logs `PurchaseGRN` transaction in `StockTransactions`.

#### 3. `sp_ReserveInventoryForOnlineOrder`
- **Inputs:** `@LocationId` UNIQUEIDENTIFIER, `@SKUId` UNIQUEIDENTIFIER, `@Quantity` DECIMAL(18,3), `@OrderId` VARCHAR(50)
- **Outputs:** `@IsSuccess` BIT
- **Functionality:** Checks available stock and moves quantity into `ReservedQuantity` state.

#### 4. `sp_ExecuteStockAdjustment`
- **Inputs:** `@LocationId` UNIQUEIDENTIFIER, `@SKUId` UNIQUEIDENTIFIER, `@AdjustmentQty` DECIMAL(18,3), `@ReasonCode` VARCHAR(50), `@ApprovedBy` UNIQUEIDENTIFIER
- **Functionality:** Applies adjustment to `Stocks` and appends audit log to `StockTransactions` and `StockAdjustments`.

#### 5. `sp_GetLowStockAlerts`
- **Inputs:** `@LocationId` UNIQUEIDENTIFIER, `@ThresholdQty` DECIMAL(18,3)
- **Outputs:** List of SKUs below safety stock threshold
- **Functionality:** Identifies items requiring automated procurement re-orders.

---

### 3.3 Views (2 Views)

#### 1. `vw_LocationStockSummary`
- **Query:** Selects `LocationId`, `LocationType`, `SKUId`, `QuantityOnHand`, `ReservedQuantity`, `AvailableQuantity`.
- **Purpose:** Read-optimized query source for inventory control center.

#### 2. `vw_RecentStockMovementLedger`
- **Query:** Joins `StockTransactions` with location metadata ordered by `CreatedAt DESC`.
- **Purpose:** Stock movement audit trail view.

---

### 3.4 Indexes (8 Indexes)

- `IX_Stocks_LocationId_SKUId` (Unique Clustered/Non-Clustered on `Stocks(LocationId, SKUId)`) $\rightarrow$ Primary stock lookup index.
- `IX_StockTransactions_LocationId_SKUId` (Composite Non-Clustered on `StockTransactions(LocationId, SKUId)`).
- `IX_StockTransactions_ReferenceId` (Non-Clustered on `StockTransactions.ReferenceId`).
- `IX_StockTransactions_CreatedAt` (Non-Clustered on `StockTransactions.CreatedAt`).
- `IX_StockTransfers_Status` (Non-Clustered on `StockTransfers.Status`).
- `IX_StockTransferItems_TransferId` (Non-Clustered on `StockTransferItems.TransferId`).
- `IX_Stocks_AvailableQuantity` (Non-Clustered on `Stocks.AvailableQuantity`).
- `IX_StockAdjustments_LocationId` (Non-Clustered on `StockAdjustments.LocationId`).

---

## 4. ERPInfinity_Sales / POS Service Database (`Db_Sales`)

### 4.1 Tables (5 Tables)

#### 1. `POSRegisters`
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `StoreId` (UNIQUEIDENTIFIER), `TerminalCode` (VARCHAR(20)), `CashierId` (UNIQUEIDENTIFIER), `Status` (VARCHAR(20) - 'Open'/'Closed'), `OpeningBalance` (DECIMAL(18,2)), `ClosingBalance` (DECIMAL(18,2)), `OpenedAt` (DATETIME2), `ClosedAt` (DATETIME2, NULL)
- **Purpose:** Manages cash counter register sessions.

#### 2. `SalesInvoices`
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `InvoiceNumber` (VARCHAR(50)), `StoreId` (UNIQUEIDENTIFIER), `POSTerminalId` (UNIQUEIDENTIFIER), `CashierId` (UNIQUEIDENTIFIER), `SubTotal` (DECIMAL(18,2)), `DiscountAmount` (DECIMAL(18,2)), `TaxAmount` (DECIMAL(18,2)), `GrandTotal` (DECIMAL(18,2)), `PaymentMethod` (VARCHAR(20)), `PaymentStatus` (VARCHAR(20)), `CreatedAt` (DATETIME2)
- **Constraints:** UNIQUE(`InvoiceNumber`)
- **Purpose:** Header table for retail billing invoices.

#### 3. `SalesInvoiceItems`
- **Columns:** `Id` (BIGINT, PK, IDENTITY), `InvoiceId` (UNIQUEIDENTIFIER, FK $\rightarrow$ SalesInvoices.Id), `SKUId` (UNIQUEIDENTIFIER), `SKUCode` (VARCHAR(50)), `ProductName` (NVARCHAR(200)), `UnitPrice` (DECIMAL(18,2)), `Quantity` (DECIMAL(18,3)), `DiscountAmount` (DECIMAL(18,2)), `TaxAmount` (DECIMAL(18,2)), `LineTotal` (DECIMAL(18,2))
- **Purpose:** Individual items billed on a sales invoice.

#### 4. `SalesReturns`
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `ReturnNumber` (VARCHAR(50)), `OriginalInvoiceId` (UNIQUEIDENTIFIER, FK $\rightarrow$ SalesInvoices.Id), `StoreId` (UNIQUEIDENTIFIER), `RefundAmount` (DECIMAL(18,2)), `Reason` (NVARCHAR(200)), `ProcessedBy` (UNIQUEIDENTIFIER), `CreatedAt` (DATETIME2)
- **Purpose:** Handles customer counter product returns and cash/credit refunds.

#### 5. `SalesOutbox`
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `EventType` (VARCHAR(100)), `Payload` (NVARCHAR(MAX)), `ProcessedAt` (DATETIME2, NULL), `CreatedAt` (DATETIME2)
- **Purpose:** Outbox table for emitting `SalesInvoiceCreatedEvent` to RabbitMQ.

---

### 4.2 Stored Procedures (4 Procedures)

#### 1. `sp_CreateSalesInvoice`
- **Inputs:** Invoice Header JSON + Line Items JSON array
- **Outputs:** `@InvoiceId` UNIQUEIDENTIFIER, `@InvoiceNumber` VARCHAR(50)
- **Functionality:** High-speed atomic procedure creating Invoice Header, Line Items, and Outbox Event. Runs in $< 5\text{ms}$.

#### 2. `sp_OpenPOSRegister`
- **Inputs:** `@StoreId` UNIQUEIDENTIFIER, `@TerminalCode` VARCHAR(20), `@CashierId` UNIQUEIDENTIFIER, `@OpeningBalance` DECIMAL(18,2)
- **Functionality:** Opens counter session and validates no active duplicate session exists.

#### 3. `sp_ClosePOSRegister`
- **Inputs:** `@RegisterId` UNIQUEIDENTIFIER, `@ClosingBalance` DECIMAL(18,2)
- **Outputs:** Cash variance amount
- **Functionality:** Closes register session and computes cash reconciliation variance.

#### 4. `sp_ProcessSalesReturn`
- **Inputs:** Return Header JSON + Item array
- **Functionality:** Validates original invoice, generates return receipt, and emits `SalesReturnCreatedEvent`.

---

### 4.3 Views (2 Views)

#### 1. `vw_DailyStoreSalesSummary`
- **Query:** Aggregates `SalesInvoices` grouped by `StoreId`, `CAST(CreatedAt AS DATE)`.
- **Purpose:** Executive daily sales totals view.

#### 2. `vw_POSTerminalPerformance`
- **Query:** Summarizes total bills, cash volume, card volume, and average bill value per register terminal.
- **Purpose:** Store operational metrics dashboard.

---

### 4.4 Indexes (7 Indexes)

- `IX_SalesInvoices_InvoiceNumber` (Unique Clustered/Non-Clustered on `SalesInvoices.InvoiceNumber`).
- `IX_SalesInvoices_StoreId_CreatedAt` (Composite Non-Clustered on `SalesInvoices(StoreId, CreatedAt)`).
- `IX_SalesInvoiceItems_InvoiceId` (Non-Clustered on `SalesInvoiceItems.InvoiceId`).
- `IX_SalesInvoiceItems_SKUId` (Non-Clustered on `SalesInvoiceItems.SKUId`).
- `IX_POSRegisters_StoreId_TerminalCode` (Composite Non-Clustered on `POSRegisters(StoreId, TerminalCode)`).
- `IX_SalesOutbox_ProcessedAt` (Non-Clustered on `SalesOutbox.ProcessedAt`).
- `IX_SalesReturns_OriginalInvoiceId` (Non-Clustered on `SalesReturns.OriginalInvoiceId`).

---

## 5. ERPInfinity_Purchase & Procurement Database (`Db_Purchase`)

### 5.1 Tables (5 Tables)

#### 1. `Suppliers`
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `VendorCode` (VARCHAR(30)), `Name` (NVARCHAR(200)), `GSTIN` (VARCHAR(15)), `Email` (VARCHAR(150)), `Phone` (VARCHAR(20)), `CreditDays` (INT), `IsActive` (BIT)
- **Constraints:** UNIQUE(`VendorCode`), UNIQUE(`GSTIN`)

#### 2. `PurchaseOrders`
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `PONumber` (VARCHAR(50)), `SupplierId` (UNIQUEIDENTIFIER, FK $\rightarrow$ Suppliers.Id), `WarehouseId` (UNIQUEIDENTIFIER), `Status` (VARCHAR(30) - 'Draft', 'Submitted', 'Approved', 'PartiallyReceived', 'Completed', 'Cancelled'), `TotalAmount` (DECIMAL(18,2)), `CreatedBy` (UNIQUEIDENTIFIER), `ApprovedBy` (UNIQUEIDENTIFIER, NULL), `CreatedAt` (DATETIME2)
- **Constraints:** UNIQUE(`PONumber`)

#### 3. `PurchaseOrderItems`
- **Columns:** `Id` (BIGINT, PK, IDENTITY), `POId` (UNIQUEIDENTIFIER, FK $\rightarrow$ PurchaseOrders.Id), `SKUId` (UNIQUEIDENTIFIER), `OrderedQuantity` (DECIMAL(18,3)), `ReceivedQuantity` (DECIMAL(18,3)), `UnitPrice` (DECIMAL(18,2)), `Total` (DECIMAL(18,2))

#### 4. `GoodsReceivedNotes` (GRN)
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `GRNNumber` (VARCHAR(50)), `POId` (UNIQUEIDENTIFIER, FK $\rightarrow$ PurchaseOrders.Id), `WarehouseId` (UNIQUEIDENTIFIER), `ChallanNumber` (VARCHAR(50)), `VehicleNumber` (VARCHAR(30)), `ReceivedDate` (DATETIME2), `ReceivedBy` (UNIQUEIDENTIFIER)
- **Constraints:** UNIQUE(`GRNNumber`)

#### 5. `GRNItems`
- **Columns:** `Id` (BIGINT, PK, IDENTITY), `GRNId` (UNIQUEIDENTIFIER, FK $\rightarrow$ GoodsReceivedNotes.Id), `SKUId` (UNIQUEIDENTIFIER), `AcceptedQty` (DECIMAL(18,3)), `RejectedQty` (DECIMAL(18,3)), `UnitPrice` (DECIMAL(18,2))

---

### 5.2 Stored Procedures (4 Procedures)

#### 1. `sp_CreatePurchaseOrder`
- **Inputs:** PO Header JSON + Line Items JSON array
- **Outputs:** `@POId` UNIQUEIDENTIFIER, `@PONumber` VARCHAR(50)
- **Functionality:** Generates draft PO with auto-incremented PO number format (`PO-YYYYMMDD-XXXX`).

#### 2. `sp_ApprovePurchaseOrder`
- **Inputs:** `@POId` UNIQUEIDENTIFIER, `@ApprovedBy` UNIQUEIDENTIFIER
- **Functionality:** Updates status to 'Approved' and fires `PurchaseOrderApprovedEvent` to RabbitMQ.

#### 3. `sp_ProcessGRNReceipt`
- **Inputs:** GRN Header JSON + Items array
- **Functionality:** Updates PO item received quantities, marks PO status, and triggers inventory stock increase.

#### 4. `sp_GetPendingPOBySupplier`
- **Inputs:** `@SupplierId` UNIQUEIDENTIFIER
- **Outputs:** List of active POs awaiting delivery.

---

### 5.3 Views (2 Views)

#### 1. `vw_POFulfillmentStatus`
- **Query:** Joins `PurchaseOrders` and `PurchaseOrderItems` comparing `OrderedQuantity` vs `ReceivedQuantity`.
- **Purpose:** Procurement tracking dashboard.

#### 2. `vw_SupplierPerformanceMetrics`
- **Query:** Computes fill-rate percentage, average lead time, and total order volume per vendor.
- **Purpose:** Vendor rating and evaluation dashboard.

---

### 5.4 Indexes (6 Indexes)

- `IX_PurchaseOrders_PONumber` (Unique Clustered/Non-Clustered on `PurchaseOrders.PONumber`).
- `IX_PurchaseOrders_SupplierId` (Non-Clustered on `PurchaseOrders.SupplierId`).
- `IX_PurchaseOrders_Status` (Non-Clustered on `PurchaseOrders.Status`).
- `IX_PurchaseOrderItems_POId` (Non-Clustered on `PurchaseOrderItems.POId`).
- `IX_GoodsReceivedNotes_POId` (Non-Clustered on `GoodsReceivedNotes.POId`).
- `IX_Suppliers_VendorCode` (Non-Clustered UNIQUE on `Suppliers.VendorCode`).

---

## 6. ERPInfinity_Warehouse Service Database (`Db_Warehouse`)

### 6.1 Tables (5 Tables)

#### 1. `Warehouses`
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `Code` (VARCHAR(20)), `Name` (NVARCHAR(150)), `Address` (NVARCHAR(300)), `IsActive` (BIT)

#### 2. `Zones`
- **Columns:** `Id` (INT, PK, IDENTITY), `WarehouseId` (UNIQUEIDENTIFIER, FK $\rightarrow$ Warehouses.Id), `ZoneCode` (VARCHAR(20)), `ZoneType` (VARCHAR(30) - 'ColdStorage', 'DryGrocery', 'Hazmat', 'HighValue')

#### 3. `Racks`
- **Columns:** `Id` (INT, PK, IDENTITY), `ZoneId` (INT, FK $\rightarrow$ Zones.Id), `RackCode` (VARCHAR(20))

#### 4. `Bins`
- **Columns:** `Id` (BIGINT, PK, IDENTITY), `RackId` (INT, FK $\rightarrow$ Racks.Id), `BinCode` (VARCHAR(30)), `MaxCapacityKg` (DECIMAL(10,2))
- **Constraints:** UNIQUE(`BinCode`)

#### 5. `PickingLists`
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `ListNumber` (VARCHAR(50)), `OrderId` (VARCHAR(50)), `WarehouseId` (UNIQUEIDENTIFIER), `Status` (VARCHAR(30) - 'Assigned', 'Picking', 'Completed'), `AssignedPickerId` (UNIQUEIDENTIFIER), `CreatedAt` (DATETIME2)

---

### 6.2 Stored Procedures (3 Procedures)

#### 1. `sp_GeneratePickingListForOrder`
- **Inputs:** `@OrderId` VARCHAR(50), `@WarehouseId` UNIQUEIDENTIFIER
- **Outputs:** Picking List Number and Bin Location sequence
- **Functionality:** Generates optimized warehouse walking route for order item picking.

#### 2. `sp_AssignBinLocation`
- **Inputs:** `@SKUId` UNIQUEIDENTIFIER, `@WarehouseId` UNIQUEIDENTIFIER, `@Quantity` DECIMAL(18,3)
- **Outputs:** Suggested `BinCode`
- **Functionality:** Finds available bin capacity based on zone requirements.

#### 3. `sp_CompletePickingTask`
- **Inputs:** `@PickingListId` UNIQUEIDENTIFIER, `@PickerId` UNIQUEIDENTIFIER
- **Functionality:** Marks list completed and advances order state to 'Packing'.

---

### 6.3 Views (2 Views)

#### 1. `vw_WarehouseBinCapacity`
- **Query:** Calculates total bins, occupied bins, and capacity percentage per zone.

#### 2. `vw_ActivePickerQueue`
- **Query:** Displays current pending order picking lists per picker.

---

### 6.4 Indexes (5 Indexes)

- `IX_Bins_BinCode` (Non-Clustered UNIQUE on `Bins.BinCode`).
- `IX_Zones_WarehouseId` (Non-Clustered on `Zones.WarehouseId`).
- `IX_Racks_ZoneId` (Non-Clustered on `Racks.ZoneId`).
- `IX_Bins_RackId` (Non-Clustered on `Bins.RackId`).
- `IX_PickingLists_OrderId_Status` (Composite Non-Clustered on `PickingLists(OrderId, Status)`).

---

## 7. ERPInfinity_Order Service Database (`Db_Order`)

### 7.1 Tables (5 Tables)

#### 1. `Carts`
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `CustomerId` (UNIQUEIDENTIFIER), `CreatedAt` (DATETIME2), `UpdatedAt` (DATETIME2)

#### 2. `CartItems`
- **Columns:** `Id` (BIGINT, PK, IDENTITY), `CartId` (UNIQUEIDENTIFIER, FK $\rightarrow$ Carts.Id), `SKUId` (UNIQUEIDENTIFIER), `Quantity` (DECIMAL(18,3)), `UnitPrice` (DECIMAL(18,2))

#### 3. `Orders`
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `OrderNumber` (VARCHAR(50)), `CustomerId` (UNIQUEIDENTIFIER), `StoreId` (UNIQUEIDENTIFIER), `OrderStatus` (VARCHAR(30) - 'Created', 'PaymentPending', 'Confirmed', 'Picking', 'Packed', 'Shipped', 'Delivered', 'Cancelled'), `TotalAmount` (DECIMAL(18,2)), `DeliveryFee` (DECIMAL(18,2)), `ShippingAddress` (NVARCHAR(500)), `CreatedAt` (DATETIME2)
- **Constraints:** UNIQUE(`OrderNumber`)

#### 4. `OrderItems`
- **Columns:** `Id` (BIGINT, PK, IDENTITY), `OrderId` (UNIQUEIDENTIFIER, FK $\rightarrow$ Orders.Id), `SKUId` (UNIQUEIDENTIFIER), `SKUCode` (VARCHAR(50)), `ProductName` (NVARCHAR(200)), `UnitPrice` (DECIMAL(18,2)), `Quantity` (DECIMAL(18,3)), `Total` (DECIMAL(18,2))

#### 5. `OrderStatusHistory`
- **Columns:** `Id` (BIGINT, PK, IDENTITY), `OrderId` (UNIQUEIDENTIFIER, FK $\rightarrow$ Orders.Id), `PreviousStatus` (VARCHAR(30)), `NewStatus` (VARCHAR(30)), `ChangedBy` (VARCHAR(100)), `ChangedAt` (DATETIME2)

---

### 7.2 Stored Procedures (4 Procedures)

#### 1. `sp_CreateOnlineOrder`
- **Inputs:** CustomerId, CartId, AddressJSON, StoreId
- **Outputs:** `@OrderId` UNIQUEIDENTIFIER, `@OrderNumber` VARCHAR(50)
- **Functionality:** Converts shopping cart into confirmed Order, reserves inventory, and clears cart.

#### 2. `sp_UpdateOrderStatus`
- **Inputs:** `@OrderId` UNIQUEIDENTIFIER, `@NewStatus` VARCHAR(30), `@UpdatedBy` VARCHAR(100)
- **Functionality:** Updates status, appends log to `OrderStatusHistory`, and publishes RabbitMQ event.

#### 3. `sp_CancelOrder`
- **Inputs:** `@OrderId` UNIQUEIDENTIFIER, `@Reason` NVARCHAR(200)
- **Functionality:** Releases reserved stock back to Inventory and triggers refund sequence.

#### 4. `sp_GetCustomerOrderHistory`
- **Inputs:** `@CustomerId` UNIQUEIDENTIFIER, `@PageIndex` INT, `@PageSize` INT
- **Outputs:** Paginated customer order list.

---

### 7.3 Views (2 Views)

#### 1. `vw_OrderFulfillmentPipeline`
- **Query:** Overview of orders grouped by status (`Confirmed`, `Picking`, `Packed`, `Shipped`).

#### 2. `vw_CustomerOrderSummary`
- **Query:** Total orders, lifetime value, and average order value per customer.

---

### 7.4 Indexes (6 Indexes)

- `IX_Orders_OrderNumber` (Unique Clustered/Non-Clustered on `Orders.OrderNumber`).
- `IX_Orders_CustomerId_CreatedAt` (Composite Non-Clustered on `Orders(CustomerId, CreatedAt)`).
- `IX_Orders_OrderStatus` (Non-Clustered on `Orders.OrderStatus`).
- `IX_OrderItems_OrderId` (Non-Clustered on `OrderItems.OrderId`).
- `IX_CartItems_CartId` (Non-Clustered on `CartItems.CartId`).
- `IX_OrderStatusHistory_OrderId` (Non-Clustered on `OrderStatusHistory.OrderId`).

---

## 8. ERPInfinity_Pricing Service Database (`Db_Pricing`)

### 8.1 Tables (4 Tables)

#### 1. `PriceLists`
- **Columns:** `Id` (INT, PK, IDENTITY), `Name` (NVARCHAR(100)), `Currency` (VARCHAR(10)), `IsDefault` (BIT)

#### 2. `SKUBasePrices`
- **Columns:** `Id` (BIGINT, PK, IDENTITY), `SKUId` (UNIQUEIDENTIFIER), `PriceListId` (INT, FK $\rightarrow$ PriceLists.Id), `MRP` (DECIMAL(18,2)), `BaseSellingPrice` (DECIMAL(18,2)), `EffectiveFrom` (DATETIME2)

#### 3. `Promotions`
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `PromoCode` (VARCHAR(30)), `Title` (NVARCHAR(150)), `DiscountType` (VARCHAR(20) - 'Percentage', 'FlatAmount', 'BuyXGetY'), `DiscountValue` (DECIMAL(18,2)), `MinOrderValue` (DECIMAL(18,2)), `StartDate` (DATETIME2), `EndDate` (DATETIME2), `IsActive` (BIT)

#### 4. `StoreSpecialPrices`
- **Columns:** `Id` (BIGINT, PK, IDENTITY), `StoreId` (UNIQUEIDENTIFIER), `SKUId` (UNIQUEIDENTIFIER), `SpecialPrice` (DECIMAL(18,2)), `EffectiveFrom` (DATETIME2), `EffectiveTo` (DATETIME2)

---

### 8.2 Stored Procedures (3 Procedures)

#### 1. `sp_CalculateItemEffectivePrice`
- **Inputs:** `@SKUId` UNIQUEIDENTIFIER, `@StoreId` UNIQUEIDENTIFIER, `@Quantity` DECIMAL(18,3), `@PromoCode` VARCHAR(30)
- **Outputs:** `@FinalUnitPrice` DECIMAL(18,2), `@DiscountAmount` DECIMAL(18,2)
- **Functionality:** Evaluates Base Price, Store Special Price, and Active Promotions to return exact checkout price. Cached in Redis.

#### 2. `sp_CreatePromotion`
- **Inputs:** Promotion JSON details
- **Functionality:** Creates promo offer and invalidates Redis pricing caches.

#### 3. `sp_ExpireOldPromotions`
- **Functionality:** Scheduled background procedure setting `IsActive = 0` for expired promotions.

---

### 8.3 Views (2 Views)

#### 1. `vw_ActiveStorePriceCatalog`
- **Query:** Combines `SKUBasePrices` and `StoreSpecialPrices` for active store pricing lists.

#### 2. `vw_CurrentRunningPromotions`
- **Query:** Active promos between `StartDate` and `EndDate`.

---

### 8.4 Indexes (5 Indexes)

- `IX_SKUBasePrices_SKUId_EffectiveFrom` (Composite Non-Clustered on `SKUBasePrices(SKUId, EffectiveFrom)`).
- `IX_StoreSpecialPrices_StoreId_SKUId` (Composite Non-Clustered on `StoreSpecialPrices(StoreId, SKUId)`).
- `IX_Promotions_PromoCode` (Non-Clustered UNIQUE on `Promotions.PromoCode`).
- `IX_Promotions_IsActive_Dates` (Composite Non-Clustered on `Promotions(IsActive, StartDate, EndDate)`).
- `IX_SKUBasePrices_PriceListId` (Non-Clustered on `SKUBasePrices.PriceListId`).

---

## 9. ERPInfinity_Payment Service Database (`Db_Payment`)

### 9.1 Tables (4 Tables)

#### 1. `PaymentTransactions`
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `TransactionReference` (VARCHAR(100)), `OrderId` (VARCHAR(50)), `Amount` (DECIMAL(18,2)), `PaymentMode` (VARCHAR(30) - 'Cash', 'UPI', 'CreditCard', 'DebitCard', 'NetBanking', 'Wallet'), `GatewayName` (VARCHAR(50)), `Status` (VARCHAR(20) - 'Pending', 'Success', 'Failed'), `CreatedAt` (DATETIME2)

#### 2. `PaymentAttempts`
- **Columns:** `Id` (BIGINT, PK, IDENTITY), `PaymentTransactionId` (UNIQUEIDENTIFIER, FK $\rightarrow$ PaymentTransactions.Id), `GatewayRequestPayload` (NVARCHAR(MAX)), `GatewayResponsePayload` (NVARCHAR(MAX)), `StatusCode` (VARCHAR(20)), `AttemptedAt` (DATETIME2)

#### 3. `Refunds`
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `RefundReference` (VARCHAR(100)), `PaymentTransactionId` (UNIQUEIDENTIFIER, FK $\rightarrow$ PaymentTransactions.Id), `Amount` (DECIMAL(18,2)), `Reason` (NVARCHAR(200)), `Status` (VARCHAR(20)), `ProcessedAt` (DATETIME2)

#### 4. `PaymentReconciliations`
- **Columns:** `Id` (BIGINT, PK, IDENTITY), `ReconciliationDate` (DATE), `GatewayName` (VARCHAR(50)), `TotalGatewayAmount` (DECIMAL(18,2)), `TotalSystemAmount` (DECIMAL(18,2)), `DiscrepancyAmount` (DECIMAL(18,2))

---

### 9.2 Stored Procedures (3 Procedures)

#### 1. `sp_RecordPaymentSuccess`
- **Inputs:** `@TransactionReference` VARCHAR(100), `@GatewayResponse` NVARCHAR(MAX)
- **Functionality:** Marks transaction successful and publishes `PaymentCompletedEvent` to RabbitMQ.

#### 2. `sp_InitiateRefund`
- **Inputs:** `@PaymentTransactionId` UNIQUEIDENTIFIER, `@Amount` DECIMAL(18,2), `@Reason` NVARCHAR(200)
- **Outputs:** `@RefundId` UNIQUEIDENTIFIER
- **Functionality:** Creates refund request and initiates gateway API callback.

#### 3. `sp_ExecuteDailyPaymentReconciliation`
- **Inputs:** `@ReconciliationDate` DATE, `@GatewayName` VARCHAR(50)
- **Functionality:** Reconciles system payment logs against bank gateway settlement files.

---

### 9.3 Views (2 Views)

#### 1. `vw_DailyPaymentModeBreakdown`
- **Query:** Grouped totals by Cash vs UPI vs Card per store.

#### 2. `vw_PendingRefundsQueue`
- **Query:** List of unprocessed refund requests.

---

### 9.4 Indexes (5 Indexes)

- `IX_PaymentTransactions_TransactionReference` (Non-Clustered UNIQUE on `PaymentTransactions.TransactionReference`).
- `IX_PaymentTransactions_OrderId` (Non-Clustered on `PaymentTransactions.OrderId`).
- `IX_PaymentTransactions_Status_CreatedAt` (Composite Non-Clustered on `PaymentTransactions(Status, CreatedAt)`).
- `IX_Refunds_PaymentTransactionId` (Non-Clustered on `Refunds.PaymentTransactionId`).
- `IX_PaymentAttempts_PaymentTransactionId` (Non-Clustered on `PaymentAttempts.PaymentTransactionId`).

---

## 10. ERPInfinity_Finance Service Database (`Db_Finance`)

### 10.1 Tables (5 Tables)

#### 1. `Accounts`
- **Columns:** `Id` (INT, PK, IDENTITY), `AccountCode` (VARCHAR(20)), `AccountName` (NVARCHAR(150)), `AccountType` (VARCHAR(30) - 'Asset', 'Liability', 'Equity', 'Revenue', 'Expense')

#### 2. `JournalEntries`
- **Columns:** `Id` (UNIQUEIDENTIFIER, PK), `VoucherNumber` (VARCHAR(50)), `EntryDate` (DATETIME2), `ReferenceNumber` (VARCHAR(100)), `Description` (NVARCHAR(300)), `CreatedAt` (DATETIME2)

#### 3. `JournalEntryLines`
- **Columns:** `Id` (BIGINT, PK, IDENTITY), `JournalEntryId` (UNIQUEIDENTIFIER, FK $\rightarrow$ JournalEntries.Id), `AccountId` (INT, FK $\rightarrow$ Accounts.Id), `DebitAmount` (DECIMAL(18,2)), `CreditAmount` (DECIMAL(18,2))

#### 4. `SupplierLedgers`
- **Columns:** `Id` (BIGINT, PK, IDENTITY), `SupplierId` (UNIQUEIDENTIFIER), `GRNNumber` (VARCHAR(50)), `InvoiceAmount` (DECIMAL(18,2)), `PaidAmount` (DECIMAL(18,2)), `BalanceAmount` AS (`InvoiceAmount` - `PaidAmount`), `DueDate` (DATE)

#### 5. `TaxSettlements`
- **Columns:** `Id` (BIGINT, PK, IDENTITY), `PeriodMonth` (INT), `PeriodYear` (INT), `TotalOutputGST` (DECIMAL(18,2)), `TotalInputGST` (DECIMAL(18,2)), `NetTaxPayable` (DECIMAL(18,2))

---

### 10.2 Stored Procedures (4 Procedures)

#### 1. `sp_PostJournalEntry`
- **Inputs:** Header JSON + Lines JSON array
- **Functionality:** Validates total Debit equals total Credit before committing double-entry accounting record.

#### 2. `sp_GenerateTrialBalance`
- **Inputs:** `@StartDate` DATE, `@EndDate` DATE
- **Outputs:** Debit and Credit balances per GL Account.

#### 3. `sp_ProcessSupplierPaymentSettlement`
- **Inputs:** `@SupplierId` UNIQUEIDENTIFIER, `@PaymentAmount` DECIMAL(18,2), `@PaymentReference` VARCHAR(50)
- **Functionality:** Applies payment against pending supplier ledger invoices.

#### 4. `sp_CalculateGSTSummary`
- **Inputs:** `@Month` INT, `@Year` INT
- **Outputs:** Output Tax vs Input Tax ledger breakdown.

---

### 10.3 Views (2 Views)

#### 1. `vw_GeneralLedgerSummary`
- **Query:** Trial balance aggregation per General Ledger account.

#### 2. `vw_SupplierPayablesAging`
- **Query:** Aging report for vendor payables (0-30 days, 31-60 days, 61+ days).

---

### 10.4 Indexes (6 Indexes)

- `IX_JournalEntries_VoucherNumber` (Non-Clustered UNIQUE on `JournalEntries.VoucherNumber`).
- `IX_JournalEntries_EntryDate` (Non-Clustered on `JournalEntries.EntryDate`).
- `IX_JournalEntryLines_JournalEntryId` (Non-Clustered on `JournalEntryLines.JournalEntryId`).
- `IX_JournalEntryLines_AccountId` (Non-Clustered on `JournalEntryLines.AccountId`).
- `IX_SupplierLedgers_SupplierId_DueDate` (Composite Non-Clustered on `SupplierLedgers(SupplierId, DueDate)`).
- `IX_Accounts_AccountCode` (Non-Clustered UNIQUE on `Accounts.AccountCode`).

---

## 11. ERPInfinity_Store & Infrastructure Services Databases

### 11.1 ERPInfinity_Store Service (`Db_Store`)
- **Tables (3):** `Stores`, `StoreTerminals`, `StoreUsers`
- **Stored Procedures (2):** `sp_RegisterNewStore`, `sp_AssignStoreTerminal`
- **Views (1):** `vw_ActiveStoreNetwork`
- **Indexes (4):** Non-clustered on `StoreCode`, `TerminalCode`, `ManagerId`.

### 11.2 ERPInfinity_Notification Service (`Db_Notification`)
- **Tables (2):** `NotificationTemplates`, `NotificationLogs`
- **Stored Procedures (2):** `sp_QueueNotification`, `sp_MarkNotificationSent`
- **Views (1):** `vw_FailedNotificationRetryQueue`
- **Indexes (3):** Non-clustered on `Recipient`, `Status`, `CreatedAt`.

---

## 12. MongoDB Read Projection Collections (CQRS Engine)

MongoDB hosts denormalized JSON document collections fed by RabbitMQ event consumers for fast Query UI rendering:

1. **`Mongo_ProductCatalogRead`:** Embedded product, category, brand, pricing, and barcode document for rapid web/mobile catalog search.
2. **`Mongo_InventoryDashboardRead`:** Real-time stock levels aggregated across all stores and central warehouses.
3. **`Mongo_SalesAnalyticsRead`:** Live hourly store billing charts, top-selling SKUs, and counter activity analytics.
4. **`Mongo_Customer360Read`:** Unified customer purchase history, active delivery orders, and loyalty point totals.
5. **`Mongo_AuditLogsRead`:** Centralized system compliance and security audit logs.

---

## 13. System Database Summary Matrix

```text
========================================================================================
DATABASE NAME            TABLES   PROCEDURES   VIEWS   INDEXES   PRIMARY STORAGE ROLE
========================================================================================
Db_Identity                 5         3          2        6      AuthN, AuthZ, Permissions
Db_Product                  6         4          2        7      Product Master & Barcodes
Db_Inventory                5         5          2        8      Stock Ledger & Movements
Db_Sales                    5         4          2        7      POS Invoicing & Billing
Db_Purchase                 5         4          2        6      PO, Vendor & GRN Receipts
Db_Warehouse                5         3          2        5      Racks, Bins & Picking Lists
Db_Order                    5         4          2        6      Cart & Order Fulfillment
Db_Pricing                  4         3          2        5      Base Prices & Promotions
Db_Payment                  4         3          2        5      Gateway Transactions
Db_Finance                  5         4          2        6      General Ledger & Tax
Db_Store                    3         2          1        4      Physical Store Network
Db_Notification             2         2          1        3      SMS/Email Logs & Templates
----------------------------------------------------------------------------------------
TOTAL RELATIONAL (SQL):    54        41         22       68      ACID Transactional Data
TOTAL NOSQL (MongoDB):     14 Collections                        CQRS Read Projections
========================================================================================
```
