// Angular ERPInfinity Core TypeScript Data Models

export type UserRole = 'CEO' | 'Admin' | 'Cashier';

export interface UserSession {
    id: string;
    username: string;
    displayName: string;
    email: string;
    role: UserRole;
    token: string;
}

export interface ProductItem {
    id: string;
    skuCode: string;
    productName: string;
    category: string;
    barcode: string;
    mrp: number;
    sellingPrice: number;
    taxPercentage: number;
    hsnCode: string;
}

export interface CartItem extends ProductItem {
    quantity: number;
}

export interface InventoryItem {
    location: string;
    skuCode: string;
    productName: string;
    onHand: number;
    reserved: number;
    available: number;
    reorderPoint: number;
    costPrice: number;
}

export interface PurchaseOrder {
    poNumber: string;
    vendor: string;
    warehouse: string;
    status: string;
    amount: number;
    date: string;
}

export interface SalesInvoice {
    invoiceNum: string;
    customer: string;
    date: string;
    itemsCount: number;
    mode: string;
    amount: number;
}

export interface MicroserviceHealth {
    name: string;
    port: number;
    route: string;
    pingMs: number;
    status: 'Healthy' | 'Degraded';
}
