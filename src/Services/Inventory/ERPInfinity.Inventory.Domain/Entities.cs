namespace ERPInfinity.Inventory.Domain;

public class Location
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string LocationCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Warehouse";
    public bool IsActive { get; set; } = true;
}

public class Stock
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LocationId { get; set; }
    public Guid SKUId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity => QuantityOnHand - ReservedQuantity;
    public decimal ReorderPoint { get; set; } = 10.000m;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class StockTransaction
{
    public long Id { get; set; }
    public Guid LocationId { get; set; }
    public Guid SKUId { get; set; }
    public string TransactionType { get; set; } = "POS_DEDUCTION";
    public decimal QuantityChange { get; set; }
    public decimal PreviousQuantity { get; set; }
    public decimal NewQuantity { get; set; }
    public string? ReferenceDocumentNumber { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class StockAdjustment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LocationId { get; set; }
    public Guid SKUId { get; set; }
    public string AdjustmentReason { get; set; } = string.Empty;
    public decimal QuantityAdjusted { get; set; }
    public Guid ApprovedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
