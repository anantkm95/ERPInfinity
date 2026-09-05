namespace ERPInfinity.Product.Domain;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int BrandId { get; set; }
    public int CategoryId { get; set; }
    public string HSNCode { get; set; } = string.Empty;
    public decimal TaxPercentage { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<ProductSKU> SKUs { get; set; } = new();
}

public class Category
{
    public int Id { get; set; }
    public int? ParentCategoryId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class Brand
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class ProductSKU
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public string SKUCode { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = "PCS";
    public decimal PackSize { get; set; } = 1.0m;
    public decimal WeightKg { get; set; }
    public decimal MRP { get; set; }
    public decimal SellingPrice { get; set; }
    public List<Barcode> Barcodes { get; set; } = new();
}

public class Barcode
{
    public long Id { get; set; }
    public Guid SKUId { get; set; }
    public string BarcodeNumber { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = true;
}
