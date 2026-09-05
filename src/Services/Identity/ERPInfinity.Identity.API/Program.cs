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
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ERPInfinity Identity Service v1");
    c.RoutePrefix = string.Empty; // Swagger UI at root URL
});

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
