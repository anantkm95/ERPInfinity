using ERPInfinity.Product.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERPInfinity.Product.Infrastructure;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Product> Products => Set<Domain.Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<ProductSKU> ProductSKUs => Set<ProductSKU>();
    public DbSet<Barcode> Barcodes => Set<Barcode>();
    public DbSet<ProductOutboxMessage> OutboxMessages => Set<ProductOutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product Configuration
        modelBuilder.Entity<Domain.Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.ProductCode).HasMaxLength(30).IsRequired();
            entity.HasIndex(p => p.ProductCode).IsUnique();
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.HSNCode).HasMaxLength(20);
            entity.Property(p => p.TaxPercentage).HasColumnType("decimal(5,2)");
        });

        // Category Configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.CategoryCode).HasMaxLength(20).IsRequired();
            entity.HasIndex(c => c.CategoryCode).IsUnique();
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
        });

        // Brand Configuration
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.ToTable("Brands");
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Name).HasMaxLength(100).IsRequired();
        });

        // ProductSKU Configuration
        modelBuilder.Entity<ProductSKU>(entity =>
        {
            entity.ToTable("ProductSKUs");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.SKUCode).HasMaxLength(50).IsRequired();
            entity.HasIndex(s => s.SKUCode).IsUnique();
            entity.Property(s => s.MRP).HasColumnType("decimal(18,2)");
            entity.Property(s => s.SellingPrice).HasColumnType("decimal(18,2)");
        });

        // Barcode Configuration
        modelBuilder.Entity<Barcode>(entity =>
        {
            entity.ToTable("Barcodes");
            entity.HasKey(b => b.Id);
            entity.Property(b => b.BarcodeNumber).HasMaxLength(50).IsRequired();
            entity.HasIndex(b => b.BarcodeNumber).IsUnique();
        });

        // Outbox Configuration
        modelBuilder.Entity<ProductOutboxMessage>(entity =>
        {
            entity.ToTable("ProductOutbox");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.EventType).HasMaxLength(100).IsRequired();
        });
    }
}

public class ProductOutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
