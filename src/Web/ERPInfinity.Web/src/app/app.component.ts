import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from './services/auth.service';
import { AudioService } from './services/audio.service';
import { ProductItem, CartItem, InventoryItem, PurchaseOrder, SalesInvoice, MicroserviceHealth, UserRole } from './models/erp-models';

export interface ToastAlert {
  id: string;
  message: string;
  type: 'success' | 'info' | 'amber';
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  public authService = inject(AuthService);
  public audioService = inject(AudioService);

  // Active Tab View Signal
  public activeTab = signal<string>('pos');
  public activeStore = signal<string>('WH-CENTRAL-01');
  public activeCustomer = signal<{ id: string; name: string }>({ id: 'CUST-WALKIN', name: 'Walk-in Customer' });
  public paymentMode = signal<string>('CASH');

  // Toast System Signal
  public toasts = signal<ToastAlert[]>([]);

  // Auth Inputs
  public loginUsername = signal<string>('admin.anant@erpinfinity.com');
  public loginPassword = signal<string>('Admin@2026');
  public authTabMode = signal<'login' | 'register'>('login');

  public regName = signal<string>('');
  public regUsername = signal<string>('');
  public regEmail = signal<string>('');
  public regRole = signal<UserRole>('Cashier');
  public regPassword = signal<string>('');

  // Cart & POS Billing State Signals
  public cart = signal<CartItem[]>([]);
  public barcodeInput = signal<string>('');
  public discountAmount = signal<number>(0);
  public cashTendered = signal<number>(0);

  // Add Product Form Signals
  public newProdName = signal<string>('');
  public newProdCat = signal<string>('Grocery');
  public newProdBarcode = signal<string>('');
  public newProdSku = signal<string>('');
  public newProdMrp = signal<number | null>(null);
  public newProdPrice = signal<number | null>(null);
  public newProdTax = signal<number>(5);
  public newProdHsn = signal<string>('25010010');

  // GRN Receive Form Signals
  public grnSku = signal<string>('SKU-TATA-SALT-1KG');
  public grnLocation = signal<string>('WH-CENTRAL-01');
  public grnQty = signal<number>(100);

  // PO Form Signals
  public poVendor = signal<string>('');
  public poWarehouse = signal<string>('WH-CENTRAL-01');
  public poAmount = signal<number | null>(null);

  // Analytics Metrics Signals
  public salesTotal = signal<number>(4320.00);
  public invoiceCount = signal<number>(12);
  public totalGstCollected = signal<number>(216.00);
  public salesInvoices = signal<SalesInvoice[]>([
    { invoiceNum: 'INV-20260912-849201', customer: 'Walk-in Customer', date: '11:45 AM', itemsCount: 4, mode: 'CASH', amount: 345.00 },
    { invoiceNum: 'INV-20260912-730192', customer: 'Rahul Sharma (VIP)', date: '10:30 AM', itemsCount: 8, mode: 'UPI', amount: 1250.00 },
    { invoiceNum: 'INV-20260912-619203', customer: 'Priya Patel (Corporate)', date: '09:15 AM', itemsCount: 15, mode: 'CARD', amount: 2725.00 }
  ]);

  // Thermal Receipt State
  public activeReceiptModal = signal<boolean>(false);
  public currentReceipt = signal<{
    invNum: string;
    date: string;
    customer: string;
    subtotal: number;
    tax: number;
    discount: number;
    grandTotal: number;
    mode: string;
    items: CartItem[];
  } | null>(null);

  // Active Modals
  public activeAddProductModal = signal<boolean>(false);
  public activeGrnModal = signal<boolean>(false);
  public activePoModal = signal<boolean>(false);

  // Master Catalog Data Signals
  public catalog = signal<ProductItem[]>([
    { id: '1', skuCode: 'SKU-TATA-SALT-1KG', productName: 'Tata Salt Vacuum Evaporated 1kg', category: 'Grocery', barcode: '8901058000101', mrp: 28.00, sellingPrice: 25.00, taxPercentage: 5.00, hsnCode: '25010010' },
    { id: '2', skuCode: 'SKU-AMUL-BUTTER-500G', productName: 'Amul Pasteurised Butter 500g', category: 'Dairy', barcode: '8901262010054', mrp: 275.00, sellingPrice: 260.00, taxPercentage: 12.00, hsnCode: '04051000' },
    { id: '3', skuCode: 'SKU-MAGGI-NOODLES-280G', productName: 'Maggi 2-Minute Masala Instant Noodles 280g', category: 'Packaged Food', barcode: '8901058852311', mrp: 54.00, sellingPrice: 50.00, taxPercentage: 18.00, hsnCode: '19023010' },
    { id: '4', skuCode: 'SKU-FORTUNE-OIL-1L', productName: 'Fortune Refined Sunflower Oil 1L Pouch', category: 'Grocery', barcode: '8901030678912', mrp: 160.00, sellingPrice: 145.00, taxPercentage: 5.00, hsnCode: '15121910' },
    { id: '5', skuCode: 'SKU-GOOD-DAY-BISCUIT', productName: 'Britannia Good Day Butter Cookies 200g', category: 'Packaged Food', barcode: '8901014002105', mrp: 35.00, sellingPrice: 30.00, taxPercentage: 18.00, hsnCode: '19053100' },
    { id: '6', skuCode: 'SKU-DETTOL-SOAP-125G', productName: 'Dettol Original Bathing Soap 125g', category: 'Personal Care', barcode: '8901396001012', mrp: 50.00, sellingPrice: 45.00, taxPercentage: 18.00, hsnCode: '34011110' },
    { id: '7', skuCode: 'SKU-RED-LABEL-TEA-500G', productName: 'Brooke Bond Red Label Tea 500g', category: 'Grocery', barcode: '8901030005428', mrp: 310.00, sellingPrice: 280.00, taxPercentage: 5.00, hsnCode: '09023020' },
    { id: '8', skuCode: 'SKU-DAIRY-MILK-SILK-150G', productName: 'Cadbury Dairy Milk Silk Chocolate 150g', category: 'Packaged Food', barcode: '8901233021941', mrp: 190.00, sellingPrice: 175.00, taxPercentage: 18.00, hsnCode: '18063100' }
  ]);

  public inventory = signal<InventoryItem[]>([
    { location: 'WH-CENTRAL-01', skuCode: 'SKU-TATA-SALT-1KG', productName: 'Tata Salt Vacuum Evaporated 1kg', onHand: 500, reserved: 0, available: 500, reorderPoint: 50, costPrice: 20.00 },
    { location: 'STORE-MUMBAI-01', skuCode: 'SKU-AMUL-BUTTER-500G', productName: 'Amul Pasteurised Butter 500g', onHand: 150, reserved: 0, available: 150, reorderPoint: 20, costPrice: 230.00 },
    { location: 'WH-CENTRAL-01', skuCode: 'SKU-MAGGI-NOODLES-280G', productName: 'Maggi 2-Minute Masala Instant Noodles 280g', onHand: 300, reserved: 0, available: 300, reorderPoint: 40, costPrice: 42.00 },
    { location: 'WH-CENTRAL-01', skuCode: 'SKU-FORTUNE-OIL-1L', productName: 'Fortune Refined Sunflower Oil 1L Pouch', onHand: 220, reserved: 0, available: 220, reorderPoint: 30, costPrice: 120.00 },
    { location: 'STORE-MUMBAI-01', skuCode: 'SKU-GOOD-DAY-BISCUIT', productName: 'Britannia Good Day Butter Cookies 200g', onHand: 400, reserved: 0, available: 400, reorderPoint: 50, costPrice: 22.00 }
  ]);

  public purchaseOrders = signal<PurchaseOrder[]>([
    { poNumber: 'PO-20260912-48201', vendor: 'Tata Consumer Products Ltd.', warehouse: 'WH-CENTRAL-01', status: 'Approved', amount: 12500.00, date: 'Today 11:30 AM' },
    { poNumber: 'PO-20260912-39102', vendor: 'Gujarat Cooperative Milk Marketing (Amul)', warehouse: 'STORE-MUMBAI-01', status: 'In Transit', amount: 34500.00, date: 'Yesterday 04:15 PM' },
    { poNumber: 'PO-20260912-18293', vendor: 'Nestle India Pvt Ltd.', warehouse: 'WH-CENTRAL-01', status: 'Pending Approval', amount: 18200.00, date: '10 Sep 02:00 PM' }
  ]);

  public services = signal<MicroserviceHealth[]>([
    { name: 'Identity & Auth Service', port: 5001, route: '/identity/health', pingMs: 1.2, status: 'Healthy' },
    { name: 'Product Master Service', port: 5002, route: '/products/health', pingMs: 1.8, status: 'Healthy' },
    { name: 'Inventory & Stock Ledger', port: 5003, route: '/inventory/health', pingMs: 0.9, status: 'Healthy' },
    { name: 'Sales & POS Billing', port: 5004, route: '/sales/health', pingMs: 1.4, status: 'Healthy' },
    { name: 'Purchase & Procurement', port: 5005, route: '/purchase/health', pingMs: 2.1, status: 'Healthy' },
    { name: 'Warehouse & Fulfillment', port: 5006, route: '/warehouse/health', pingMs: 1.1, status: 'Healthy' },
    { name: 'Order Management Service', port: 5007, route: '/orders/health', pingMs: 1.5, status: 'Healthy' },
    { name: 'Pricing & Promotion Engine', port: 5008, route: '/pricing/health', pingMs: 0.8, status: 'Healthy' },
    { name: 'Payment Gateway Integration', port: 5009, route: '/payments/health', pingMs: 2.4, status: 'Healthy' },
    { name: 'Finance & General Ledger', port: 5010, route: '/finance/health', pingMs: 1.7, status: 'Healthy' },
    { name: 'Store Infrastructure Node', port: 5011, route: '/stores/health', pingMs: 1.3, status: 'Healthy' },
    { name: 'Notification Service', port: 5012, route: '/notifications/health', pingMs: 1.0, status: 'Healthy' },
    { name: 'Customer & CRM Service', port: 5013, route: '/customers/health', pingMs: 1.6, status: 'Healthy' },
    { name: 'Reporting & BI Analytics', port: 5014, route: '/reporting/health', pingMs: 2.2, status: 'Healthy' }
  ]);

  // Computed Cart Calculations
  public cartSubtotal = computed(() => this.cart().reduce((sum, item) => sum + (item.sellingPrice * item.quantity), 0));
  public cartTax = computed(() => this.cart().reduce((sum, item) => sum + (item.sellingPrice * item.quantity * (item.taxPercentage / 100)), 0));
  public cartGrandTotal = computed(() => Math.max(0, this.cartSubtotal() + this.cartTax() - this.discountAmount()));
  public cartItemsCount = computed(() => this.cart().reduce((sum, item) => sum + item.quantity, 0));
  public changeDue = computed(() => Math.max(0, this.cashTendered() - this.cartGrandTotal()));

  // Computed Inventory Valuation
  public totalStockOnHand = computed(() => this.inventory().reduce((sum, item) => sum + item.onHand, 0));
  public totalInventoryValuation = computed(() => this.inventory().reduce((sum, item) => sum + (item.onHand * item.costPrice), 0));

  // Computed Initials
  public userInitials = computed(() => {
    const user = this.authService.currentUser();
    if (!user || !user.displayName) return 'AK';
    return user.displayName.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  });

  // Toast Feedback Helper
  public showToast(message: string, type: 'success' | 'info' | 'amber' = 'success'): void {
    const toast: ToastAlert = { id: `t-${Date.now()}`, message, type };
    this.toasts.update(list => [...list, toast]);
    setTimeout(() => {
      this.toasts.update(list => list.filter(t => t.id !== toast.id));
    }, 3200);
  }

  // Auth Persona Login
  public loginWithPersona(role: UserRole): void {
    const user = this.authService.loginAsPersona(role);
    this.audioService.playBeep('success');
    this.showToast(`Welcome back, ${user.displayName}! Signed in as ${user.role}.`, 'success');
    if (role === 'Cashier') this.activeTab.set('pos');
    else if (role === 'Admin') this.activeTab.set('products');
    else this.activeTab.set('sales');
  }

  public submitLogin(e: Event): void {
    e.preventDefault();
    if (!this.loginUsername().trim() || !this.loginPassword().trim()) {
      this.showToast('Please enter both username and password!', 'amber');
      return;
    }
    const uname = this.loginUsername();
    let role: UserRole = 'Admin';
    if (uname.includes('anant') || uname.includes('ceo')) role = 'CEO';
    if (uname.includes('cashier')) role = 'Cashier';
    this.loginWithPersona(role);
  }

  public submitRegister(e: Event): void {
    e.preventDefault();
    if (!this.regName().trim() || !this.regEmail().trim()) {
      this.showToast('Please fill all required registration fields!', 'amber');
      return;
    }
    const user = this.authService.loginAsPersona(this.regRole());
    user.displayName = this.regName();
    user.email = this.regEmail();
    this.authService.setSession(user);
    this.showToast(`Account created for ${this.regName()}!`, 'success');
    this.activeTab.set('pos');
  }

  public logout(): void {
    this.authService.logout();
    this.showToast('Signed out of ERPInfinity session.', 'info');
  }

  // POS Billing Logic with Strict UI Validation
  public addItemByBarcode(code?: string): void {
    const barcode = code || this.barcodeInput().trim();
    if (!barcode) {
      this.showToast('Please enter or scan a valid EAN barcode!', 'amber');
      return;
    }

    let target = this.catalog().find(p => p.barcode === barcode || p.skuCode === barcode);
    if (!target) {
      target = {
        id: `GEN-${Date.now()}`,
        skuCode: `SKU-GEN-${barcode.slice(-4)}`,
        productName: `Scanned Item #${barcode}`,
        category: 'General',
        barcode: barcode,
        mrp: 100.00,
        sellingPrice: 90.00,
        taxPercentage: 5.00,
        hsnCode: '25010010'
      };
    }

    const currentCart = [...this.cart()];
    const idx = currentCart.findIndex(i => i.barcode === barcode || i.skuCode === barcode);
    if (idx > -1) {
      currentCart[idx].quantity += 1;
    } else {
      currentCart.push({ ...target, quantity: 1 });
    }

    this.cart.set(currentCart);
    this.barcodeInput.set('');
    this.audioService.playBeep('scan');
    this.showToast(`Added: ${target.productName}`, 'success');
  }

  public updateQty(index: number, newQty: number): void {
    if (newQty <= 0) {
      this.showToast('Quantity must be at least 1!', 'amber');
      return;
    }
    const current = [...this.cart()];
    current[index].quantity = newQty;
    this.cart.set(current);
  }

  public removeCartItem(index: number): void {
    const current = [...this.cart()];
    const removed = current.splice(index, 1);
    this.cart.set(current);
    if (removed[0]) this.showToast(`Removed ${removed[0].productName}`, 'amber');
  }

  public completeCheckout(): void {
    if (this.cart().length === 0) {
      this.showToast('Cart is empty! Scan items before completing checkout.', 'amber');
      return;
    }

    if (this.paymentMode() === 'CASH' && this.cashTendered() > 0 && this.cashTendered() < this.cartGrandTotal()) {
      this.showToast(`Cash tendered (₹${this.cashTendered()}) is less than Total (₹${this.cartGrandTotal().toFixed(2)})!`, 'amber');
      return;
    }

    const invNum = `INV-${new Date().toISOString().slice(0, 10).replace(/-/g, '')}-${Math.floor(100000 + Math.random() * 900000)}`;
    const checkoutCart = [...this.cart()];

    this.currentReceipt.set({
      invNum: invNum,
      date: new Date().toLocaleString(),
      customer: this.activeCustomer().name,
      subtotal: this.cartSubtotal(),
      tax: this.cartTax(),
      discount: this.discountAmount(),
      grandTotal: this.cartGrandTotal(),
      mode: this.paymentMode(),
      items: checkoutCart
    });

    this.salesTotal.update(v => v + this.cartGrandTotal());
    this.invoiceCount.update(v => v + 1);
    this.totalGstCollected.update(v => v + this.cartTax());

    const newInvoice: SalesInvoice = {
      invoiceNum: invNum,
      customer: this.activeCustomer().name,
      date: new Date().toLocaleTimeString(),
      itemsCount: this.cartItemsCount(),
      mode: this.paymentMode(),
      amount: this.cartGrandTotal()
    };
    this.salesInvoices.update(list => [newInvoice, ...list]);

    // Deduct Stock
    this.inventory.update(list => {
      return list.map(item => {
        const cartMatch = checkoutCart.find(c => c.skuCode === item.skuCode);
        if (cartMatch) {
          const newOnHand = Math.max(0, item.onHand - cartMatch.quantity);
          return { ...item, onHand: newOnHand, available: newOnHand };
        }
        return item;
      });
    });

    this.audioService.playBeep('success');
    this.activeReceiptModal.set(true);
    this.showToast(`Transaction ${invNum} Completed Successfully!`, 'success');
    this.cart.set([]);
    this.cashTendered.set(0);
  }

  // Add Product Form Handler
  public submitAddProduct(e: Event): void {
    e.preventDefault();
    if (!this.newProdName().trim() || !this.newProdSku().trim() || !this.newProdBarcode().trim()) {
      this.showToast('Please fill all required product details!', 'amber');
      return;
    }
    const mrp = this.newProdMrp() || 100;
    const price = this.newProdPrice() || 90;

    const newProd: ProductItem = {
      id: `PROD-${Date.now()}`,
      skuCode: this.newProdSku(),
      productName: this.newProdName(),
      category: this.newProdCat(),
      barcode: this.newProdBarcode(),
      mrp: mrp,
      sellingPrice: price,
      taxPercentage: this.newProdTax(),
      hsnCode: this.newProdHsn()
    };

    this.catalog.update(list => [...list, newProd]);
    this.inventory.update(list => [...list, {
      location: 'WH-CENTRAL-01',
      skuCode: this.newProdSku(),
      productName: this.newProdName(),
      onHand: 100,
      reserved: 0,
      available: 100,
      reorderPoint: 20,
      costPrice: price * 0.8
    }]);

    this.activeAddProductModal.set(false);
    this.showToast(`Product '${this.newProdName()}' added to catalog!`, 'success');
    this.newProdName.set('');
    this.newProdSku.set('');
    this.newProdBarcode.set('');
  }

  // Receive GRN Handler
  public submitReceiveGRN(e: Event): void {
    e.preventDefault();
    const qty = this.grnQty();
    if (qty <= 0) {
      this.showToast('GRN Quantity must be greater than 0!', 'amber');
      return;
    }

    this.inventory.update(list => {
      const idx = list.findIndex(i => i.skuCode === this.grnSku() && i.location === this.grnLocation());
      if (idx > -1) {
        const copy = [...list];
        copy[idx].onHand += qty;
        copy[idx].available += qty;
        return copy;
      }
      const prod = this.catalog().find(p => p.skuCode === this.grnSku());
      return [...list, {
        location: this.grnLocation(),
        skuCode: this.grnSku(),
        productName: prod ? prod.productName : this.grnSku(),
        onHand: qty,
        reserved: 0,
        available: qty,
        reorderPoint: 20,
        costPrice: 50.00
      }];
    });

    this.activeGrnModal.set(false);
    this.showToast(`GRN Stock +${qty} units posted to ${this.grnLocation()}!`, 'success');
  }

  // Create PO Handler
  public submitCreatePO(e: Event): void {
    e.preventDefault();
    if (!this.poVendor().trim() || !this.poAmount() || this.poAmount()! <= 0) {
      this.showToast('Please enter vendor name and valid PO amount!', 'amber');
      return;
    }

    const poNum = `PO-${new Date().toISOString().slice(0, 10).replace(/-/g, '')}-${Math.floor(10000 + Math.random() * 90000)}`;
    const newPO: PurchaseOrder = {
      poNumber: poNum,
      vendor: this.poVendor(),
      warehouse: this.poWarehouse(),
      status: 'Issued & Sent',
      amount: this.poAmount()!,
      date: 'Just now'
    };

    this.purchaseOrders.update(list => [newPO, ...list]);
    this.activePoModal.set(false);
    this.showToast(`Purchase Order ${poNum} issued to ${this.poVendor()}!`, 'success');
    this.poVendor.set('');
    this.poAmount.set(null);
  }

  public refreshHealth(): void {
    this.services.update(list => list.map(s => ({
      ...s,
      pingMs: parseFloat((Math.random() * 1.8 + 0.8).toFixed(1))
    })));
    this.showToast('Microservices Topology Health Refreshed: 100% OPERATIONAL', 'success');
  }

  public printReceipt(): void {
    window.print();
  }
}
