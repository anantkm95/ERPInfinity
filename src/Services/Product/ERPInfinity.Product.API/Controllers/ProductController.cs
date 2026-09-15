using ERPInfinity.Product.Domain;
using ERPInfinity.Product.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPInfinity.Product.API.Controllers;

[ApiController]
[Route("api/v1/products")]
[Produces("application/json")]
public class ProductController : ControllerBase
{
    private readonly IProductRepository _productRepository;

    public ProductController(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    /// <summary>
    /// Health check endpoint for Product & Catalog Service.
    /// </summary>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "ERPInfinity.Product",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// High-speed barcode scan lookup for POS terminals (<2ms SP execution).
    /// </summary>
    [HttpGet("barcode/{code}")]
    [AllowAnonymous]
    public async Task<IActionResult> LookupByBarcode(string code, CancellationToken cancellationToken)
    {
        var item = await _productRepository.LookupByBarcodeAsync(code, cancellationToken);
        if (item == null)
        {
            return NotFound(new { message = $"No active product found for barcode '{code}'." });
        }
        return Ok(item);
    }

    /// <summary>
    /// Retrieves full product details by ProductId.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            return NotFound(new { message = $"Product '{id}' not found." });
        }
        return Ok(product);
    }

    /// <summary>
    /// Creates a new Product Master with SKUs and Barcodes.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "ProductWrite")]
    public async Task<IActionResult> CreateProduct([FromBody] Domain.Product product, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(product.Name) || string.IsNullOrWhiteSpace(product.ProductCode))
        {
            return BadRequest(new { message = "ProductCode and Name are required." });
        }

        await _productRepository.CreateProductAsync(product, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>
    /// Updates SKU MRP and Selling Price and emits Outbox integration event.
    /// </summary>
    [HttpPut("skus/{skuId:guid}/price")]
    [Authorize(Policy = "ProductWrite")]
    public async Task<IActionResult> UpdateSKUPricing(Guid skuId, [FromBody] UpdatePriceRequest request, CancellationToken cancellationToken)
    {
        await _productRepository.UpdateSKUPricingAsync(skuId, request.NewMRP, request.NewSellingPrice, request.UpdatedBy, cancellationToken);
        return Ok(new { message = "SKU pricing updated and Outbox event emitted.", skuId, request.NewMRP, request.NewSellingPrice });
    }

    /// <summary>
    /// Internal machine-to-machine endpoint for inter-microservice communication.
    /// </summary>
    [HttpPost("internal-sync")]
    [Authorize(Policy = "InternalServiceOnly")]
    public IActionResult InternalSync([FromBody] object payload)
    {
        return Ok(new
        {
            service = "Product & Catalog Service",
            message = "Scope-protected inter-microservice machine-to-machine communication successful.",
            timestamp = DateTime.UtcNow
        });
    }
}

public record UpdatePriceRequest(decimal NewMRP, decimal NewSellingPrice, Guid UpdatedBy);
