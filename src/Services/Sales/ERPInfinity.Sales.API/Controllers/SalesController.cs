using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPInfinity.Sales.API.Controllers;

[ApiController]
[Route("api/v1/sales")]
[Produces("application/json")]
public class SalesController : ControllerBase
{
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
    /// Retrieves summary status for Sales & POS Billing Service (Requires Scope / Permission Policy 'SalesCreate').
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "SalesCreate")]
    public IActionResult GetSummary()
    {
        return Ok(new
        {
            service = "Sales & POS Billing Service",
            policyRequired = "SalesCreate",
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
            service = "Sales & POS Billing Service",
            message = "Scope-protected inter-microservice machine-to-machine communication successful.",
            timestamp = DateTime.UtcNow
        });
    }
}
