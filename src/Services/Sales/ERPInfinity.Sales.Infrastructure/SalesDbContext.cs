using ERPInfinity.Sales.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERPInfinity.Sales.Infrastructure;

public class SalesDbContext : DbContext
{
    public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options)
    {
    }

    public DbSet<POSRegister> POSRegisters => Set<POSRegister>();
    public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
    public DbSet<SalesInvoiceItem> SalesInvoiceItems => Set<SalesInvoiceItem>();
    public DbSet<SalesReturn> SalesReturns => Set<SalesReturn>();
    public DbSet<SalesOutboxMessage> OutboxMessages => Set<SalesOutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<POSRegister>(entity =>
        {
            entity.ToTable("POSRegisters");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.TerminalCode).HasMaxLength(20).IsRequired();
            entity.HasIndex(r => new { r.StoreId, r.TerminalCode });
        });

        modelBuilder.Entity<SalesInvoice>(entity =>
        {
            entity.ToTable("SalesInvoices");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.InvoiceNumber).HasMaxLength(50).IsRequired();
            entity.HasIndex(i => i.InvoiceNumber).IsUnique();
        });

        modelBuilder.Entity<SalesInvoiceItem>(entity =>
        {
            entity.ToTable("SalesInvoiceItems");
            entity.HasKey(t => t.Id);
        });

        modelBuilder.Entity<SalesOutboxMessage>(entity =>
        {
            entity.ToTable("SalesOutbox");
            entity.HasKey(o => o.Id);
        });
    }
}

public class SalesOutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
