# ERPInfinity – Enterprise Retail ERP Technical Architecture & Implementation Blueprint

> **Project Name:** ERPInfinity  
> **System Scope:** Large-Scale Enterprise Retail ERP (DMart-Style Supermarket & E-Commerce Platform)  
> **Target Architecture:** Distributed Microservices + CQRS + Event-Driven Architecture  
> **Technology Stack:** ASP.NET Core (.NET 8/9), Angular 18+, YARP API Gateway, SQL Server, MongoDB, RabbitMQ, Redis, gRPC  

---

## 1. Executive Summary & System Vision

**ERPInfinity** is a production-grade, enterprise-scale retail ERP platform designed for high-throughput retail operations, hypermarkets, supermarkets, and e-commerce platforms. The system manages end-to-end retail operations, including multi-store administration, central warehouse logistics, procurement, physical POS counter billing, online order fulfillment, dynamic multi-tier pricing/promotions, automated financial accounting, and real-time inventory tracking.

### Key Architectural Principles
1. **Microservices Boundaries:** Strict Domain-Driven Design (DDD) domain boundaries with database-per-service isolation.
2. **CQRS Pattern:** Complete separation of Command (write/transactional) workloads from Query (read/dashboard) workloads.
3. **Polyglot Persistence:** SQL Server for ACID-compliant transactional state; MongoDB for denormalized, read-optimized document projections; Redis for high-speed caching and distributed locks.
4. **Hybrid Communication:** 
   - **REST API:** Angular Single Page Application (SPA) $\rightarrow$ YARP API Gateway.
   - **gRPC:** High-performance, strongly-typed synchronous internal service-to-service calls.
   - **RabbitMQ:** Asynchronous event-driven messaging for domain event handling and eventual consistency.

---

## 2. High-Level System Architecture

```
                               ┌─────────────────────────┐
                               │   Angular 18+ SPA       │
                               │  ERPInfinity Web / POS  │
                               └────────────┬────────────┘
                                            │ REST / HTTPS
                                            ▼
                               ┌─────────────────────────┐
                               │   YARP API Gateway      │
                               │  (Routing, Auth, Rate)  │
                               └────────────┬────────────┘
                                            │
         ┌──────────────────────────────────┼──────────────────────────────────┐
         │                                  │                                  │3
         ▼                                  ▼                                  ▼
 ┌───────────────┐                  ┌───────────────┐                  ┌───────────────┐
 │ Identity Svc  │                  │  Product Svc  │                  │   Store Svc   │
 └───────┬───────┘                  └───────┬───────┘                  └─sw ──────┬───────┘
         │                                  │                                  │
         └──────────────────────────────────┼──────────────────────────────────┘
                                            │ gRPC (Synchronous Queries)
         ┌──────────────────────────────────┼──────────────────────────────────┐
         ▼                                  ▼                                  ▼
 ┌───────────────┐                  ┌───────────────┐                  ┌───────────────┐
 │ Inventory Svc │                  │ Warehouse Svc │                  │ Purchase Svc  │
 └───────┬───────┘                  └───────┬───────┘                  └───────┬───────┘
         │                                  │                                  │
         └──────────────────────────────────┼──────────────────────────────────┘
                                            │
                                  RabbitMQ Event Bus (Async Events)
                                            │
         ┌──────────────────────────────────┼──────────────────────────────────┐
         ▼                                  ▼                                  ▼
 ┌───────────────┐                  ┌───────────────┐                  ┌───────────────┐
 │ Sales/POS Svc │                  │ Payment Svc   │                  │  Finance Svc  │
 └───────┬───────┘                  └───────┬───────┘                  └───────┬───────┘
         │                                  │                                  │
         └──────────────────────────────────┼──────────────────────────────────┘
                                            │
                                            ▼
                               ┌─────────────────────────┐
                               │  Reporting & Analytics  │
                               │    (MongoDB Engine)     │
                               └────────────┬────────────┘
```

---

## 3. Comprehensive Breakdown of Microservices

The **ERPInfinity** platform comprises **14 core business microservices** plus **infrastructure services**.

| # | Microservice | Primary Responsibility | Write Storage (SQL Server) | Read Storage (MongoDB / Redis) |
|---|---|---|---|---|
| 1 | **Identity Service** | AuthN/AuthN, Users, Roles, JWT/OAuth2, Security Audit | `ERPInfinity_Identity` | Redis (Token Blacklist & Sessions) |
| 2 | **Product Service** | Product Master, SKU, Barcodes, Categories, Tax Slabs | `ERPInfinity_Product` | `Mongo_ProductRead` |
| 3 | **Store Service** | Physical Stores, POS Counters, Store Registers & Terminals | `ERPInfinity_Store` | `Mongo_StoreRead` |
| 4 | **Inventory Service** | Real-time Stock, Transactions, Adjustments, Transfers | `ERPInfinity_Inventory` | `Mongo_InventoryRead` |
| 5 | **Warehouse Service** | Racks, Bins, Goods Receiving (GRN), Picking, Packing | `ERPInfinity_Warehouse` | `Mongo_WarehouseRead` |
| 6 | **Purchase Service** | Supplier Management, Purchase Orders (PO), Approvals | `ERPInfinity_Purchase` | `Mongo_PurchaseRead` |
| 7 | **Sales / POS Service**| Retail Counter Cash Billing, Invoicing, Counter Returns | `ERPInfinity_Sales` | `Mongo_SalesRead` |
| 8 | **Order Service** | E-commerce / Home Delivery Orders & Fulfillment Cycle | `ERPInfinity_Order` | `Mongo_OrderRead` |
| 9 | **Customer Service** | Customer Profiles, Addresses, Loyalty Points & Tiers | `ERPInfinity_Customer` | `Mongo_CustomerRead` |
| 10 | **Pricing Service** | Base Price, Store-Level Prices, Schemes, Bulk Offers | `ERPInfinity_Pricing` | Redis (Cached Price Lists) |
| 11 | **Payment Service** | Payment Gateway Integration, Cash/Card/UPI, Refunds | `ERPInfinity_Payment` | `Mongo_PaymentRead` |
| 12 | **Finance Service** | General Ledger, Expense Tracking, Supplier Settlements | `ERPInfinity_Finance` | `Mongo_FinanceRead` |
| 13 | **Notification Service**| SMS, Email, Push Notifications, WhatsApp Alerts | `ERPInfinity_Notification` | MongoDB (Notification History) |
| 14 | **Reporting Service** | Executive Dashboards, Store Analytics, Real-time Sales | N/A (Consumes Events) | `Mongo_Reporting` |
| 15 | **Audit Service** | Enterprise System Action Audit & Compliance Trails | N/A | `Mongo_Audit` |

---

## 4. Deep Dive into Core Microservices

### 4.1 Product Service
* **Responsibility:** Centralized product catalog (Product Master).
* **Entities & Relational Tables:**
  - `Products` (Id, Name, Description, BrandId, CategoryId, HSNCode, TaxPercentage, IsActive)
  - `Categories` (Id, ParentCategoryId, Name, Code)
  - `Brands` (Id, Name, Manufacturer)
  - `ProductSKUs` (Id, ProductId, SKUCode, UnitOfMeasure, PackSize, Weight)
  - `Barcodes` (Id, SKUId, BarcodeNumber, IsPrimary)
* **MongoDB Read Projection (`ProductReadModel`):**
  ```json
  {
    "productId": "PRD-10982",
    "name": "Tata Salt 1kg",
    "brand": "Tata",
    "category": "Grocery > Spices & Salt",
    "sku": "TATA-SALT-1KG",
    "barcode": "8901234567890",
    "mrp": 30.00,
    "sellingPrice": 27.00,
    "taxPercentage": 5.0,
    "hsnCode": "2501"
  }
  ```

---

### 4.2 Inventory Service
* **Responsibility:** Stock quantities, batch numbers, expiry dates, and movement logs across warehouses and physical stores.
* **Entities & Relational Tables:**
  - `Stocks` (Id, LocationId, LocationType, SKUId, QuantityOnHand, ReservedQuantity, AvailableQuantity)
  - `StockTransactions` (Id, SKUId, LocationId, TransactionType [Purchase, Sale, Adjustment, Transfer], Quantity, ReferenceId, CreatedAt)
  - `StockAdjustments` (Id, LocationId, SKUId, AdjustmentQty, Reason, ApprovedBy)
  - `StockTransfers` (Id, SourceLocationId, DestinationLocationId, Status, DispatchedAt, ReceivedAt)
* **Rule:** Never execute raw `Stock = Stock - X` without appending a corresponding immutable `StockTransaction` record.

---

### 4.3 Sales / POS Service
* **Responsibility:** Billing counters at physical supermarket stores with fast barcode scanning, bill generation, and cash/UPI receipt printing under tight sub-second latency constraints.
* **Entities & Relational Tables:**
  - `SalesInvoices` (Id, InvoiceNumber, StoreId, POSTerminalId, CashierId, TotalAmount, TaxAmount, DiscountAmount, PaymentStatus, CreatedAt)
  - `SalesInvoiceItems` (Id, InvoiceId, SKUId, Quantity, UnitPrice, DiscountAmount, TaxAmount, Total)
  - `POSRegisters` (Id, StoreId, TerminalCode, CounterStatus, OpeningBalance, ClosingBalance)

---

### 4.4 Purchase & Procurement Service
* **Responsibility:** Vendor relationships, purchase requisitions, PO approval workflows, and Goods Received Notes (GRN).
* **Entities & Relational Tables:**
  - `Suppliers` (Id, VendorCode, Name, GSTIN, ContactEmail, CreditDays)
  - `PurchaseOrders` (Id, PONumber, SupplierId, WarehouseId, Status, TotalAmount, CreatedBy, ApprovedBy)
  - `PurchaseOrderItems` (Id, POId, SKUId, OrderedQty, ReceivedQty, UnitPrice)
  - `GoodsReceivedNotes` (Id, GRNNumber, POId, WarehouseId, ReceivedDate, VehicleNumber, InspectorId)

---

## 5. CQRS Pattern Implementation

The **ERPInfinity** platform implements CQRS using **MediatR** in .NET Core to isolate Write (Command) operations from Read (Query) operations.

```
                              CQRS Pattern Architecture

           ┌─────────────────────────────────────────────────────────────┐
           │                     API Controller / Endpoint                │
           └──────────────┬──────────────────────────────┬───────────────┘
                          │                              │
                Command   │                              │ Query
                (Write)   ▼                              ▼ (Read)
           ┌─────────────────────────────┐┌─────────────────────────────┐
           │      Command Handler        ││        Query Handler        │
           └──────────────┬──────────────┘└──────────────┬─────────────┘
                          │                              │
                          ▼                              ▼
                 SQL Server (ACID Write)        MongoDB (Read Projection)
                          │                              ▲
                          ▼                              │ Sync
                   Domain Event                          │
                          │                              │
                          ▼                              │
                  RabbitMQ Bus ──────────────────────────┘
```

### 5.1 Command Flow Pipeline (Write Path)
1. HTTP POST/PUT request enters the API Controller.
2. Request payload mapped to a `Command` object (e.g., `CreatePurchaseOrderCommand`).
3. MediatR dispatches `Command` to `CommandHandler`.
4. `CommandHandler` executes business rules on Domain Aggregates.
5. Entity modifications saved to **SQL Server** inside an explicit Database Transaction.
6. A **Domain/Integration Event** (e.g., `PurchaseOrderApprovedEvent`) is emitted to the **Outbox Table**.
7. An Outbox Background Worker publishes the event to **RabbitMQ**.

### 5.2 Query Flow Pipeline (Read Path)
1. HTTP GET request enters the API Controller.
2. Request payload mapped to a `Query` object (e.g., `GetInventoryDashboardQuery`).
3. MediatR dispatches `Query` to `QueryHandler`.
4. `QueryHandler` queries **MongoDB** (or Redis cache) using clean read-only DTO projections.
5. Data returned to the client without relational joins or complex ORM mapping overhead.

---

## 6. Inter-Service Communication & Event Bus

### 6.1 Communication Matrix

| Call Type | Protocol | Use Case | Example |
|---|---|---|---|
| **External $\rightarrow$ Internal** | REST / JSON | Angular SPA $\rightarrow$ YARP Gateway | Client browser fetching store inventory list |
| **Internal Sync** | gRPC / HTTP2 | Service $\rightarrow$ Service (Immediate Response) | Order Service checking item price from Pricing Service |
| **Internal Async** | RabbitMQ / AMQP | Service $\rightarrow$ Service (Event Notification) | Sales Service notifying Inventory Service of a completed sale |

### 6.2 Key Integration Events Catalog

| Event Name | Publisher Service | Subscriber Service(s) | Business Outcome |
|---|---|---|---|
| `ProductCreatedEvent` | Product Service | Reporting, Inventory | Initializes stock projections and reporting search indices. |
| `PurchaseOrderApproved` | Purchase Service | Warehouse, Inventory | Alerts warehouse of incoming supplier shipment. |
| `GoodsReceivedEvent` | Warehouse Service | Inventory, Purchase, Finance | Increments available stock and generates vendor bill ledger. |
| `StockUpdatedEvent` | Inventory Service | Product, Reporting | Updates online product availability status. |
| `SalesInvoiceCreatedEvent` | Sales/POS Service | Inventory, Finance, Reporting | Decrements store inventory; posts daily revenue to Finance ledger. |
| `PaymentCompletedEvent` | Payment Service | Order, Notification, Finance | Marks e-commerce order as confirmed; sends customer SMS. |
| `OrderShippedEvent` | Order Service | Notification, Customer | Sends shipment tracking link to customer. |

---

## 7. Clean Architecture Solution Structure

The solution tree for **ERPInfinity** is structured as follows:

```text
ERPInfinity/
├── src/
│   ├── BuildingBlocks/
│   │   ├── ERPInfinity.BuildingBlocks.CQRS/          # MediatR interfaces & behaviors
│   │   ├── ERPInfinity.BuildingBlocks.Messaging/     # RabbitMQ MassTransit wrappers
│   │   ├── ERPInfinity.BuildingBlocks.Persistence/   # EF Core & Mongo base context
│   │   └── ERPInfinity.BuildingBlocks.Logging/       # Serilog & OpenTelemetry setup
│   │
│   ├── Services/
│   │   ├── Product/
│   │   │   ├── ERPInfinity.Product.API/           # Controllers, gRPC Endpoints, Middleware
│   │   │   ├── ERPInfinity.Product.Application/   # Commands, Queries, Handlers, DTOs
│   │   │   ├── ERPInfinity.Product.Domain/        # Aggregates, Entities, Value Objects, Events
│   │   │   └── ERPInfinity.Product.Infrastructure/  # EF Core, Mongo Repositories, gRPC Clients
│   │   │
│   │   ├── Inventory/
│   │   │   ├── ERPInfinity.Inventory.API/
│   │   │   ├── ERPInfinity.Inventory.Application/
│   │   │   ├── ERPInfinity.Inventory.Domain/
│   │   │   └── ERPInfinity.Inventory.Infrastructure/
│   │   │
│   │   └── Sales/
│   │       ├── ERPInfinity.Sales.API/
│   │       ├── ERPInfinity.Sales.Application/
│   │       ├── ERPInfinity.Sales.Domain/
│   │       └── ERPInfinity.Sales.Infrastructure/
│   │
│   └── Gateway/
│       └── ERPInfinity.YarpGateway/                  # YARP Reverse Proxy Configuration
│
└── tests/
    ├── ERPInfinity.Product.UnitTests/
    └── ERPInfinity.Inventory.IntegrationTests/
```

---

## 8. Core Business Workflows

### 8.1 Procurement & Stock Receipt Workflow (Purchase-to-Stock)

```
[Store/Warehouse] ──> Create Requisition ──> [Purchase Service] ──> Generate PO
                                                                        │
                                                                 Supplier Delivery
                                                                        │
                                                                        ▼
[Finance Ledger]  <── Supplier Bill <── [GRN Created] <── [Warehouse Receiving]
       │                                       │
       ▼                                       ▼
  [Finance]                            [Inventory Service]
                                       Stock Increased (+N)
```

1. **Requisition:** Low stock triggers a Purchase Requisition.
2. **PO Approval:** Purchase Service generates a PO; manager approves it.
3. **Goods Receipt:** Supplier delivers items to warehouse; Warehouse Service issues a Goods Received Note (GRN).
4. **Stock Update:** `GoodsReceivedEvent` fires $\rightarrow$ Inventory Service increases available stock (`QuantityOnHand`).
5. **Settlement:** Finance Service creates a payable ledger record for the supplier.

---

### 8.2 Physical Store POS Cashier Billing Workflow

```
[POS Barcode Scanner] ──> Item Scanned ──> [Sales Service] ──> Price Check (Redis/Pricing)
                                                                       │
                                                                Calculates Tax/Offers
                                                                       │
                                                                       ▼
[Inventory Service]  <── Stock Decremented <── Invoice Issued <── Payment Processed
  (Stock - Quantity)                                              (Cash/UPI/Card)
```

1. Cashier scans item barcodes at POS terminal.
2. Sales Service retrieves price, active discounts, and tax rates.
3. Payment collected via Cash, UPI, or Credit Card Terminal.
4. Invoice generated and printed instantly.
5. `SalesInvoiceCreatedEvent` published to RabbitMQ.
6. Inventory Service asynchronously deducts stock quantity for the store.

---

## 9. Technology Stack Matrix

| Infrastructure Layer | Recommended Tool / Framework |
|---|---|
| **Frontend Framework** | Angular 18+ with TypeScript & Angular Material |
| **Backend Framework** | ASP.NET Core Web API 8.0 / 9.0 (C#) |
| **API Gateway** | YARP (Yet Another Reverse Proxy) |
| **CQRS & Mediator** | MediatR |
| **Relational Database** | Microsoft SQL Server 2022 |
| **Document Database** | MongoDB 7.0+ |
| **Distributed Cache** | Redis |
| **Message Broker** | RabbitMQ with MassTransit |
| **Inter-Service RPC** | gRPC (.NET gRPC tooling) |
| **Data Access / ORM** | EF Core 8 (Writes) + Dapper (High-speed Reads) |
| **Validation** | FluentValidation |
| **Logging & Tracing** | Serilog + OpenTelemetry + Jaeger |
| **Containerization** | Docker & Kubernetes (k8s) |

---

## 10. Phase-by-Phase Implementation Roadmap

```
┌────────────────────────────────────────────────────────────────────────┐
│ PHASE 1: Foundation & Gateway                                          │
│ ERPInfinity Solution layout, YARP Gateway, Identity Svc, Docker setup  │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│ PHASE 2: Core Master Data Services                                     │
│ Product Service (Master Data) & Store Service                          │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│ PHASE 3: Inventory & Procurement                                       │
│ Inventory Service (Stock Transactions) & Purchase Service (PO / GRN)   │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│ PHASE 4: Sales Billing & POS                                           │
│ Sales Service (POS Billing Counter), Pricing & Promotion Service       │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│ PHASE 5: E-Commerce & Customer Fulfillment                             │
│ Customer Service, Order Service, Payment Gateway Service               │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│ PHASE 6: Finance & Reporting Engine                                    │
│ Finance Service (General Ledger), Reporting Service (MongoDB Engine)   │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 11. Crucial Architectural Rules

1. **Strict Data Isolation:** Microservices must NEVER access or query another service's database tables directly.
2. **Transactional Truth:** SQL Server is the sole source of truth for transactional commands.
3. **Read Projections:** MongoDB is reserved for denormalized read queries and real-time analytical dashboards.
4. **Reliable Messaging:** Use the Outbox Pattern for publishing domain events to prevent lost message updates during database transactions.
5. **Idempotent Consumers:** All event consumers in RabbitMQ must be idempotent to handle potential message re-deliveries smoothly.
6. **Graceful Degredation:** POS billing terminals must retain offline scanning capabilities if network connectivity drops temporarily.
