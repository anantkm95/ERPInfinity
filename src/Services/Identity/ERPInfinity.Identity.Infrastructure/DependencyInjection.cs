using ERPInfinity.Identity.Application.Abstractions;
using ERPInfinity.Identity.Infrastructure.Persistence;
using ERPInfinity.Identity.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ERPInfinity.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("InMemory"))
        {
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseInMemoryDatabase("ERPInfinity_IdentityDb"));
        }
        else
        {
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName)));
        }

        services.AddScoped<IIdentityDbContext>(provider => provider.GetRequiredService<IdentityDbContext>());
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
