using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace ERPInfinity.BuildingBlocks.CQRS.Security;

public static class ScopeAuthorizationExtensions
{
    public static IServiceCollection AddMicroserviceScopePolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Internal M2M Service-to-Service Policy
            options.AddPolicy("InternalServiceOnly", policy =>
                policy.RequireClaim("scope", "erpinfinity.internal"));

            // Product Scopes & Permissions
            options.AddPolicy("ProductRead", policy =>
                policy.RequireAssertion(ctx =>
                    ctx.User.HasClaim("scope", "product.read") ||
                    ctx.User.HasClaim("permission", "Product.View") ||
                    ctx.User.HasClaim("scope", "erpinfinity.internal")));

            options.AddPolicy("ProductWrite", policy =>
                policy.RequireAssertion(ctx =>
                    ctx.User.HasClaim("scope", "product.write") ||
                    ctx.User.HasClaim("permission", "Product.Create") ||
                    ctx.User.HasClaim("scope", "erpinfinity.internal")));

            // Inventory Scopes & Permissions
            options.AddPolicy("InventoryRead", policy =>
                policy.RequireAssertion(ctx =>
                    ctx.User.HasClaim("scope", "inventory.read") ||
                    ctx.User.HasClaim("permission", "Inventory.View") ||
                    ctx.User.HasClaim("scope", "erpinfinity.internal")));

            options.AddPolicy("InventoryAdjust", policy =>
                policy.RequireAssertion(ctx =>
                    ctx.User.HasClaim("scope", "inventory.adjust") ||
                    ctx.User.HasClaim("permission", "Inventory.Adjust") ||
                    ctx.User.HasClaim("scope", "erpinfinity.internal")));

            // Sales Scopes & Permissions
            options.AddPolicy("SalesCreate", policy =>
                policy.RequireAssertion(ctx =>
                    ctx.User.HasClaim("scope", "sales.create") ||
                    ctx.User.HasClaim("permission", "Sales.Create") ||
                    ctx.User.HasClaim("scope", "erpinfinity.internal")));

            // Finance Scopes & Permissions
            options.AddPolicy("FinanceView", policy =>
                policy.RequireAssertion(ctx =>
                    ctx.User.HasClaim("scope", "finance.read") ||
                    ctx.User.HasClaim("permission", "Finance.View") ||
                    ctx.User.HasClaim("scope", "erpinfinity.internal")));

            // Identity Scopes & Permissions
            options.AddPolicy("IdentityManage", policy =>
                policy.RequireAssertion(ctx =>
                    ctx.User.HasClaim("scope", "identity.manage") ||
                    ctx.User.HasClaim("permission", "User.Manage") ||
                    ctx.User.HasClaim("scope", "erpinfinity.internal")));
        });

        return services;
    }
}
