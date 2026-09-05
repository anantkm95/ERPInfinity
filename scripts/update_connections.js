const fs = require('fs');
const path = require('path');

const services = [
    { name: 'Customer', db: 'Db_Customer' },
    { name: 'Finance', db: 'Db_Finance' },
    { name: 'Identity', db: 'Db_Identity' },
    { name: 'Inventory', db: 'Db_Inventory' },
    { name: 'Notification', db: 'Db_Notification' },
    { name: 'Order', db: 'Db_Order' },
    { name: 'Payment', db: 'Db_Payment' },
    { name: 'Pricing', db: 'Db_Pricing' },
    { name: 'Product', db: 'Db_Product' },
    { name: 'Purchase', db: 'Db_Purchase' },
    { name: 'Reporting', db: 'Db_Reporting' },
    { name: 'Sales', db: 'Db_Sales' },
    { name: 'Store', db: 'Db_Store' },
    { name: 'Warehouse', db: 'Db_Warehouse' }
];

const basePath = path.join(__dirname, '..', 'src', 'Services');

services.forEach(s => {
    const appsettingsPath = path.join(basePath, s.name, `ERPInfinity.${s.name}.API`, 'appsettings.json');
    if (fs.existsSync(appsettingsPath)) {
        try {
            const rawData = fs.readFileSync(appsettingsPath, 'utf-8');
            const json = JSON.parse(rawData);

            json.ConnectionStrings = json.ConnectionStrings || {};
            json.ConnectionStrings.DefaultConnection = `Server=.\\MSSQLSERVER01;Database=${s.db};Trusted_Connection=True;TrustServerCertificate=True;`;

            json.MongoDbSettings = json.MongoDbSettings || {
                ConnectionString: 'mongodb://localhost:27017',
                DatabaseName: 'ERPInfinity_CQRSRead'
            };

            fs.writeFileSync(appsettingsPath, JSON.stringify(json, null, 2), 'utf-8');
            console.log(`[SUCCESS] Mapped ERPInfinity.${s.name}.API -> Database=${s.db}`);
        } catch (err) {
            console.error(`[ERROR] Failed to process ${appsettingsPath}:`, err.message);
        }
    } else {
        console.warn(`[WARN] File not found: ${appsettingsPath}`);
    }
});
