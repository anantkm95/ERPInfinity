# ERPInfinity – Multi-Tenant SaaS & Enterprise Retail ERP Blueprint

> **Product Vision:** Dual-Market Retail ERP Platform  
> **Target Audience A (SaaS / Small Shopkeepers):** Single-Store Retailers, Grocery Stores, Local Marts (Quick POS Billing, Inventory, Online Storefront, WhatsApp Invoices).  
> **Target Audience B (Enterprise Chains / DMart):** Large Supermarkets & Multi-Branch Enterprise Chains (Central HQ Control, Multi-Warehouse Logistics, Multi-Counter POS, General Ledger Accounting).  

---

## 1. Business & Product Strategy

**ERPInfinity** is architected to serve both ends of the retail market through a scalable **Multi-Tenant Architecture**:

```text
                               ┌────────────────────────────────────────┐
                               │       ERPInfinity SaaS Platform        │
                               └───────────────────┬────────────────────┘
                                                   │
                ┌──────────────────────────────────┴──────────────────────────────────┐
                ▼                                                                     ▼
    ┌───────────────────────┐                                             ┌───────────────────────┐
    │  SaaS Small Shopkeeper│                                             │   Enterprise Chain    │
    │  (Single Store / Mart)│                                             │  (DMart / Multi-Branch│
    └───────────┬───────────┘                                             └───────────┬───────────┘
                │                                                                     │
  • Shared Multi-Tenant Cluster                                         • Dedicated / Hybrid Database
  • Quick Barcode / UPI Billing                                         • Multi-Branch & Multi-Warehouse
  • Auto-Generated Online Shop                                          • Central Procurement & GRN
  • Subscription ($/month)                                              • Custom Role RBAC & GL Finance
```

---

## 2. Multi-Tenant & Multi-Branch Organizational Hierarchy

Every entity in the database is scoped by **`TenantId`** and optionally **`BranchId`**:

```text
                               ┌─────────────────────────┐
                               │   Tenant (Company/SaaS) │
                               │   e.g. DMart Retail     │
                               └────────────┬────────────┘
                                            │
                    ┌───────────────────────┴───────────────────────┐
                    ▼                                               ▼
        ┌───────────────────────┐                       ┌───────────────────────┐
        │ Branch / Store 001    │                       │ Branch / Store 002    │
        │ (Gurgaon Hypermarket) │                       │ (Noida Supermarket)   │
        └───────────┬───────────┘                       └───────────┬───────────┘
                    │                                               │
           ┌────────┴────────┐                             ┌────────┴────────┐
           ▼                 ▼                             ▼                 ▼
   ┌───────────────┐ ┌───────────────┐             ┌───────────────┐ ┌───────────────┐
   │ POS Counter 1 │ │ POS Counter 2 │             │ POS Counter 1 │ │ POS Counter 2 │
   └───────────────┘ └───────────────┘             └───────────────┘ └───────────────┘
```

---

## 3. Data Isolation Architecture

To balance cost-efficiency for small shopkeepers with maximum security for Enterprise chains, **ERPInfinity** uses a **Hybrid Isolation Strategy**:

```text
           ERPInfinity Data Isolation Model

┌─────────────────────────────────────────────────────────┐
│              SaaS Pool (Small Shopkeepers)              │
│  Shared SQL Database + TenantId Column Discriminator    │
│  `WHERE TenantId = @CurrentTenantId`                    │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│            Enterprise Pool (DMart / Chains)             │
│  Dedicated Database-per-Tenant or Isolated Schema       │
│  `Db_DMart_Product`, `Db_DMart_Inventory`, etc.        │
└─────────────────────────────────────────────────────────┘
```

### 3.1 Tenant Isolation Levels
1. **Shared Database (Discriminator Model - Small Shops):**
   - Tables contain a `TenantId` column indexed as a composite key (e.g. `IX_Stocks_TenantId_SKUId`).
   - Low infrastructure cost; supports thousands of shopkeepers per database instance.
2. **Isolated Database (Dedicated Model - DMart / Big Sharks):**
   - Enterprise client receives an isolated database set (`ERPInfinity_DMart_Identity`, `ERPInfinity_DMart_Product`, etc.).
   - Guarantees strict data privacy, custom compliance, and independent database backups.

---

## 4. SaaS vs Enterprise Feature Matrix

| Feature | Lite Plan (Small Shopkeepers) | Pro Plan (Supermarkets) | Enterprise Plan (DMart / Chains) |
|---|:---:|:---:|:---:|
| **Target User** | Kirana / Single Store | Multi-Counter Mart | Enterprise Hypermarket Chain |
| **Max Stores / Branches** | 1 Store | Up to 3 Stores | Unlimited Branches |
| **POS Billing Counter** | 1 Web / Mobile POS | Up to 5 Counters | Unlimited POS Counters |
| **Barcode Scanning** | Yes (Sub-2ms) | Yes | Yes (Hardware Integration) |
| **Online E-Commerce Shop** | Auto-Generated Storefront | Auto-Generated Storefront | Branded Web & Mobile App |
| **WhatsApp Invoicing** | Included | Included | Custom SMS / WhatsApp Gateway |
| **Inventory Management** | Basic Stock Tracking | Stock + Low Stock Alerts | Multi-Warehouse & Bin Tracking |
| **Procurement & PO** | Basic Vendor Log | Purchase Orders & GRN | Multi-Level PO Approvals & Vendors |
| **General Ledger Finance** | Simple Expense Log | Profit & Loss Summary | Full Double-Entry GL Accounting |
| **Reporting & CQRS** | Basic Sales Summary | Store Analytics | MongoDB Real-Time Dashboard Engine |
| **Deployment Model** | Shared SaaS Cloud | Shared SaaS Cloud | Dedicated Private Cloud / On-Premise |

---

## 5. Merchant Self-Service Online Store Generator

For small shopkeepers, **ERPInfinity** provides an instant, self-service online store builder:

1. **Merchant Onboarding:** Shopkeeper registers on ERPInfinity $\rightarrow$ Enters shop name (`tatasupermarket`).
2. **Auto-Generated URL:** System provisions an instant e-commerce web storefront at `tatasupermarket.erpinfinity.com` or custom domain.
3. **Automated Catalog Sync:** Items created in the shopkeeper's POS catalog automatically publish to their online storefront.
4. **WhatsApp Order Notifications:** When a local customer orders online, the shopkeeper receives an instant WhatsApp alert and POS counter notification.

---

## 6. Enterprise HQ Command Dashboard (DMart View)

For large retail chains like **DMart**, the Central Headquarters (HQ) dashboard provides real-time control across all branches:

1. **Consolidated Sales Feed:** Live real-time sales stream across all nationwide branches via MongoDB aggregation pipelines.
2. **Inter-Branch Stock Transfer:** HQ managers can initiate stock transfers between low-selling stores and high-demand branches.
3. **Centralized Procurement:** Automated Purchase Orders generated based on aggregated nationwide stock levels.
4. **Branch Performance Ranking:** Real-time leaderboards ranking stores by revenue, footfall, average bill value, and cashier efficiency.

---

## 7. Multi-Tenant Database Schema Adjustments

To enable multi-tenancy across all microservice databases, every core table includes **`TenantId`** and **`BranchId`**:

```sql
-- Example Multi-Tenant Sales Invoice Table
CREATE TABLE SalesInvoices (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,        -- Scopes data to specific Shopkeeper or DMart
    BranchId UNIQUEIDENTIFIER NOT NULL,        -- Scopes data to specific physical store branch
    InvoiceNumber VARCHAR(50) NOT NULL,
    SubTotal DECIMAL(18,2) NOT NULL,
    TaxAmount DECIMAL(18,2) NOT NULL,
    GrandTotal DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

-- Multitenant Index
CREATE UNIQUE INDEX IX_SalesInvoices_Tenant_Branch_Invoice 
ON SalesInvoices (TenantId, BranchId, InvoiceNumber);
```
