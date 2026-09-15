using ERPInfinity.Product.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERPInfinity.Product.Infrastructure;

public interface IProductRepository
{
    Task<BarcodeLookupDto?> LookupByBarcodeAsync(string barcodeNumber, CancellationToken cancellationToken = default);
    Task<Domain.Product?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task CreateProductAsync(Domain.Product product, CancellationToken cancellationToken = default);
    Task UpdateSKUPricingAsync(Guid skuId, decimal newMRP, decimal newSellingPrice, Guid updatedBy, CancellationToken cancellationToken = default);
}

public record BarcodeLookupDto(
    Guid ProductId,
    Guid SKUId,
    string SKUCode,
    string ProductName,
    decimal MRP,
    decimal SellingPrice,
    decimal TaxPercentage,
    string HSNCode,
    string BarcodeNumber
);

public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _dbContext;

    public ProductRepository(ProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BarcodeLookupDto?> LookupByBarcodeAsync(string barcodeNumber, CancellationToken cancellationToken = default)
    {
        // High speed SP execution for <2ms POS scanning
        var results = await _dbContext.Database
            .SqlQueryRaw<BarcodeLookupDto>(
                "EXEC dbo.sp_LookupProductByBarcode @BarcodeNumber = {0}", barcodeNumber)
            .ToListAsync(cancellationToken);

        return results.FirstOrDefault();
    }

    public async Task<Domain.Product?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(p => p.SKUs)
            .ThenInclude(s => s.Barcodes)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
    }

    public async Task CreateProductAsync(Domain.Product product, CancellationToken cancellationToken = default)
    {
        await _dbContext.Products.AddAsync(product, cancellationToken);

        // Transactional Outbox event
        var outboxMessage = new ProductOutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = "ProductCreatedEvent",
            Payload = $"{{\"ProductId\":\"{product.Id}\",\"ProductCode\":\"{product.ProductCode}\",\"Name\":\"{product.Name}\"}}",
            CreatedAt = DateTime.UtcNow
        };
        await _dbContext.OutboxMessages.AddAsync(outboxMessage, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateSKUPricingAsync(Guid skuId, decimal newMRP, decimal newSellingPrice, Guid updatedBy, CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.ExecuteSqlRawAsync(
            "EXEC dbo.sp_UpdateSKUPricing @SKUId = {0}, @NewMRP = {1}, @NewSellingPrice = {2}, @UpdatedBy = {3}",
            skuId, newMRP, newSellingPrice, updatedBy, cancellationToken);
    }
}
