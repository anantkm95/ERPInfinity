using ERPInfinity.BuildingBlocks.CQRS;

namespace ERPInfinity.Inventory.Application;

public record DeductPOSInventoryCommand(Guid StoreId, Guid SKUId, decimal Quantity, string InvoiceNumber) : ICommand<StockOperationResponse>;

public record ReceiveGRNStockCommand(Guid WarehouseId, Guid SKUId, decimal Quantity, string GRNNumber) : ICommand<StockOperationResponse>;

public record StockOperationResponse(
    Guid LocationId,
    Guid SKUId,
    decimal PreviousQuantity,
    decimal NewQuantity,
    string TransactionType,
    string ReferenceId
);

public class DeductPOSInventoryCommandHandler : ICommandHandler<DeductPOSInventoryCommand, StockOperationResponse>
{
    public Task<Result<StockOperationResponse>> Handle(DeductPOSInventoryCommand command, CancellationToken cancellationToken = default)
    {
        // Mock atomic inventory deduction
        var previousQty = 100.0m;
        var newQty = previousQty - command.Quantity;

        var response = new StockOperationResponse(
            command.StoreId,
            command.SKUId,
            previousQty,
            newQty,
            "POSSale",
            command.InvoiceNumber
        );

        return Task.FromResult(Result<StockOperationResponse>.Success(response));
    }
}
