using ERPInfinity.Identity.Application.Abstractions;
using ERPInfinity.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERPInfinity.Identity.Infrastructure.Persistence;

public class IdentityDbContext : DbContext, IIdentityDbContext
{
    private const string HashedAdminPassword = "mCdDsmssAIq88Ha1qT4G0kbhzltOYnSutuNDkPy2xt/d31XElNuO98R9KE2rWN8Z";

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Username).HasMaxLength(50).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(100).IsRequired();
            entity.Property(u => u.FirstName).HasMaxLength(50);
            entity.Property(u => u.LastName).HasMaxLength(50);
            entity.Property(u => u.PhoneNumber).HasMaxLength(20);
        });

        // Role Configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.Name).IsUnique();
            entity.Property(r => r.Name).HasMaxLength(50).IsRequired();
            entity.Property(r => r.Description).HasMaxLength(200);
        });

        // Permission Configuration
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.PermissionCode).IsUnique();
            entity.Property(p => p.PermissionCode).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Module).HasMaxLength(50).IsRequired();
        });

        // UserRole Composite Key
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);
        });

        // RolePermission Composite Key
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);
        });

        // RefreshToken Configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.Property(rt => rt.Token).HasMaxLength(200).IsRequired();

            entity.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId);
        });

        // Seed Default Roles & Permissions
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Roles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", Description = "System Administrator with full access", IsSystemRole = true },
            new Role { Id = 2, Name = "StoreManager", Description = "Store Manager responsible for inventory and branch sales", IsSystemRole = true },
            new Role { Id = 3, Name = "Cashier", Description = "POS Cashier handling checkout transactions", IsSystemRole = true },
            new Role { Id = 4, Name = "InventoryClerk", Description = "Inventory Clerk managing stock transfers and receipts", IsSystemRole = true }
        );

        // Permissions
        modelBuilder.Entity<Permission>().HasData(
            new Permission { Id = 1, PermissionCode = "User.Manage", Module = "Identity", Description = "Create and manage users" },
            new Permission { Id = 2, PermissionCode = "Role.Manage", Module = "Identity", Description = "Manage roles and permissions" },
            new Permission { Id = 3, PermissionCode = "Product.View", Module = "Product", Description = "View product catalog" },
            new Permission { Id = 4, PermissionCode = "Product.Create", Module = "Product", Description = "Create products and categories" },
            new Permission { Id = 5, PermissionCode = "Inventory.View", Module = "Inventory", Description = "View stock levels" },
            new Permission { Id = 6, PermissionCode = "Inventory.Adjust", Module = "Inventory", Description = "Perform stock adjustments" },
            new Permission { Id = 7, PermissionCode = "Sales.Create", Module = "Sales", Description = "Create sales invoices and POS orders" },
            new Permission { Id = 8, PermissionCode = "Finance.View", Module = "Finance", Description = "View financial reports and ledgers" }
        );

        // Seed Admin User (Password: Admin@123)
        var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = adminId,
                Username = "admin",
                Email = "admin@erpinfinity.com",
                PasswordHash = HashedAdminPassword,
                FirstName = "System",
                LastName = "Administrator",
                PhoneNumber = "+10000000000",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // Seed Admin User Role
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole { UserId = adminId, RoleId = 1, AssignedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Seed Role Permissions (Admin gets all permissions)
        for (int pId = 1; pId <= 8; pId++)
        {
            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { RoleId = 1, PermissionId = pId }
            );
        }
    }
}
