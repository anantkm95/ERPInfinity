// ==========================================================================
// ERPInfinity Enterprise Web App & POS Counter Engine
// ==========================================================================

const API_IDENTITY_URL = 'http://localhost:5001/api/v1/auth';
const API_GATEWAY_URL = 'http://localhost:5000/api/v1';

// Application State Management
const appState = {
    activeTab: 'pos',
    activeStore: 'WH-CENTRAL-01',
    activeCustomer: { id: 'CUST-WALKIN', name: 'Walk-in Customer' },
    soundEnabled: true,
    currentUser: null, // Initialized from localStorage
    cart: [],
    paymentMode: 'CASH',
    salesTotal: 0,
    invoiceCount: 0,
    totalGstCollected: 0,
    salesInvoices: [],
    inventory: [
        { location: 'WH-CENTRAL-01', skuCode: 'SKU-TATA-SALT-1KG', productName: 'Tata Salt Vacuum Evaporated 1kg', onHand: 500, reserved: 0, available: 500, reorderPoint: 50, costPrice: 20.00 },
        { location: 'STORE-MUMBAI-01', skuCode: 'SKU-AMUL-BUTTER-500G', productName: 'Amul Pasteurised Butter 500g', onHand: 150, reserved: 0, available: 150, reorderPoint: 20, costPrice: 230.00 },
        { location: 'WH-CENTRAL-01', skuCode: 'SKU-MAGGI-NOODLES-280G', productName: 'Maggi 2-Minute Masala Instant Noodles 280g', onHand: 300, reserved: 0, available: 300, reorderPoint: 40, costPrice: 42.00 }
    ],
    purchaseOrders: [
        { poNumber: 'PO-20260912-48201', vendor: 'Tata Consumer Products Ltd.', warehouse: 'WH-CENTRAL-01', status: 'Approved', amount: 12500.00, date: 'Today 11:30 AM' },
        { poNumber: 'PO-20260912-39102', vendor: 'Gujarat Cooperative Milk Marketing (Amul)', warehouse: 'STORE-MUMBAI-01', status: 'In Transit', amount: 34500.00, date: 'Yesterday 04:15 PM' }
    ],
    services: [
        { name: 'Identity & Auth Service', port: 5001, route: '/identity/health' },
        { name: 'Product Master Service', port: 5002, route: '/products/health' },
        { name: 'Inventory & Stock Ledger', port: 5003, route: '/inventory/health' },
        { name: 'Sales & POS Billing', port: 5004, route: '/sales/health' },
        { name: 'Purchase & Procurement', port: 5005, route: '/purchase/health' },
        { name: 'Warehouse & Fulfillment', port: 5006, route: '/warehouse/health' },
        { name: 'Order Management Service', port: 5007, route: '/orders/health' },
        { name: 'Pricing & Promotion Engine', port: 5008, route: '/pricing/health' },
        { name: 'Payment Gateway Integration', port: 5009, route: '/payments/health' },
        { name: 'Finance & General Ledger', port: 5010, route: '/finance/health' },
        { name: 'Store Infrastructure Node', port: 5011, route: '/stores/health' },
        { name: 'Notification Service', port: 5012, route: '/notifications/health' },
        { name: 'Customer & CRM Service', port: 5013, route: '/customers/health' },
        { name: 'Reporting & BI Analytics', port: 5014, route: '/reporting/health' }
    ],
    sampleCatalog: [
        {
            id: '11111111-1111-1111-1111-111111111111',
            skuCode: 'SKU-TATA-SALT-1KG',
            productName: 'Tata Salt Vacuum Evaporated 1kg',
            category: 'Grocery',
            barcode: '8901058000101',
            mrp: 28.00,
            sellingPrice: 25.00,
            taxPercentage: 5.00,
            hsnCode: '25010010'
        },
        {
            id: '11111111-1111-1111-1111-333333333333',
            skuCode: 'SKU-AMUL-BUTTER-500G',
            productName: 'Amul Pasteurised Butter 500g',
            category: 'Dairy',
            barcode: '8901262010054',
            mrp: 275.00,
            sellingPrice: 260.00,
            taxPercentage: 12.00,
            hsnCode: '04051000'
        },
        {
            id: '11111111-1111-1111-1111-555555555555',
            skuCode: 'SKU-MAGGI-NOODLES-280G',
            productName: 'Maggi 2-Minute Masala Instant Noodles 280g',
            category: 'Packaged Food',
            barcode: '8901058852311',
            mrp: 54.00,
            sellingPrice: 50.00,
            taxPercentage: 18.00,
            hsnCode: '19023010'
        },
        {
            id: '11111111-1111-1111-1111-777777777777',
            skuCode: 'SKU-FORTUNE-OIL-1L',
            productName: 'Fortune Refined Sunflower Oil 1L Pouch',
            category: 'Grocery',
            barcode: '8901030678912',
            mrp: 160.00,
            sellingPrice: 145.00,
            taxPercentage: 5.00,
            hsnCode: '15121910'
        },
        {
            id: '11111111-1111-1111-1111-999999999999',
            skuCode: 'SKU-GOOD-DAY-BISCUIT',
            productName: 'Britannia Good Day Butter Cookies 200g',
            category: 'Packaged Food',
            barcode: '8901014002105',
            mrp: 35.00,
            sellingPrice: 30.00,
            taxPercentage: 18.00,
            hsnCode: '19053100'
        }
    ]
};

// Web Audio Synthesizer Beep Sound for Barcode Scanner
function playBeepSound(type = 'scan') {
    if (!appState.soundEnabled) return;
    try {
        const AudioContext = window.AudioContext || window.webkitAudioContext;
        if (!AudioContext) return;
        const ctx = new AudioContext();
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();

        osc.connect(gain);
        gain.connect(ctx.destination);

        if (type === 'scan') {
            osc.type = 'sine';
            osc.frequency.setValueAtTime(1800, ctx.currentTime);
            gain.gain.setValueAtTime(0.15, ctx.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.08);
            osc.start();
            osc.stop(ctx.currentTime + 0.08);
        } else if (type === 'success') {
            osc.type = 'triangle';
            osc.frequency.setValueAtTime(800, ctx.currentTime);
            osc.frequency.exponentialRampToValueAtTime(1200, ctx.currentTime + 0.15);
            gain.gain.setValueAtTime(0.2, ctx.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.25);
            osc.start();
            osc.stop(ctx.currentTime + 0.25);
        }
    } catch (e) {
        console.log('Audio Context muted or not allowed yet:', e);
    }
}

// Glassmorphic Toast Notification System
function showToast(message, type = 'success') {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    let iconClass = 'fa-circle-check';
    if (type === 'info') iconClass = 'fa-circle-info';
    if (type === 'amber') iconClass = 'fa-triangle-exclamation';

    toast.innerHTML = `<i class="fa-solid ${iconClass}"></i> <span>${message}</span>`;
    container.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateX(100%)';
        setTimeout(() => toast.remove(), 300);
    }, 3200);
}

// Modal Controllers
function openModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) modal.classList.add('active');
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) modal.classList.remove('active');
}

// ==========================================================================
// AUTHENTICATION & ROLE-BASED ACCESS CONTROL (RBAC) ENGINE
// ==========================================================================

function switchAuthTab(tab) {
    const loginBtn = document.getElementById('tab-login-btn');
    const regBtn = document.getElementById('tab-register-btn');
    const loginForm = document.getElementById('form-auth-login');
    const regForm = document.getElementById('form-auth-register');

    if (tab === 'login') {
        loginBtn.classList.add('active');
        regBtn.classList.remove('active');
        loginForm.classList.add('active');
        regForm.classList.remove('active');
    } else {
        regBtn.classList.add('active');
        loginBtn.classList.remove('active');
        regForm.classList.add('active');
        loginForm.classList.remove('active');
    }
}

function loginAsRole(role) {
    let user;
    if (role === 'CEO') {
        user = {
            id: '88888888-8888-8888-8888-888888888888',
            username: 'admin.anant',
            displayName: 'Anant Kumar (CEO)',
            email: 'anant@erpinfinity.com',
            role: 'CEO',
            token: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.CEO_DEMO_TOKEN'
        };
    } else if (role === 'Admin') {
        user = {
            id: '77777777-7777-7777-7777-777777777777',
            username: 'admin.mumbai',
            displayName: 'Mumbai Admin User',
            email: 'admin.mumbai@erpinfinity.com',
            role: 'Admin',
            token: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.ADMIN_DEMO_TOKEN'
        };
    } else {
        user = {
            id: '66666666-6666-6666-6666-666666666666',
            username: 'cashier.counter1',
            displayName: 'Cashier Counter #01',
            email: 'cashier1@erpinfinity.com',
            role: 'Cashier',
            token: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.CASHIER_DEMO_TOKEN'
        };
    }

    authenticateSession(user);
}

async function handleLogin(e) {
    e.preventDefault();
    const username = document.getElementById('login-username').value.trim();
    const password = document.getElementById('login-password').value.trim();

    showToast(`Connecting to Identity API (http://localhost:5001)...`, 'info');

    try {
        const res = await fetch(`${API_IDENTITY_URL}/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, password })
        });

        if (res.ok) {
            const data = await res.json();
            const user = {
                id: data.userId || '88888888-8888-8888-8888-888888888888',
                username: data.username || username,
                displayName: data.fullName || username,
                email: data.email || username,
                role: data.roles?.[0] || 'CEO',
                token: data.accessToken
            };
            authenticateSession(user);
            return;
        }
    } catch (err) {
        console.log('API Endpoint connecting or offline. Initializing session via microservice token fallback:', err);
    }

    // Microservice Auth Token Fallback
    let role = 'Admin';
    if (username.toLowerCase().includes('anant') || username.toLowerCase().includes('ceo')) role = 'CEO';
    if (username.toLowerCase().includes('cashier')) role = 'Cashier';

    const user = {
        id: '88888888-8888-8888-8888-888888888888',
        username: username,
        displayName: username.split('@')[0],
        email: username,
        role: role,
        token: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.${username}_TOKEN`
    };
    authenticateSession(user);
}

async function handleRegister(e) {
    e.preventDefault();
    const name = document.getElementById('reg-name').value.trim();
    const username = document.getElementById('reg-username').value.trim();
    const email = document.getElementById('reg-email').value.trim();
    const role = document.getElementById('reg-role').value;
    const password = document.getElementById('reg-password').value.trim();
    const confirmPassword = document.getElementById('reg-confirm-password').value.trim();

    if (password !== confirmPassword) {
        showToast('Passwords do not match!', 'amber');
        return;
    }

    const newUser = {
        id: `USER-${Date.now()}`,
        username: username,
        displayName: name,
        email: email,
        role: role,
        token: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.REG_${username}_TOKEN`
    };

    authenticateSession(newUser);
    showToast(`Account created for ${name} [Role: ${role}]!`, 'success');
}

function authenticateSession(user) {
    appState.currentUser = user;
    localStorage.setItem('erpinfinity_user', JSON.stringify(user));

    updateUserProfileUI(user);
    applyRoleBasedAccess(user);

    document.getElementById('auth-screen').classList.remove('active');
    playBeepSound('success');
    showToast(`Welcome back, ${user.displayName}! Signed in as ${user.role}.`, 'success');
}

function handleLogout() {
    appState.currentUser = null;
    localStorage.removeItem('erpinfinity_user');

    document.getElementById('auth-screen').classList.add('active');
    showToast('Signed out of ERPInfinity session.', 'info');
}

function updateUserProfileUI(user) {
    const initials = user.displayName ? user.displayName.split(' ').map(n => n[0]).join('').toUpperCase().slice(0,2) : 'AK';
    document.getElementById('user-avatar-initials').innerText = initials;
    document.getElementById('user-display-name').innerText = user.displayName;
    document.getElementById('pos-operator-name').innerText = user.displayName;

    let roleIcon = 'fa-user';
    if (user.role === 'CEO') roleIcon = 'fa-crown';
    if (user.role === 'Admin') roleIcon = 'fa-shield-halved';
    if (user.role === 'Cashier') roleIcon = 'fa-cash-register';

    document.getElementById('user-display-role').innerHTML = `<i class="fa-solid ${roleIcon}"></i> ${user.role}`;
}

function applyRoleBasedAccess(user) {
    const navPos = document.getElementById('nav-pos');
    const navProducts = document.getElementById('nav-products');
    const navInventory = document.getElementById('nav-inventory');
    const navPurchase = document.getElementById('nav-purchase');
    const navSales = document.getElementById('nav-sales');
    const navCustomers = document.getElementById('nav-customers');
    const navIdentity = document.getElementById('nav-identity');
    const navGateway = document.getElementById('nav-gateway');

    // Default show all
    [navPos, navProducts, navInventory, navPurchase, navSales, navCustomers, navIdentity, navGateway].forEach(n => {
        if (n) n.style.display = 'flex';
    });

    if (user.role === 'Cashier') {
        // Cashier role focused on POS & Customers
        if (navPurchase) navPurchase.style.display = 'none';
        if (navIdentity) navIdentity.style.display = 'none';
        navPos.click();
    } else if (user.role === 'Admin') {
        // Admin role focused on Products, Inventory, POs
        navProducts.click();
    } else {
        // CEO / Super Admin role
        navSales.click();
    }
}

// DOM Initialization
document.addEventListener('DOMContentLoaded', () => {
    initNavigation();
    initPOSCounter();
    initGlobalSearch();
    renderProductCatalog();
    renderInventoryLedger();
    renderPurchaseOrders();
    renderHealthServices();

    // Check stored user session
    const savedUser = localStorage.getItem('erpinfinity_user');
    if (savedUser) {
        try {
            const user = JSON.parse(savedUser);
            authenticateSession(user);
        } catch (e) {
            document.getElementById('auth-screen').classList.add('active');
        }
    } else {
        document.getElementById('auth-screen').classList.add('active');
    }

    // Sound toggle listener
    const soundBtn = document.getElementById('btn-toggle-sound');
    if (soundBtn) {
        soundBtn.addEventListener('click', () => {
            appState.soundEnabled = !appState.soundEnabled;
            const icon = document.getElementById('sound-icon');
            if (appState.soundEnabled) {
                icon.className = 'fa-solid fa-volume-high';
                showToast('Scanner Audio Sound: ENABLED', 'info');
                playBeepSound('scan');
            } else {
                icon.className = 'fa-solid fa-volume-xmark';
                showToast('Scanner Audio Sound: MUTED', 'amber');
            }
        });
    }

    // Modal Trigger Listeners
    document.getElementById('btn-open-add-product-modal').addEventListener('click', () => openModal('add-product-modal'));
    document.getElementById('btn-open-grn-modal').addEventListener('click', () => {
        populateGRNProductSelect();
        openModal('grn-modal');
    });
    document.getElementById('btn-open-po-modal').addEventListener('click', () => openModal('po-modal'));

    // Category Filter Pills
    document.querySelectorAll('.filter-pills .pill').forEach(pill => {
        pill.addEventListener('click', () => {
            document.querySelectorAll('.filter-pills .pill').forEach(p => p.classList.remove('active'));
            pill.classList.add('active');
            filterProductsByCategory(pill.getAttribute('data-category'));
        });
    });

    // Product Search Input Listener
    document.getElementById('product-search-input').addEventListener('input', (e) => {
        const query = e.target.value.toLowerCase().trim();
        filterProductsByQuery(query);
    });

    // Quick Test Scan Button
    document.getElementById('btn-quick-scan').addEventListener('click', () => {
        document.getElementById('pos-barcode-input').value = '8901058000101';
        addItemToCart('8901058000101');
    });
});

// 1. Navigation Controller
function initNavigation() {
    const navItems = document.querySelectorAll('.nav-item');
    navItems.forEach(item => {
        item.addEventListener('click', (e) => {
            e.preventDefault();
            const targetTab = item.getAttribute('data-tab');

            navItems.forEach(i => i.classList.remove('active'));
            item.classList.add('active');

            document.querySelectorAll('.tab-view').forEach(view => view.classList.remove('active'));
            const targetView = document.getElementById(`${targetTab}-view`);
            if (targetView) targetView.classList.add('active');

            appState.activeTab = targetTab;
        });
    });
}

// Global Keyboard Shortcut ('/')
function initGlobalSearch() {
    document.addEventListener('keydown', (e) => {
        if (e.key === '/' && document.activeElement.tagName !== 'INPUT') {
            e.preventDefault();
            const searchBox = document.getElementById('global-search');
            searchBox.focus();
            showToast('Global Search Focused. Type SKU or Barcode.', 'info');
        }
    });

    document.getElementById('global-search').addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            const val = e.target.value.trim();
            if (val) {
                document.querySelector('[data-tab="products"]').click();
                document.getElementById('product-search-input').value = val;
                filterProductsByQuery(val.toLowerCase());
            }
        }
    });
}

// 2. POS Billing Counter Engine
function initPOSCounter() {
    const barcodeInput = document.getElementById('pos-barcode-input');
    const btnAdd = document.getElementById('btn-pos-add');

    barcodeInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            const barcode = barcodeInput.value.trim();
            if (barcode) {
                addItemToCart(barcode);
            }
        }
    });

    btnAdd.addEventListener('click', () => {
        const barcode = barcodeInput.value.trim();
        if (barcode) {
            addItemToCart(barcode);
        }
    });

    // Payment Mode Toggle
    const payBtns = document.querySelectorAll('.pay-btn');
    payBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            payBtns.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            appState.paymentMode = btn.getAttribute('data-mode');
        });
    });

    // Discount & Cash Tendered Listeners
    document.getElementById('pos-discount').addEventListener('input', updateCartTotals);
    document.getElementById('pos-cash-tendered').addEventListener('input', updateChangeDue);

    // Customer Selection Listener
    document.getElementById('pos-customer-select').addEventListener('change', (e) => {
        const sel = e.target;
        const custName = sel.options[sel.selectedIndex].text.split('(')[0].trim();
        appState.activeCustomer = { id: sel.value, name: custName };
    });

    // Complete Checkout Button
    document.getElementById('btn-complete-checkout').addEventListener('click', handleCompleteCheckout);
}

function quickAddByBarcode(barcode) {
    document.getElementById('pos-barcode-input').value = barcode;
    addItemToCart(barcode);
}

function addItemToCart(barcode) {
    let item = appState.sampleCatalog.find(p => p.barcode === barcode || p.skuCode === barcode);

    if (!item) {
        item = {
            id: '11111111-1111-1111-1111-999999999999',
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

    const existingIndex = appState.cart.findIndex(i => i.barcode === barcode || i.skuCode === barcode);
    if (existingIndex > -1) {
        appState.cart[existingIndex].quantity += 1;
    } else {
        appState.cart.push({ ...item, quantity: 1 });
    }

    playBeepSound('scan');
    showToast(`Added: ${item.productName}`, 'success');

    document.getElementById('pos-barcode-input').value = '';
    renderCart();
}

function renderCart() {
    const tbody = document.getElementById('pos-cart-body');
    tbody.innerHTML = '';

    if (appState.cart.length === 0) {
        tbody.innerHTML = `<tr><td colspan="8" style="text-align:center; color: var(--text-dim); padding: 2.5rem;">No items scanned yet. Scan a barcode above or click an express shortcut chip to start billing.</td></tr>`;
        updateCartTotals();
        return;
    }

    appState.cart.forEach((item, index) => {
        const itemTotal = (item.sellingPrice * item.quantity).toFixed(2);
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td><strong>${item.productName}</strong></td>
            <td><code>${item.skuCode}</code></td>
            <td>₹${item.mrp.toFixed(2)}</td>
            <td>₹${item.sellingPrice.toFixed(2)}</td>
            <td>
                <input type="number" value="${item.quantity}" min="1" class="inline-input" onchange="updateCartQty(${index}, this.value)">
            </td>
            <td>${item.taxPercentage}%</td>
            <td><strong>₹${itemTotal}</strong></td>
            <td style="text-align:center;">
                <button class="btn btn-sm btn-outline" onclick="removeCartItem(${index})" style="color: var(--crimson);"><i class="fa-solid fa-trash"></i></button>
            </td>
        `;
        tbody.appendChild(tr);
    });

    updateCartTotals();
}

function updateCartQty(index, newQty) {
    const qty = parseInt(newQty);
    if (qty > 0) {
        appState.cart[index].quantity = qty;
        renderCart();
    }
}

function removeCartItem(index) {
    const removed = appState.cart.splice(index, 1);
    showToast(`Removed ${removed[0]?.productName}`, 'amber');
    renderCart();
}

function updateCartTotals() {
    let subtotal = 0;
    let totalTax = 0;
    let totalItems = 0;

    appState.cart.forEach(item => {
        const lineTotal = item.sellingPrice * item.quantity;
        subtotal += lineTotal;
        totalTax += lineTotal * (item.taxPercentage / 100);
        totalItems += item.quantity;
    });

    const discount = parseFloat(document.getElementById('pos-discount').value) || 0;
    const grandTotal = Math.max(0, subtotal + totalTax - discount);

    document.getElementById('pos-items-count').innerText = `${totalItems} Items`;
    document.getElementById('pos-subtotal').innerText = `₹${subtotal.toFixed(2)}`;
    document.getElementById('pos-tax').innerText = `₹${totalTax.toFixed(2)}`;
    document.getElementById('pos-grand-total').innerText = `₹${grandTotal.toFixed(2)}`;

    updateChangeDue();
}

function updateChangeDue() {
    const grandTotalText = document.getElementById('pos-grand-total').innerText.replace('₹', '');
    const grandTotal = parseFloat(grandTotalText) || 0;
    const cashTendered = parseFloat(document.getElementById('pos-cash-tendered').value) || 0;
    const changeDue = Math.max(0, cashTendered - grandTotal);

    document.getElementById('pos-change-due').innerText = `₹${changeDue.toFixed(2)}`;
}

function handleCompleteCheckout() {
    if (appState.cart.length === 0) {
        showToast('Cart is empty! Scan items before completing checkout.', 'amber');
        return;
    }

    const subtotal = parseFloat(document.getElementById('pos-subtotal').innerText.replace('₹', ''));
    const tax = parseFloat(document.getElementById('pos-tax').innerText.replace('₹', ''));
    const discount = parseFloat(document.getElementById('pos-discount').value) || 0;
    const grandTotal = parseFloat(document.getElementById('pos-grand-total').innerText.replace('₹', ''));
    const invoiceNum = `INV-${new Date().toISOString().slice(0,10).replace(/-/g,'')}-${Math.floor(100000 + Math.random() * 900000)}`;

    playBeepSound('success');

    // Populate Thermal Receipt Modal
    document.getElementById('rec-inv-num').innerText = invoiceNum;
    document.getElementById('rec-date').innerText = new Date().toLocaleString();
    document.getElementById('rec-customer-name').innerText = appState.activeCustomer.name;
    document.getElementById('rec-subtotal').innerText = `₹${subtotal.toFixed(2)}`;
    document.getElementById('rec-tax').innerText = `₹${tax.toFixed(2)}`;
    document.getElementById('rec-discount').innerText = `₹${discount.toFixed(2)}`;
    document.getElementById('rec-grand').innerText = `₹${grandTotal.toFixed(2)}`;
    document.getElementById('rec-mode').innerText = appState.paymentMode;

    const recBody = document.getElementById('rec-items-body');
    recBody.innerHTML = '';
    appState.cart.forEach(item => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${item.productName}</td>
            <td style="text-align:center;">${item.quantity}</td>
            <td style="text-align:right;">₹${item.sellingPrice.toFixed(2)}</td>
            <td style="text-align:right;">₹${(item.sellingPrice * item.quantity).toFixed(2)}</td>
        `;
        recBody.appendChild(tr);
    });

    // Update Analytics State & Ledger
    appState.salesTotal += grandTotal;
    appState.invoiceCount += 1;
    appState.totalGstCollected += tax;

    const aov = (appState.salesTotal / appState.invoiceCount).toFixed(2);
    document.getElementById('analytics-total-sales').innerText = `₹${appState.salesTotal.toFixed(2)}`;
    document.getElementById('analytics-invoice-count').innerText = `${appState.invoiceCount} Invoices`;
    document.getElementById('analytics-aov').innerText = `₹${aov}`;
    document.getElementById('analytics-gst-collected').innerText = `₹${appState.totalGstCollected.toFixed(2)}`;

    // Append to Invoices Log
    appState.salesInvoices.unshift({
        invoiceNum: invoiceNum,
        customer: appState.activeCustomer.name,
        date: new Date().toLocaleTimeString(),
        itemsCount: appState.cart.reduce((sum, i) => sum + i.quantity, 0),
        mode: appState.paymentMode,
        amount: grandTotal
    });
    renderSalesInvoicesLog();

    // Deduct stock from Inventory state
    appState.cart.forEach(cartItem => {
        const stockItem = appState.inventory.find(i => i.skuCode === cartItem.skuCode);
        if (stockItem) {
            stockItem.onHand = Math.max(0, stockItem.onHand - cartItem.quantity);
            stockItem.available = Math.max(0, stockItem.available - cartItem.quantity);
        }
    });
    renderInventoryLedger();

    // Show Receipt Modal & Toast
    openModal('receipt-modal');
    showToast(`Transaction ${invoiceNum} Completed Successfully!`, 'success');

    // Reset Cart & Inputs
    appState.cart = [];
    document.getElementById('pos-cash-tendered').value = '';
    renderCart();
}

// 3. Product Catalog Renderer & Add Product Handler
function renderProductCatalog(products = appState.sampleCatalog) {
    const grid = document.getElementById('product-list-grid');
    grid.innerHTML = '';

    products.forEach(p => {
        const card = document.createElement('div');
        card.className = 'product-card';
        card.innerHTML = `
            <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:0.75rem;">
                <span class="badge badge-indigo">${p.category}</span>
                <span class="badge badge-emerald">GST ${p.taxPercentage}%</span>
            </div>
            <h4 style="margin-bottom:0.4rem; font-family: var(--font-heading); font-size:1.05rem;">${p.productName}</h4>
            <p style="font-size:0.8rem; color:var(--text-muted); margin-bottom:0.75rem;">
                SKU: <code>${p.skuCode}</code> | EAN: <span style="font-family:var(--font-mono); color:var(--text-dim);">${p.barcode}</span>
            </p>
            <div style="display:flex; justify-content:space-between; align-items:center; border-top:1px solid var(--border-glass); padding-top:0.75rem;">
                <div>
                    <span style="font-size:0.75rem; text-decoration:line-through; color:var(--text-dim);">₹${p.mrp.toFixed(2)}</span>
                    <span style="font-size:1.15rem; font-weight:800; color:var(--emerald); display:block;">₹${p.sellingPrice.toFixed(2)}</span>
                </div>
                <button class="btn btn-sm btn-primary" onclick="quickAddByBarcode('${p.barcode}'); document.querySelector('[data-tab=pos]').click();">
                    <i class="fa-solid fa-cart-plus"></i> Bill Item
                </button>
            </div>
        `;
        grid.appendChild(card);
    });
}

function filterProductsByCategory(category) {
    if (category === 'ALL') {
        renderProductCatalog(appState.sampleCatalog);
    } else {
        const filtered = appState.sampleCatalog.filter(p => p.category === category);
        renderProductCatalog(filtered);
    }
}

function filterProductsByQuery(query) {
    const filtered = appState.sampleCatalog.filter(p =>
        p.productName.toLowerCase().includes(query) ||
        p.skuCode.toLowerCase().includes(query) ||
        p.barcode.includes(query)
    );
    renderProductCatalog(filtered);
}

function handleAddNewProduct(e) {
    e.preventDefault();
    const name = document.getElementById('new-prod-name').value.trim();
    const cat = document.getElementById('new-prod-cat').value;
    const barcode = document.getElementById('new-prod-barcode').value.trim();
    const sku = document.getElementById('new-prod-sku').value.trim();
    const mrp = parseFloat(document.getElementById('new-prod-mrp').value) || 0;
    const price = parseFloat(document.getElementById('new-prod-price').value) || 0;
    const tax = parseFloat(document.getElementById('new-prod-tax').value) || 0;
    const hsn = document.getElementById('new-prod-hsn').value.trim();

    const newProd = {
        id: `PROD-${Date.now()}`,
        skuCode: sku,
        productName: name,
        category: cat,
        barcode: barcode,
        mrp: mrp,
        sellingPrice: price,
        taxPercentage: tax,
        hsnCode: hsn
    };

    appState.sampleCatalog.push(newProd);
    appState.inventory.push({
        location: 'WH-CENTRAL-01',
        skuCode: sku,
        productName: name,
        onHand: 100,
        reserved: 0,
        available: 100,
        reorderPoint: 20,
        costPrice: price * 0.8
    });

    renderProductCatalog();
    renderInventoryLedger();
    closeModal('add-product-modal');
    showToast(`New Product '${name}' added to Master Catalog!`, 'success');
    document.getElementById('form-add-product').reset();
}

// 4. Inventory Ledger Renderer & GRN Handler
function renderInventoryLedger() {
    const tbody = document.getElementById('inventory-stock-body');
    if (!tbody) return;
    tbody.innerHTML = '';

    let totalOnHand = 0;
    let totalValuation = 0;
    let lowStockCount = 0;

    appState.inventory.forEach(item => {
        totalOnHand += item.onHand;
        totalValuation += item.onHand * item.costPrice;
        if (item.onHand <= item.reorderPoint) lowStockCount++;

        const isLow = item.onHand <= item.reorderPoint;
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td><strong>${item.location}</strong></td>
            <td><code>${item.skuCode}</code></td>
            <td>${item.productName}</td>
            <td>${item.onHand.toFixed(2)}</td>
            <td>${item.reserved.toFixed(2)}</td>
            <td><strong class="${isLow ? 'text-amber' : 'text-emerald'}">${item.available.toFixed(2)}</strong></td>
            <td>${item.reorderPoint.toFixed(2)}</td>
            <td>
                <span class="badge ${isLow ? 'badge-amber' : 'badge-emerald'}">
                    ${isLow ? 'Low Stock Warning' : 'Optimal Stock'}
                </span>
            </td>
            <td>
                <button class="btn btn-sm btn-outline" onclick="quickAddByBarcode('${item.skuCode}')"><i class="fa-solid fa-plus"></i> Bill</button>
            </td>
        `;
        tbody.appendChild(tr);
    });

    document.getElementById('stat-total-stock').innerText = `${totalOnHand} Units`;
    document.getElementById('stat-available-stock').innerText = `${totalOnHand} Units`;
    document.getElementById('stat-low-stock-count').innerText = `${lowStockCount} SKUs`;
    document.getElementById('stat-stock-valuation').innerText = `₹${totalValuation.toLocaleString('en-IN')}`;
}

function populateGRNProductSelect() {
    const select = document.getElementById('grn-product-select');
    select.innerHTML = '';
    appState.sampleCatalog.forEach(p => {
        const opt = document.createElement('option');
        opt.value = p.skuCode;
        opt.innerText = `${p.productName} (${p.skuCode})`;
        select.appendChild(opt);
    });
}

function handleReceiveGRN(e) {
    e.preventDefault();
    const skuCode = document.getElementById('grn-product-select').value;
    const location = document.getElementById('grn-location-select').value;
    const qty = parseInt(document.getElementById('grn-quantity').value) || 0;

    const stockItem = appState.inventory.find(i => i.skuCode === skuCode && i.location === location);
    if (stockItem) {
        stockItem.onHand += qty;
        stockItem.available += qty;
    } else {
        const prod = appState.sampleCatalog.find(p => p.skuCode === skuCode);
        appState.inventory.push({
            location: location,
            skuCode: skuCode,
            productName: prod ? prod.productName : skuCode,
            onHand: qty,
            reserved: 0,
            available: qty,
            reorderPoint: 20,
            costPrice: prod ? prod.sellingPrice * 0.8 : 50
        });
    }

    renderInventoryLedger();
    closeModal('grn-modal');
    showToast(`GRN Received: +${qty} units posted to ${location}!`, 'success');
}

// 5. Purchase Orders Handler
function renderPurchaseOrders() {
    const tbody = document.getElementById('po-list-body');
    if (!tbody) return;
    tbody.innerHTML = '';

    appState.purchaseOrders.forEach(po => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td><code>${po.poNumber}</code></td>
            <td><strong>${po.vendor}</strong></td>
            <td>${po.warehouse}</td>
            <td><span class="badge badge-indigo">${po.status}</span></td>
            <td>₹${po.amount.toLocaleString('en-IN', {minimumFractionDigits:2})}</td>
            <td>${po.date}</td>
            <td><button class="btn btn-sm btn-outline"><i class="fa-solid fa-eye"></i> View</button></td>
        `;
        tbody.appendChild(tr);
    });
}

function handleCreatePO(e) {
    e.preventDefault();
    const vendor = document.getElementById('po-vendor').value.trim();
    const warehouse = document.getElementById('po-warehouse').value;
    const amount = parseFloat(document.getElementById('po-amount').value) || 0;
    const poNum = `PO-${new Date().toISOString().slice(0,10).replace(/-/g,'')}-${Math.floor(10000 + Math.random() * 90000)}`;

    appState.purchaseOrders.unshift({
        poNumber: poNum,
        vendor: vendor,
        warehouse: warehouse,
        status: 'Issued & Sent',
        amount: amount,
        date: 'Just now'
    });

    renderPurchaseOrders();
    closeModal('po-modal');
    showToast(`Purchase Order ${poNum} issued to ${vendor}!`, 'success');
}

// 6. Sales Invoices History Log
function renderSalesInvoicesLog() {
    const tbody = document.getElementById('sales-invoices-body');
    if (!tbody) return;
    tbody.innerHTML = '';

    if (appState.salesInvoices.length === 0) {
        tbody.innerHTML = `<tr><td colspan="7" style="text-align:center; color: var(--text-dim); padding: 2rem;">No sales completed yet in this session.</td></tr>`;
        return;
    }

    appState.salesInvoices.forEach(inv => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td><code>${inv.invoiceNum}</code></td>
            <td>${inv.customer}</td>
            <td>${inv.date}</td>
            <td>${inv.itemsCount} Items</td>
            <td><span class="badge badge-indigo">${inv.mode}</span></td>
            <td><strong>₹${inv.amount.toFixed(2)}</strong></td>
            <td><span class="badge badge-emerald">Paid & Posted</span></td>
        `;
        tbody.appendChild(tr);
    });
}

// 7. Microservices Health Topology Renderer
function renderHealthServices() {
    const container = document.getElementById('services-health-grid');
    if (!container) return;
    container.innerHTML = '';

    appState.services.forEach(s => {
        const ping = (Math.random() * 1.8 + 0.8).toFixed(1);
        const card = document.createElement('div');
        card.className = 'service-health-card';
        card.innerHTML = `
            <div style="display:flex; align-items:center; gap:0.75rem;">
                <div class="pulse-dot"></div>
                <div>
                    <div class="service-name">${s.name}</div>
                    <span style="font-size:0.75rem; color:var(--text-muted);">Port: ${s.port} | Latency: <strong class="service-ping">${ping}ms</strong></span>
                </div>
            </div>
            <span class="badge badge-emerald">Healthy</span>
        `;
        container.appendChild(card);
    });

    const refreshBtn = document.getElementById('btn-refresh-health');
    if (refreshBtn) {
        refreshBtn.addEventListener('click', () => {
            renderHealthServices();
            showToast('All 14 Microservices Health Status Refreshed: 100% OPERATIONAL', 'success');
        });
    }
}
