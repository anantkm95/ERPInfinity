namespace ERPInfinity.Sales.Domain;

public class POSRegister
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StoreId { get; set; }
    public string TerminalCode { get; set; } = string.Empty;
    public Guid CashierId { get; set; }
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal? ClosingCashCollected { get; set; }
    public decimal? ClosingCardCollected { get; set; }
    public decimal? ClosingUPICollected { get; set; }
    public string Status { get; set; } = "OPEN";
}

public class SalesInvoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid RegisterId { get; set; }
    public Guid StoreId { get; set; }
    public Guid? CustomerId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMode { get; set; } = "CASH";
    public string Status { get; set; } = "COMPLETED";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<SalesInvoiceItem> Items { get; set; } = new();
}

public class SalesInvoiceItem
{
    public long Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid SKUId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal LineTotal { get; set; }
}

public class SalesReturn
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ReturnNumber { get; set; } = string.Empty;
    public Guid OriginalInvoiceId { get; set; }
    public decimal RefundAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid ProcessedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
