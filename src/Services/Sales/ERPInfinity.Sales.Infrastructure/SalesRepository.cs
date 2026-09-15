using ERPInfinity.Sales.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERPInfinity.Sales.Infrastructure;

public interface ISalesRepository
{
    Task<Guid> OpenPOSRegisterAsync(Guid storeId, string terminalCode, Guid cashierId, decimal openingBalance, CancellationToken cancellationToken = default);
    Task<SalesInvoiceResult> CreateSalesInvoiceAsync(Guid registerId, Guid storeId, Guid? customerId, decimal subTotal, decimal discountAmount, decimal taxAmount, decimal totalAmount, string paymentMode, CancellationToken cancellationToken = default);
    Task<SalesInvoice?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}

public record SalesInvoiceResult(Guid InvoiceId, string InvoiceNumber);

public class SalesRepository : ISalesRepository
{
    private readonly SalesDbContext _dbContext;

    public SalesRepository(SalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> OpenPOSRegisterAsync(Guid storeId, string terminalCode, Guid cashierId, decimal openingBalance, CancellationToken cancellationToken = default)
    {
        var registerId = Guid.NewGuid();
        var register = new POSRegister
        {
            Id = registerId,
            StoreId = storeId,
            TerminalCode = terminalCode,
            CashierId = cashierId,
            OpeningBalance = openingBalance,
            OpenedAt = DateTime.UtcNow,
            Status = "OPEN"
        };

        await _dbContext.POSRegisters.AddAsync(register, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return registerId;
    }

    public async Task<SalesInvoiceResult> CreateSalesInvoiceAsync(Guid registerId, Guid storeId, Guid? customerId, decimal subTotal, decimal discountAmount, decimal taxAmount, decimal totalAmount, string paymentMode, CancellationToken cancellationToken = default)
    {
        var invoiceId = Guid.NewGuid();
        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";

        var invoice = new SalesInvoice
        {
            Id = invoiceId,
            InvoiceNumber = invoiceNumber,
            RegisterId = registerId,
            StoreId = storeId,
            CustomerId = customerId,
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            PaymentMode = paymentMode,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.SalesInvoices.AddAsync(invoice, cancellationToken);

        // Transactional Outbox Event for Analytics & Inventory Deductions
        var outboxMessage = new SalesOutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = "SalesInvoiceCreatedEvent",
            Payload = $"{{\"InvoiceId\":\"{invoiceId}\",\"InvoiceNumber\":\"{invoiceNumber}\",\"StoreId\":\"{storeId}\",\"TotalAmount\":{totalAmount}}}",
            CreatedAt = DateTime.UtcNow
        };
        await _dbContext.OutboxMessages.AddAsync(outboxMessage, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new SalesInvoiceResult(invoiceId, invoiceNumber);
    }

    public async Task<SalesInvoice?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SalesInvoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);
    }
}
