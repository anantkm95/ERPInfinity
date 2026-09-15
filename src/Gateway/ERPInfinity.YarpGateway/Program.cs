using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// 1. Add YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// 2. Add Swagger API Explorer & CORS
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ERPInfinity - Aggregated Microservices Gateway API",
        Version = "v1",
        Description = "Central API Gateway aggregating OpenAPI definitions for all 14 ERPInfinity Microservices.",
        Contact = new OpenApiContact
        {
            Name = "ERPInfinity Architecture Team",
            Email = "support@erpinfinity.com"
        }
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

// 3. Configure Swagger UI with Multi-Service Definition Dropdown
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ERPInfinity - YARP Gateway v1");
    c.SwaggerEndpoint("http://localhost:5001/swagger/v1/swagger.json", "1. Identity & Auth Service v1");
    c.SwaggerEndpoint("http://localhost:5002/swagger/v1/swagger.json", "2. Product Master Service v1");
    c.SwaggerEndpoint("http://localhost:5003/swagger/v1/swagger.json", "3. Inventory & Stock Ledger v1");
    c.SwaggerEndpoint("http://localhost:5004/swagger/v1/swagger.json", "4. Sales & POS Billing Service v1");
    c.SwaggerEndpoint("http://localhost:5005/swagger/v1/swagger.json", "5. Purchase & Procurement v1");
    c.SwaggerEndpoint("http://localhost:5006/swagger/v1/swagger.json", "6. Warehouse & Fulfillment v1");
    c.SwaggerEndpoint("http://localhost:5007/swagger/v1/swagger.json", "7. Order Management Service v1");
    c.SwaggerEndpoint("http://localhost:5008/swagger/v1/swagger.json", "8. Pricing & Promotion Engine v1");
    c.SwaggerEndpoint("http://localhost:5009/swagger/v1/swagger.json", "9. Payment Gateway Service v1");
    c.SwaggerEndpoint("http://localhost:5010/swagger/v1/swagger.json", "10. Finance & General Ledger v1");
    c.SwaggerEndpoint("http://localhost:5011/swagger/v1/swagger.json", "11. Store Infrastructure v1");
    c.SwaggerEndpoint("http://localhost:5012/swagger/v1/swagger.json", "12. Notification Service v1");
    c.SwaggerEndpoint("http://localhost:5013/swagger/v1/swagger.json", "13. Customer & CRM Service v1");
    c.SwaggerEndpoint("http://localhost:5014/swagger/v1/swagger.json", "14. Reporting & BI Analytics v1");
    c.RoutePrefix = "swagger";
});

app.MapReverseProxy();

app.Run();
