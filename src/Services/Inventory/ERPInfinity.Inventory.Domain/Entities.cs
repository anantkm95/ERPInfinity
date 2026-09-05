namespace ERPInfinity.Inventory.Domain;

public class Stock
{
    public long Id { get; set; }
    public Guid LocationId { get; set; }
    public string LocationType { get; set; } = "Store";
    public Guid SKUId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity => QuantityOnHand - ReservedQuantity;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class StockTransaction
{
    public long Id { get; set; }
    public Guid LocationId { get; set; }
    public Guid SKUId { get; set; }
    public string TransactionType { get; set; } = "POSSale";
    public decimal Quantity { get; set; }
    public string ReferenceId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class StockAdjustment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LocationId { get; set; }
    public Guid SKUId { get; set; }
    public decimal AdjustmentQuantity { get; set; }
    public string ReasonCode { get; set; } = string.Empty;
    public Guid ApprovedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
