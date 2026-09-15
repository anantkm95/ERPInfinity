using ERPInfinity.Inventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERPInfinity.Inventory.Infrastructure;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
    public DbSet<InventoryOutboxMessage> OutboxMessages => Set<InventoryOutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("Locations");
            entity.HasKey(l => l.Id);
            entity.Property(l => l.LocationCode).HasMaxLength(30).IsRequired();
            entity.HasIndex(l => l.LocationCode).IsUnique();
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.ToTable("Stocks");
            entity.HasKey(s => s.Id);
            entity.HasIndex(s => new { s.LocationId, s.SKUId }).IsUnique();
            entity.Property(s => s.QuantityOnHand).HasColumnType("decimal(18,3)");
            entity.Property(s => s.ReservedQuantity).HasColumnType("decimal(18,3)");
            entity.Property(s => s.AvailableQuantity).HasColumnType("decimal(18,3)");
        });

        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.ToTable("StockTransactions");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.TransactionType).HasMaxLength(50).IsRequired();
            entity.Property(t => t.QuantityChange).HasColumnType("decimal(18,3)");
        });

        modelBuilder.Entity<InventoryOutboxMessage>(entity =>
        {
            entity.ToTable("InventoryOutbox");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.EventType).HasMaxLength(100).IsRequired();
        });
    }
}

public class InventoryOutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
