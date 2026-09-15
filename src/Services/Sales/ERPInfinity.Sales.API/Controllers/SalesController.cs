using ERPInfinity.Sales.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPInfinity.Sales.API.Controllers;

[ApiController]
[Route("api/v1/sales")]
[Produces("application/json")]
public class SalesController : ControllerBase
{
    private readonly ISalesRepository _salesRepository;

    public SalesController(ISalesRepository salesRepository)
    {
        _salesRepository = salesRepository;
    }

    /// <summary>
    /// Health check endpoint for Sales & POS Billing Service.
    /// </summary>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "ERPInfinity.Sales",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Opens a new POS Register session for a terminal and cashier.
    /// </summary>
    [HttpPost("registers/open")]
    [Authorize(Policy = "SalesWrite")]
    public async Task<IActionResult> OpenRegister([FromBody] OpenRegisterRequest request, CancellationToken cancellationToken)
    {
        var registerId = await _salesRepository.OpenPOSRegisterAsync(request.StoreId, request.TerminalCode, request.CashierId, request.OpeningBalance, cancellationToken);
        return Ok(new { message = "POS Register session opened successfully.", registerId, request.StoreId, request.TerminalCode });
    }

    /// <summary>
    /// Creates a new Sales Invoice for POS checkout counter billing.
    /// </summary>
    [HttpPost("invoices")]
    [Authorize(Policy = "SalesWrite")]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var result = await _salesRepository.CreateSalesInvoiceAsync(
            request.RegisterId,
            request.StoreId,
            request.CustomerId,
            request.SubTotal,
            request.DiscountAmount,
            request.TaxAmount,
            request.TotalAmount,
            request.PaymentMode,
            cancellationToken);

        return Ok(new
        {
            message = "Sales Invoice generated and Outbox event published.",
            invoiceId = result.InvoiceId,
            invoiceNumber = result.InvoiceNumber,
            totalAmount = request.TotalAmount
        });
    }

    /// <summary>
    /// Retrieves Invoice details by InvoiceId.
    /// </summary>
    [HttpGet("invoices/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetInvoice(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await _salesRepository.GetInvoiceByIdAsync(id, cancellationToken);
        if (invoice == null)
        {
            return NotFound(new { message = $"Invoice '{id}' not found." });
        }
        return Ok(invoice);
    }
}

public record OpenRegisterRequest(Guid StoreId, string TerminalCode, Guid CashierId, decimal OpeningBalance);
public record CreateInvoiceRequest(Guid RegisterId, Guid StoreId, Guid? CustomerId, decimal SubTotal, decimal DiscountAmount, decimal TaxAmount, decimal TotalAmount, string PaymentMode);
