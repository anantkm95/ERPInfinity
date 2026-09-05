// ========================================================================================
// ERPInfinity - MongoDB CQRS Read Projections Setup Script (MongoDB 7.0 / mongosh)
// Architecture: Database-Per-Service + CQRS Read Models
// Description: Initializes MongoDB database and read collections for CQRS read services.
// ========================================================================================

// Switch to ERPInfinity CQRS Read database
db = db.getSiblingDB('ERPInfinity_CQRSRead');

print('----------------------------------------------------------------------------------------');
print('Initializing ERPInfinity MongoDB CQRS Collections...');
print('----------------------------------------------------------------------------------------');

// 1. Product Service Read Projection
if (!db.getCollectionNames().includes('Mongo_ProductCatalogRead')) {
    db.createCollection('Mongo_ProductCatalogRead');
    db.Mongo_ProductCatalogRead.createIndex({ "skuCode": 1 }, { unique: true });
    db.Mongo_ProductCatalogRead.createIndex({ "barcode": 1 });
    db.Mongo_ProductCatalogRead.createIndex({ "categoryId": 1 });
    print('✓ Collection [Mongo_ProductCatalogRead] created (Product Service Read Projection)');
} else {
    print('-> Collection [Mongo_ProductCatalogRead] already exists');
}

// 2. Inventory Service Read Projection
if (!db.getCollectionNames().includes('Mongo_InventoryDashboardRead')) {
    db.createCollection('Mongo_InventoryDashboardRead');
    db.Mongo_InventoryDashboardRead.createIndex({ "locationId": 1, "skuId": 1 }, { unique: true });
    db.Mongo_InventoryDashboardRead.createIndex({ "availableQuantity": 1 });
    print('✓ Collection [Mongo_InventoryDashboardRead] created (Inventory Service Read Projection)');
} else {
    print('-> Collection [Mongo_InventoryDashboardRead] already exists');
}

// 3. Sales & POS Service Read Projection
if (!db.getCollectionNames().includes('Mongo_SalesAnalyticsRead')) {
    db.createCollection('Mongo_SalesAnalyticsRead');
    db.Mongo_SalesAnalyticsRead.createIndex({ "storeId": 1, "createdAt": -1 });
    db.Mongo_SalesAnalyticsRead.createIndex({ "invoiceNumber": 1 }, { unique: true });
    print('✓ Collection [Mongo_SalesAnalyticsRead] created (Sales Service Read Projection)');
} else {
    print('-> Collection [Mongo_SalesAnalyticsRead] already exists');
}

// 4. Customer & CRM Service Read Projection
if (!db.getCollectionNames().includes('Mongo_Customer360Read')) {
    db.createCollection('Mongo_Customer360Read');
    db.Mongo_Customer360Read.createIndex({ "customerId": 1 }, { unique: true });
    db.Mongo_Customer360Read.createIndex({ "mobileNumber": 1 });
    print('✓ Collection [Mongo_Customer360Read] created (Customer Service Read Projection)');
} else {
    print('-> Collection [Mongo_Customer360Read] already exists');
}

// 5. Purchase Service Read Projection
if (!db.getCollectionNames().includes('Mongo_PurchaseRead')) {
    db.createCollection('Mongo_PurchaseRead');
    db.Mongo_PurchaseRead.createIndex({ "poNumber": 1 }, { unique: true });
    db.Mongo_PurchaseRead.createIndex({ "supplierId": 1 });
    print('✓ Collection [Mongo_PurchaseRead] created (Purchase Service Read Projection)');
} else {
    print('-> Collection [Mongo_PurchaseRead] already exists');
}

// 6. System Audit & Compliance Read Projection
if (!db.getCollectionNames().includes('Mongo_AuditLogsRead')) {
    db.createCollection('Mongo_AuditLogsRead');
    db.Mongo_AuditLogsRead.createIndex({ "timestamp": -1 });
    db.Mongo_AuditLogsRead.createIndex({ "serviceName": 1, "userId": 1 });
    print('✓ Collection [Mongo_AuditLogsRead] created (Identity/System Audit Logs)');
} else {
    print('-> Collection [Mongo_AuditLogsRead] already exists');
}

print('----------------------------------------------------------------------------------------');
print('ALL MONGODB CQRS READ COLLECTIONS CREATED SUCCESSFULLY!');
print('----------------------------------------------------------------------------------------');
