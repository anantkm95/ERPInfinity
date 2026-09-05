# ERPInfinity – Complete Directory of All Database Tables

> **Project Name:** ERPInfinity  
> **Database Architecture:** Database-Per-Service (Microsoft SQL Server 2022)  
> **Total Relational Tables:** 54 Tables across 12 Microservice Databases  

---

## Complete Database & Table Directory Matrix

| # | Database Name | Table Name | Primary Key (PK) | Key Foreign Keys (FK) | Functional Description |
|---|---|---|---|---|---|
| 1 | `Db_Identity` | **`Users`** | `Id` (GUID) | None | System user authentication profiles, passwords & emails. |
| 2 | `Db_Identity` | **`Roles`** | `Id` (INT) | None | Enterprise system role definitions (Admin, Manager, Cashier). |
| 3 | `Db_Identity` | **`UserRoles`** | `(UserId, RoleId)` | `UserId`, `RoleId` | Junction table mapping Users to Roles. |
| 4 | `Db_Identity` | **`Permissions`** | `Id` (INT) | None | Granular security permission codes (e.g. `Sales.Create`). |
| 5 | `Db_Identity` | **`RolePermissions`** | `(RoleId, PermissionId)` | `RoleId`, `PermissionId` | Junction table mapping Roles to Permissions. |
| 6 | `Db_Product` | **`Categories`** | `Id` (INT) | `ParentCategoryId` | Hierarchical product taxonomy (Grocery $\rightarrow$ Spices $\rightarrow$ Salt). |
| 7 | `Db_Product` | **`Brands`** | `Id` (INT) | None | Master brand directory (Tata, Nestle, Unilever). |
| 8 | `Db_Product` | **`Products`** | `Id` (GUID) | `BrandId`, `CategoryId` | Master product header records. |
| 9 | `Db_Product` | **`ProductSKUs`** | `Id` (GUID) | `ProductId` | Specific SKU pack sizes, weight & selling prices. |
| 10 | `Db_Product` | **`Barcodes`** | `Id` (BIGINT) | `SKUId` | EAN/UPC barcodes for sub-2ms POS scanner scanning. |
| 11 | `Db_Product` | **`ProductOutbox`** | `Id` (GUID) | None | Outbox pattern events for RabbitMQ integration. |
| 12 | `Db_Inventory` | **`Stocks`** | `Id` (BIGINT) | None | Current physical, reserved & available stock levels. |
| 13 | `Db_Inventory` | **`StockTransactions`** | `Id` (BIGINT) | None | Immutable transaction ledger for every stock change. |
| 14 | `Db_Inventory` | **`StockAdjustments`** | `Id` (GUID) | None | Inventory correction logs (damage, audit variance). |
| 15 | `Db_Inventory` | **`StockTransfers`** | `Id` (GUID) | None | Warehouse-to-store stock shipment headers. |
| 16 | `Db_Inventory` | **`StockTransferItems`** | `Id` (BIGINT) | `TransferId` | Line items inside a stock transfer shipment. |
| 17 | `Db_Sales` | **`POSRegisters`** | `Id` (GUID) | None | Cash counter terminal sessions & cash balances. |
| 18 | `Db_Sales` | **`SalesInvoices`** | `Id` (GUID) | None | Customer retail billing invoice headers. |
| 19 | `Db_Sales` | **`SalesInvoiceItems`** | `Id` (BIGINT) | `InvoiceId` | Individual product lines billed on an invoice. |
| 20 | `Db_Sales` | **`SalesReturns`** | `Id` (GUID) | `OriginalInvoiceId` | Product returns and cashier refund receipts. |
| 21 | `Db_Sales` | **`SalesOutbox`** | `Id` (GUID) | None | Outbox event queue for POS sale completion events. |
| 22 | `Db_Purchase` | **`Suppliers`** | `Id` (GUID) | None | Procurement vendor master, GSTIN & credit days. |
| 23 | `Db_Purchase` | **`PurchaseOrders`** | `Id` (GUID) | `SupplierId` | Purchase Order headers and approval statuses. |
| 24 | `Db_Purchase` | **`PurchaseOrderItems`** | `Id` (BIGINT) | `POId` | SKUs, ordered quantities and unit purchase prices. |
| 25 | `Db_Purchase` | **`GoodsReceivedNotes`** | `Id` (GUID) | `POId` | Goods Received Notes (GRN) for warehouse deliveries. |
| 26 | `Db_Purchase` | **`GRNItems`** | `Id` (BIGINT) | `GRNId` | Accepted vs rejected item quantities on delivery. |
| 27 | `Db_Warehouse` | **`Warehouses`** | `Id` (GUID) | None | Central distribution center & warehouse facilities. |
| 28 | `Db_Warehouse` | **`Zones`** | `Id` (INT) | `WarehouseId` | Warehouse storage zones (ColdStorage, DryGrocery). |
| 29 | `Db_Warehouse` | **`Racks`** | `Id` (INT) | `ZoneId` | Rack structures inside warehouse zones. |
| 30 | `Db_Warehouse` | **`Bins`** | `Id` (BIGINT) | `RackId` | Specific bin locations and weight capacity limits. |
| 31 | `Db_Warehouse` | **`PickingLists`** | `Id` (GUID) | None | Order picking list routes for warehouse staff. |
| 32 | `Db_Order` | **`Carts`** | `Id` (GUID) | None | E-commerce active shopping carts. |
| 33 | `Db_Order` | **`CartItems`** | `Id` (BIGINT) | `CartId` | Items placed in a customer shopping cart. |
| 34 | `Db_Order` | **`Orders`** | `Id` (GUID) | None | E-commerce order headers and delivery status. |
| 35 | `Db_Order` | **`OrderItems`** | `Id` (BIGINT) | `OrderId` | Line items included in an online customer order. |
| 36 | `Db_Order` | **`OrderStatusHistory`** | `Id` (BIGINT) | `OrderId` | Lifecycle audit log of order status changes. |
| 37 | `Db_Pricing` | **`PriceLists`** | `Id` (INT) | None | Base price list definitions & currencies. |
| 38 | `Db_Pricing` | **`SKUBasePrices`** | `Id` (BIGINT) | `PriceListId` | Master SKU base prices and effective dates. |
| 39 | `Db_Pricing` | **`Promotions`** | `Id` (GUID) | None | Promotional campaign offers, discounts & coupons. |
| 40 | `Db_Pricing` | **`StoreSpecialPrices`** | `Id` (BIGINT) | None | Store-specific price overrides. |
| 41 | `Db_Payment` | **`PaymentTransactions`** | `Id` (GUID) | None | Payment transaction records (Cash/UPI/Card). |
| 42 | `Db_Payment` | **`PaymentAttempts`** | `Id` (BIGINT) | `PaymentTransactionId` | Raw gateway payload logs and status codes. |
| 43 | `Db_Payment` | **`Refunds`** | `Id` (GUID) | `PaymentTransactionId` | Refund request logs and status tracking. |
| 44 | `Db_Payment` | **`PaymentReconciliations`** | `Id` (BIGINT) | None | Daily bank gateway settlement reconciliations. |
| 45 | `Db_Finance` | **`Accounts`** | `Id` (INT) | None | General Ledger Chart of Accounts (Assets/Liabilities). |
| 46 | `Db_Finance` | **`JournalEntries`** | `Id` (GUID) | None | Header for financial accounting journal vouchers. |
| 47 | `Db_Finance` | **`JournalEntryLines`** | `Id` (BIGINT) | `JournalEntryId`, `AccountId` | Double-entry Debit and Credit transaction lines. |
| 48 | `Db_Finance` | **`SupplierLedgers`** | `Id` (BIGINT) | None | Vendor payables, pending GRN bills & payments. |
| 49 | `Db_Finance` | **`TaxSettlements`** | `Id` (BIGINT) | None | GST input/output tax balance calculation logs. |
| 50 | `Db_Store` | **`Stores`** | `Id` (GUID) | None | Physical supermarket store locations. |
| 51 | `Db_Store` | **`StoreTerminals`** | `Id` (GUID) | `StoreId` | Register terminals assigned to stores. |
| 52 | `Db_Store` | **`StoreUsers`** | `(StoreId, UserId)` | `StoreId` | Staff assigned to specific physical stores. |
| 53 | `Db_Notification` | **`NotificationTemplates`** | `Id` (INT) | None | Email, SMS, and WhatsApp message templates. |
| 54 | `Db_Notification` | **`NotificationLogs`** | `Id` (BIGINT) | None | Log of sent & failed notifications. |

---

## Breakdown by Microservice Database

### 1. `Db_Identity` (5 Tables)
- `Users`
- `Roles`
- `UserRoles`
- `Permissions`
- `RolePermissions`

### 2. `Db_Product` (6 Tables)
- `Categories`
- `Brands`
- `Products`
- `ProductSKUs`
- `Barcodes`
- `ProductOutbox`

### 3. `Db_Inventory` (5 Tables)
- `Stocks`
- `StockTransactions`
- `StockAdjustments`
- `StockTransfers`
- `StockTransferItems`

### 4. `Db_Sales` (5 Tables)
- `POSRegisters`
- `SalesInvoices`
- `SalesInvoiceItems`
- `SalesReturns`
- `SalesOutbox`

### 5. `Db_Purchase` (5 Tables)
- `Suppliers`
- `PurchaseOrders`
- `PurchaseOrderItems`
- `GoodsReceivedNotes`
- `GRNItems`

### 6. `Db_Warehouse` (5 Tables)
- `Warehouses`
- `Zones`
- `Racks`
- `Bins`
- `PickingLists`

### 7. `Db_Order` (5 Tables)
- `Carts`
- `CartItems`
- `Orders`
- `OrderItems`
- `OrderStatusHistory`

### 8. `Db_Pricing` (4 Tables)
- `PriceLists`
- `SKUBasePrices`
- `Promotions`
- `StoreSpecialPrices`

### 9. `Db_Payment` (4 Tables)
- `PaymentTransactions`
- `PaymentAttempts`
- `Refunds`
- `PaymentReconciliations`

### 10. `Db_Finance` (5 Tables)
- `Accounts`
- `JournalEntries`
- `JournalEntryLines`
- `SupplierLedgers`
- `TaxSettlements`

### 11. `Db_Store` (3 Tables)
- `Stores`
- `StoreTerminals`
- `StoreUsers`

### 12. `Db_Notification` (2 Tables)
- `NotificationTemplates`
- `NotificationLogs`
