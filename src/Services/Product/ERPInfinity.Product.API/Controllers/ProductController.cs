using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPInfinity.Product.API.Controllers;

[ApiController]
[Route("api/v1/products")]
[Produces("application/json")]
public class ProductController : ControllerBase
{
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
    /// Retrieves summary status for Product & Catalog Service (Requires Scope / Permission Policy 'ProductRead').
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "ProductRead")]
    public IActionResult GetSummary()
    {
        return Ok(new
        {
            service = "Product & Catalog Service",
            policyRequired = "ProductRead",
            status = "Authorized microservice access granted.",
            timestamp = DateTime.UtcNow
        });
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
