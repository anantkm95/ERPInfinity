using ERPInfinity.Inventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERPInfinity.Inventory.Infrastructure;

public interface IInventoryRepository
{
    Task DeductPOSInventoryAsync(Guid locationId, Guid skuId, decimal quantity, string invoiceNumber, CancellationToken cancellationToken = default);
    Task ReceiveGRNStockAsync(Guid locationId, Guid skuId, decimal receivedQuantity, string grnNumber, CancellationToken cancellationToken = default);
    Task<Stock?> GetStockAsync(Guid locationId, Guid skuId, CancellationToken cancellationToken = default);
}

public class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _dbContext;

    public InventoryRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task DeductPOSInventoryAsync(Guid locationId, Guid skuId, decimal quantity, string invoiceNumber, CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.ExecuteSqlRawAsync(
            "EXEC dbo.sp_DeductInventoryForPOS @LocationId = {0}, @SKUId = {1}, @Quantity = {2}, @InvoiceNumber = {3}",
            locationId, skuId, quantity, invoiceNumber, cancellationToken);
    }

    public async Task ReceiveGRNStockAsync(Guid locationId, Guid skuId, decimal receivedQuantity, string grnNumber, CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.ExecuteSqlRawAsync(
            "EXEC dbo.sp_ReceiveGRNStock @LocationId = {0}, @SKUId = {1}, @ReceivedQuantity = {2}, @GRNNumber = {3}",
            locationId, skuId, receivedQuantity, grnNumber, cancellationToken);
    }

    public async Task<Stock?> GetStockAsync(Guid locationId, Guid skuId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Stocks
            .FirstOrDefaultAsync(s => s.LocationId == locationId && s.SKUId == skuId, cancellationToken);
    }
}
