using ERPInfinity.Inventory.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPInfinity.Inventory.API.Controllers;

[ApiController]
[Route("api/v1/inventory")]
[Produces("application/json")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryRepository _inventoryRepository;

    public InventoryController(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    /// <summary>
    /// Health check endpoint for Inventory Service.
    /// </summary>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "ERPInfinity.Inventory",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Deducts stock for a POS counter billing transaction.
    /// </summary>
    [HttpPost("pos-deduct")]
    [Authorize(Policy = "InventoryWrite")]
    public async Task<IActionResult> DeductPOSInventory([FromBody] POSDeductRequest request, CancellationToken cancellationToken)
    {
        await _inventoryRepository.DeductPOSInventoryAsync(request.LocationId, request.SKUId, request.Quantity, request.InvoiceNumber, cancellationToken);
        return Ok(new { message = "Stock successfully deducted for POS billing.", request.LocationId, request.SKUId, request.Quantity, request.InvoiceNumber });
    }

    /// <summary>
    /// Receives incoming stock from Goods Received Note (GRN).
    /// </summary>
    [HttpPost("grn-receive")]
    [Authorize(Policy = "InventoryWrite")]
    public async Task<IActionResult> ReceiveGRNStock([FromBody] GRNReceiveRequest request, CancellationToken cancellationToken)
    {
        await _inventoryRepository.ReceiveGRNStockAsync(request.LocationId, request.SKUId, request.ReceivedQuantity, request.GRNNumber, cancellationToken);
        return Ok(new { message = "Stock successfully received from GRN.", request.LocationId, request.SKUId, request.ReceivedQuantity, request.GRNNumber });
    }

    /// <summary>
    /// Retrieves current stock balance for a Location and SKU.
    /// </summary>
    [HttpGet("stock/{locationId:guid}/{skuId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStock(Guid locationId, Guid skuId, CancellationToken cancellationToken)
    {
        var stock = await _inventoryRepository.GetStockAsync(locationId, skuId, cancellationToken);
        if (stock == null)
        {
            return NotFound(new { message = $"No stock record found for Location '{locationId}' and SKU '{skuId}'." });
        }
        return Ok(stock);
    }
}

public record POSDeductRequest(Guid LocationId, Guid SKUId, decimal Quantity, string InvoiceNumber);
public record GRNReceiveRequest(Guid LocationId, Guid SKUId, decimal ReceivedQuantity, string GRNNumber);
