using System.Text;
using ERPInfinity.BuildingBlocks.CQRS.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers & API Explorer
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. Configure JWT Authentication
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

// 3. Add Microservice Scope & Permission Authorization Policies
builder.Services.AddMicroserviceScopePolicies();

// 4. Configure Swagger / OpenAPI with Scope Authorization Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ERPInfinity - Pricing & Promotions Service Microservice API",
        Version = "v1",
        Description = "Scope-Protected Microservice API for Pricing & Promotions Service in ERPInfinity Architecture.",
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

// 6. Configure Middleware Pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ERPInfinity Pricing & Promotions Service v1");
    c.RoutePrefix = string.Empty; // Swagger UI at root URL
});

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
