namespace ERPInfinity.Store.Domain;

public class Store
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string StoreCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<StoreTerminal> Terminals { get; set; } = new();
}

public class StoreTerminal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StoreId { get; set; }
    public string TerminalCode { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}

public class StoreUser
{
    public Guid StoreId { get; set; }
    public Guid UserId { get; set; }
    public string RoleInStore { get; set; } = "Cashier";
}
