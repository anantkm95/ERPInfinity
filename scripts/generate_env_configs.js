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

const envs = [
    { envName: 'Development', suffix: 'DEV', host: '.\\MSSQLSERVER01' },
    { envName: 'INT', suffix: 'INT', host: 'int-sql.erpinfinity.internal' },
    { envName: 'UAT', suffix: 'UAT', host: 'uat-sql.erpinfinity.internal' },
    { envName: 'Production', suffix: 'PROD', host: 'prod-sql.erpinfinity.internal' }
];

const basePath = path.join(__dirname, '..', 'src', 'Services');

services.forEach(s => {
    const apiPath = path.join(basePath, s.name, `ERPInfinity.${s.name}.API`);
    if (fs.existsSync(apiPath)) {
        envs.forEach(e => {
            const envFilePath = path.join(apiPath, `appsettings.${e.envName}.json`);
            const dbName = `${s.db}_${e.suffix}`;
            
            const config = {
                Logging: {
                    LogLevel: {
                        Default: e.envName === 'Production' ? 'Warning' : 'Information',
                        'Microsoft.AspNetCore': 'Warning'
                    }
                },
                Environment: e.envName,
                ConnectionStrings: {
                    DefaultConnection: `Server=${e.host};Database=${dbName};Trusted_Connection=True;TrustServerCertificate=True;`
                },
                MongoDbSettings: {
                    ConnectionString: `mongodb://${e.envName.toLowerCase()}-mongo:27017`,
                    DatabaseName: `ERPInfinity_CQRSRead_${e.suffix}`
                }
            };

            fs.writeFileSync(envFilePath, JSON.stringify(config, null, 2), 'utf-8');
            console.log(`[CREATED] ${s.name} -> appsettings.${e.envName}.json (DB: ${dbName})`);
        });
    }
});
