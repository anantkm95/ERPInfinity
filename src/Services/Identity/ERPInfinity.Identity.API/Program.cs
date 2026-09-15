using System.Text;
using ERPInfinity.Identity.Application;
using ERPInfinity.Identity.Infrastructure;
using ERPInfinity.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers & API Explorer
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. Add Layer Dependencies (Infrastructure & Application)
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

// 3. Configure JWT Authentication
var secretKey = builder.Configuration["JwtSettings:SecretKey"] ?? "ERPInfinityEnterpriseSuperSecretSecurityKey2026!#$";
var issuer = builder.Configuration["JwtSettings:Issuer"] ?? "ERPInfinity.Identity";
var audience = builder.Configuration["JwtSettings:Audience"] ?? "ERPInfinity.Services";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 4. Configure Swagger / OpenAPI with JWT Authorization Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ERPInfinity - Identity Microservice API",
        Version = "v1",
        Description = "Enterprise Microservice API for Authentication, User Management, RBAC Roles & Permissions, and Refresh Tokens.",
        Contact = new OpenApiContact
        {
            Name = "ERPInfinity Architecture Team",
            Email = "support@erpinfinity.com"
        }
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    var schemeReference = new OpenApiSecuritySchemeReference("Bearer", null);
    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        { schemeReference, new List<string>() }
    });
});

// 5. CORS Policy Setup
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// 6. Database Ensure Created / Seed Data Execution
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    dbContext.Database.EnsureCreated();
}

// 7. Configure Middleware Pipeline
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "1. Identity & Auth Service v1");
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
