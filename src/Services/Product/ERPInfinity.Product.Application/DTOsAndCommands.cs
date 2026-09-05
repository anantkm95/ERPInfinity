using ERPInfinity.BuildingBlocks.CQRS;

namespace ERPInfinity.Product.Application;

public record LookupProductByBarcodeQuery(string BarcodeNumber) : IQuery<ProductLookupResponse>;

public record CreateProductCommand(
    string ProductCode,
    string Name,
    int BrandId,
    int CategoryId,
    string HSNCode,
    decimal TaxPercentage,
    string SKUCode,
    decimal MRP,
    decimal SellingPrice,
    string BarcodeNumber
) : ICommand<Guid>;

public record ProductLookupResponse(
    Guid ProductId,
    Guid SKUId,
    string ProductCode,
    string SKUCode,
    string Name,
    string BarcodeNumber,
    decimal MRP,
    decimal SellingPrice,
    decimal TaxPercentage,
    string HSNCode
);

public class LookupProductByBarcodeQueryHandler : IQueryHandler<LookupProductByBarcodeQuery, ProductLookupResponse>
{
    public Task<Result<ProductLookupResponse>> Handle(LookupProductByBarcodeQuery query, CancellationToken cancellationToken = default)
    {
        // Mock sub-2ms barcode lookup logic
        if (query.BarcodeNumber == "8901234567890" || query.BarcodeNumber == "8901001001001")
        {
            var response = new ProductLookupResponse(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "PRD-TATA-SALT",
                "TATA-SALT-1KG",
                "Tata Salt 1kg",
                query.BarcodeNumber,
                30.00m,
                27.00m,
                5.0m,
                "2501"
            );
            return Task.FromResult(Result<ProductLookupResponse>.Success(response));
        }

        return Task.FromResult(Result<ProductLookupResponse>.Failure($"Product with barcode '{query.BarcodeNumber}' not found."));
    }
}
