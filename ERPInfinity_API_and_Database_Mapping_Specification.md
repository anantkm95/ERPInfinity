# ERPInfinity – Microservices APIs & Database Mapping Specification

> **Project Name:** ERPInfinity  
> **Target Architecture:** Distributed Microservices + CQRS + Event-Driven Architecture  
> **Specification Focus:** End-to-End Mapping between REST/gRPC API Endpoints, CQRS Handlers, SQL Tables, Stored Procedures, Views, Indexes, MongoDB Read Collections, and Integration Events.

---

## 1. System Mapping Overview & Integration Architecture

In **ERPInfinity**, every API request passes through the **YARP API Gateway**, where security claims are validated. Requests are routed to microservices executing Clean Architecture:

```text
HTTP / gRPC Request ──> Controller ──> CQRS Command / Query
                                             │
                       ┌─────────────────────┴─────────────────────┐
                       ▼                                           ▼
            [SQL Transactional Path]                    [CQRS Read Path]
           Stored Procedure / EF Core                MongoDB / Redis Cache
                       │                                           ▲
                       ▼                                           │
                Outbox Table ──> RabbitMQ Event Bus ───────────────┘
```

---

## 2. Identity Service (`Db_Identity`) API-to-Database Mapping

### 2.1 API Endpoint & Database Mapping Matrix

| HTTP Method & Route | CQRS Command / Query | Target SQL Tables | Stored Procedure / View | Indexes Used | Events Emitted / Consumed |
|---|---|---|---|---|---|
| `POST /api/v1/identity/login` | `AuthenticateUserQuery` | `Users`, `UserRoles`, `Roles`, `RolePermissions`, `Permissions` | `sp_AuthenticateUser` / `vw_UserSecurityProfile` | `IX_Users_Username`, `IX_UserRoles_UserId` | `UserLoggedInEvent` (Pub) |
| `POST /api/v1/identity/users` | `CreateUserCommand` | `Users`, `UserRoles` | `sp_AssignUserRole` | `IX_Users_Email` | `UserCreatedEvent` (Pub) |
| `GET /api/v1/identity/users/{id}/permissions` | `GetUserPermissionsQuery` | `Users`, `Permissions` | `sp_GetUserPermissions` | `IX_Permissions_PermissionCode` | None |
| `PUT /api/v1/identity/roles/assign` | `AssignRoleCommand` | `UserRoles` | `sp_AssignUserRole` | `IX_UserRoles_UserId` | `UserRoleUpdatedEvent` (Pub) |
| `GET /api/v1/identity/matrix` | `GetSecurityMatrixQuery` | `Roles`, `Permissions` | `vw_ActiveRolePermissionMatrix` | `IX_RolePermissions_RoleId` | None |

### 2.2 Functional Flow Details
- **Login Request:** Client submits username and password hash $\rightarrow$ `AuthenticateUserQuery` executes `sp_AuthenticateUser` against `Db_Identity` $\rightarrow$ Validates `IX_Users_Username` index $\rightarrow$ Generates JWT with permission claims.

---

## 3. Product Service (`Db_Product`) API-to-Database Mapping

### 3.1 API Endpoint & Database Mapping Matrix

| HTTP Method & Route / gRPC Method | CQRS Command / Query | Target SQL Tables | Stored Procedure / View | Indexes Used | MongoDB & RabbitMQ Mapping |
|---|---|---|---|---|---|
| `GET /api/v1/products/barcode/{code}` | `LookupProductByBarcodeQuery` | `Products`, `ProductSKUs`, `Barcodes` | `sp_LookupProductByBarcode` / `vw_ActivePOSBarcodes` | `IX_Barcodes_BarcodeNumber` (Clustered) | Reads Redis cache first. Misses query SQL $< 2\text{ms}$. |
| `POST /api/v1/products` | `CreateProductCommand` | `Products`, `ProductSKUs`, `Barcodes`, `ProductOutbox` | `sp_CreateProductWithSKUs` | `IX_Products_CategoryId`, `IX_ProductSKUs_SKUCode` | Emits `ProductCreatedEvent` $\rightarrow$ Syncs `Mongo_ProductCatalogRead` |
| `PUT /api/v1/products/{id}/price` | `UpdateSKUPricingCommand` | `ProductSKUs`, `ProductOutbox` | `sp_UpdateSKUPricing` | `IX_ProductSKUs_ProductId` | Emits `ProductPriceUpdatedEvent` $\rightarrow$ Updates Redis Price Cache |
| `gRPC ProductGrpc.GetSKUDetails` | `GetSKUDetailsGrpcQuery` | `ProductSKUs`, `Barcodes` | EF Core Compiled Query | `IX_ProductSKUs_SKUCode` | High-speed inter-service gRPC lookup. |

---

## 4. Inventory Service (`Db_Inventory`) API-to-Database Mapping

### 4.1 API Endpoint & Database Mapping Matrix

| HTTP Method & Route | CQRS Command / Query | Target SQL Tables | Stored Procedure / View | Indexes Used | MongoDB & RabbitMQ Mapping |
|---|---|---|---|---|---|
| `POST /api/v1/inventory/pos-deduct` | `DeductPOSInventoryCommand` | `Stocks`, `StockTransactions` | `sp_DeductInventoryForPOS` | `IX_Stocks_LocationId_SKUId` | Consumes `SalesInvoiceCreatedEvent` $\rightarrow$ Syncs `Mongo_InventoryDashboardRead` |
| `POST /api/v1/inventory/grn-receive` | `ReceiveGRNStockCommand` | `Stocks`, `StockTransactions` | `sp_ReceiveGRNStock` | `IX_StockTransactions_LocationId_SKUId` | Consumes `GoodsReceivedEvent` $\rightarrow$ Updates stock balance |
| `POST /api/v1/inventory/reserve` | `ReserveStockCommand` | `Stocks` | `sp_ReserveInventoryForOnlineOrder` | `IX_Stocks_AvailableQuantity` | Triggered by Order Service via gRPC |
| `POST /api/v1/inventory/adjust` | `AdjustStockCommand` | `Stocks`, `StockTransactions`, `StockAdjustments` | `sp_ExecuteStockAdjustment` | `IX_StockAdjustments_LocationId` | Emits `StockAdjustedEvent` |
| `GET /api/v1/inventory/low-stock` | `GetLowStockAlertsQuery` | `Stocks` | `sp_GetLowStockAlerts` / `vw_LocationStockSummary` | `IX_Stocks_AvailableQuantity` | Reads low stock thresholds for automated re-ordering |

---

## 5. Sales / POS Service (`Db_Sales`) API-to-Database Mapping

### 5.1 API Endpoint & Database Mapping Matrix

| HTTP Method & Route | CQRS Command / Query | Target SQL Tables | Stored Procedure / View | Indexes Used | MongoDB & RabbitMQ Mapping |
|---|---|---|---|---|---|
| `POST /api/v1/sales/invoices` | `CreateSalesInvoiceCommand` | `SalesInvoices`, `SalesInvoiceItems`, `SalesOutbox` | `sp_CreateSalesInvoice` | `IX_SalesInvoices_InvoiceNumber` | Emits `SalesInvoiceCreatedEvent` $\rightarrow$ Syncs `Mongo_SalesAnalyticsRead` |
| `POST /api/v1/sales/registers/open` | `OpenPOSRegisterCommand` | `POSRegisters` | `sp_OpenPOSRegister` | `IX_POSRegisters_StoreId_TerminalCode` | Session initialized in POS Local DB |
| `POST /api/v1/sales/registers/close` | `ClosePOSRegisterCommand` | `POSRegisters` | `sp_ClosePOSRegister` | `IX_POSRegisters_StoreId_TerminalCode` | Reconciles cash $\rightarrow$ Emits `RegisterClosedEvent` |
| `POST /api/v1/sales/returns` | `ProcessSalesReturnCommand` | `SalesReturns`, `SalesInvoices` | `sp_ProcessSalesReturn` | `IX_SalesReturns_OriginalInvoiceId` | Emits `SalesReturnCreatedEvent` |
| `GET /api/v1/sales/reports/daily` | `GetDailySalesQuery` | Read Only (MongoDB) | `vw_DailyStoreSalesSummary` | `IX_SalesInvoices_StoreId_CreatedAt` | Queries `Mongo_SalesAnalyticsRead` |

---

## 6. Purchase & Procurement Service (`Db_Purchase`) API-to-Database Mapping

### 6.1 API Endpoint & Database Mapping Matrix

| HTTP Method & Route | CQRS Command / Query | Target SQL Tables | Stored Procedure / View | Indexes Used | MongoDB & RabbitMQ Mapping |
|---|---|---|---|---|---|
| `POST /api/v1/purchase/orders` | `CreatePurchaseOrderCommand` | `PurchaseOrders`, `PurchaseOrderItems` | `sp_CreatePurchaseOrder` | `IX_PurchaseOrders_PONumber` | Emits `PurchaseOrderCreatedEvent` |
| `PUT /api/v1/purchase/orders/{id}/approve` | `ApprovePurchaseOrderCommand` | `PurchaseOrders` | `sp_ApprovePurchaseOrder` | `IX_PurchaseOrders_Status` | Emits `PurchaseOrderApprovedEvent` $\rightarrow$ Alerts Warehouse |
| `POST /api/v1/purchase/grn` | `ProcessGRNReceiptCommand` | `GoodsReceivedNotes`, `GRNItems`, `PurchaseOrderItems` | `sp_ProcessGRNReceipt` | `IX_GoodsReceivedNotes_POId` | Emits `GoodsReceivedEvent` $\rightarrow$ Inventory & Finance |
| `GET /api/v1/purchase/suppliers/{id}/pending` | `GetPendingPOsQuery` | `PurchaseOrders` | `sp_GetPendingPOBySupplier` / `vw_POFulfillmentStatus` | `IX_PurchaseOrders_SupplierId` | Queries active vendor POs |

---

## 7. Warehouse Service (`Db_Warehouse`) API-to-Database Mapping

### 7.1 API Endpoint & Database Mapping Matrix

| HTTP Method & Route | CQRS Command / Query | Target SQL Tables | Stored Procedure / View | Indexes Used | MongoDB & RabbitMQ Mapping |
|---|---|---|---|---|---|
| `POST /api/v1/warehouse/picking-lists` | `GeneratePickingListCommand` | `PickingLists`, `Bins`, `Racks` | `sp_GeneratePickingListForOrder` | `IX_PickingLists_OrderId_Status` | Consumes `OrderConfirmedEvent` |
| `PUT /api/v1/warehouse/picking-lists/{id}/complete` | `CompletePickingTaskCommand` | `PickingLists` | `sp_CompletePickingTask` | `IX_PickingLists_OrderId_Status` | Emits `OrderPickedEvent` |
| `GET /api/v1/warehouse/bin-capacity` | `GetBinCapacityQuery` | `Bins`, `Zones` | `sp_AssignBinLocation` / `vw_WarehouseBinCapacity` | `IX_Bins_BinCode` | Displays bin availability status |

---

## 8. Order Service (`Db_Order`) API-to-Database Mapping

### 8.1 API Endpoint & Database Mapping Matrix

| HTTP Method & Route | CQRS Command / Query | Target SQL Tables | Stored Procedure / View | Indexes Used | MongoDB & RabbitMQ Mapping |
|---|---|---|---|---|---|
| `POST /api/v1/orders/checkout` | `CreateOnlineOrderCommand` | `Carts`, `Orders`, `OrderItems`, `OrderStatusHistory` | `sp_CreateOnlineOrder` | `IX_Orders_OrderNumber` | Emits `OrderCreatedEvent` $\rightarrow$ Triggers Payment & Inventory Reserve |
| `PUT /api/v1/orders/{id}/status` | `UpdateOrderStatusCommand` | `Orders`, `OrderStatusHistory` | `sp_UpdateOrderStatus` | `IX_Orders_OrderStatus` | Emits `OrderStatusUpdatedEvent` |
| `POST /api/v1/orders/{id}/cancel` | `CancelOrderCommand` | `Orders`, `OrderStatusHistory` | `sp_CancelOrder` | `IX_Orders_CustomerId_CreatedAt` | Emits `OrderCancelledEvent` $\rightarrow$ Releases Inventory |

---

## 9. Pricing & Promotion Service (`Db_Pricing`) API-to-Database Mapping

### 9.1 API Endpoint & Database Mapping Matrix

| HTTP Method & Route / gRPC Method | CQRS Command / Query | Target SQL Tables | Stored Procedure / View | Indexes Used | Redis Cache & RabbitMQ Mapping |
|---|---|---|---|---|---|
| `POST /api/v1/pricing/calculate` | `CalculatePriceQuery` | `SKUBasePrices`, `StoreSpecialPrices`, `Promotions` | `sp_CalculateItemEffectivePrice` | `IX_SKUBasePrices_SKUId_EffectiveFrom`, `IX_StoreSpecialPrices_StoreId_SKUId` | Evaluates prices; caches result in Redis (`TTL = 1 hr`). |
| `POST /api/v1/pricing/promotions` | `CreatePromotionCommand` | `Promotions` | `sp_CreatePromotion` | `IX_Promotions_PromoCode` | Invalidates Redis price cache |
| `gRPC PricingGrpc.GetEffectivePrice` | `GetEffectivePriceGrpcQuery` | `SKUBasePrices` | EF Core Compiled Query | `IX_SKUBasePrices_SKUId_EffectiveFrom` | High-speed gRPC call invoked by Order/Sales Services. |

---

## 10. Payment Service (`Db_Payment`) API-to-Database Mapping

### 10.1 API Endpoint & Database Mapping Matrix

| HTTP Method & Route | CQRS Command / Query | Target SQL Tables | Stored Procedure / View | Indexes Used | MongoDB & RabbitMQ Mapping |
|---|---|---|---|---|---|
| `POST /api/v1/payments/callback` | `RecordPaymentSuccessCommand` | `PaymentTransactions`, `PaymentAttempts` | `sp_RecordPaymentSuccess` | `IX_PaymentTransactions_TransactionReference` | Emits `PaymentCompletedEvent` $\rightarrow$ Order & Finance Services |
| `POST /api/v1/payments/refund` | `InitiateRefundCommand` | `Refunds`, `PaymentTransactions` | `sp_InitiateRefund` | `IX_Refunds_PaymentTransactionId` | Emits `RefundInitiatedEvent` |
| `POST /api/v1/payments/reconcile` | `ReconcilePaymentsCommand` | `PaymentReconciliations`, `PaymentTransactions` | `sp_ExecuteDailyPaymentReconciliation` | `IX_PaymentTransactions_Status_CreatedAt` | Bank settlement file reconciliation |

---

## 11. Finance Service (`Db_Finance`) API-to-Database Mapping

### 11.1 API Endpoint & Database Mapping Matrix

| HTTP Method & Route | CQRS Command / Query | Target SQL Tables | Stored Procedure / View | Indexes Used | MongoDB & RabbitMQ Mapping |
|---|---|---|---|---|---|
| `POST /api/v1/finance/journal-entries` | `PostJournalEntryCommand` | `JournalEntries`, `JournalEntryLines` | `sp_PostJournalEntry` | `IX_JournalEntries_VoucherNumber` | Double-entry validation $\rightarrow$ Financial Ledger |
| `POST /api/v1/finance/supplier-settlement` | `ProcessSupplierSettlementCommand` | `SupplierLedgers` | `sp_ProcessSupplierPaymentSettlement` | `IX_SupplierLedgers_SupplierId_DueDate` | Consumes `GoodsReceivedEvent` |
| `GET /api/v1/finance/trial-balance` | `GetTrialBalanceQuery` | `Accounts`, `JournalEntryLines` | `sp_GenerateTrialBalance` / `vw_GeneralLedgerSummary` | `IX_JournalEntryLines_AccountId` | Generates GL Trial Balance Report |

---

## 12. Store & Infrastructure Services API Mapping

### 12.1 Store Service (`Db_Store`)
- `POST /api/v1/stores` $\rightarrow$ `Stores` table $\rightarrow$ `sp_RegisterNewStore` $\rightarrow$ `IX_Stores_StoreCode`.
- `POST /api/v1/stores/terminals` $\rightarrow$ `StoreTerminals` table $\rightarrow$ `sp_AssignStoreTerminal`.

### 12.2 Notification Service (`Db_Notification`)
- `POST /api/v1/notifications/send` $\rightarrow$ `NotificationLogs` $\rightarrow$ `sp_QueueNotification`. Consumes `PaymentCompletedEvent`, `OrderShippedEvent`.

---

## 13. Comprehensive Microservices API & Database Mapping Summary

```text
====================================================================================================================================
MICROSERVICE          API ENDPOINT / gRPC METHOD             COMMAND / QUERY CLASS           PRIMARY SQL TABLES & PROCEDURES        MONGODB / REDIS READ TARGET
====================================================================================================================================
Identity Service      POST /api/v1/identity/login            AuthenticateUserQuery           Users, sp_AuthenticateUser             Redis Token Sessions
Product Service       GET /api/v1/products/barcode/{code}    LookupProductByBarcodeQuery     Barcodes, sp_LookupProductByBarcode    Redis Price & SKU Cache
Product Service       POST /api/v1/products                  CreateProductCommand            Products, sp_CreateProductWithSKUs     Mongo_ProductCatalogRead
Inventory Service     POST /api/v1/inventory/pos-deduct      DeductPOSInventoryCommand       Stocks, sp_DeductInventoryForPOS       Mongo_InventoryDashboardRead
Inventory Service     POST /api/v1/inventory/grn-receive     ReceiveGRNStockCommand          Stocks, sp_ReceiveGRNStock             Mongo_InventoryDashboardRead
Sales Service         POST /api/v1/sales/invoices            CreateSalesInvoiceCommand       SalesInvoices, sp_CreateSalesInvoice   Mongo_SalesAnalyticsRead
Purchase Service      POST /api/v1/purchase/orders           CreatePurchaseOrderCommand      PurchaseOrders, sp_CreatePurchaseOrder Mongo_PurchaseRead
Purchase Service      POST /api/v1/purchase/grn              ProcessGRNReceiptCommand        GoodsReceivedNotes, sp_ProcessGRNReceipt Mongo_PurchaseRead
Warehouse Service     POST /api/v1/warehouse/picking-lists   GeneratePickingListCommand      PickingLists, sp_GeneratePickingList   Mongo_WarehouseRead
Order Service         POST /api/v1/orders/checkout           CreateOnlineOrderCommand        Orders, sp_CreateOnlineOrder           Mongo_Customer360Read
Pricing Service       POST /api/v1/pricing/calculate         CalculatePriceQuery             SKUBasePrices, sp_CalculatePrice       Redis Price Cache
Payment Service       POST /api/v1/payments/callback         RecordPaymentSuccessCommand     PaymentTransactions, sp_RecordPayment  Mongo_FinanceRead
Finance Service       POST /api/v1/finance/journal-entries   PostJournalEntryCommand         JournalEntries, sp_PostJournalEntry    Mongo_FinanceRead
====================================================================================================================================
```
