using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPInfinity.Finance.API.Controllers;

[ApiController]
[Route("api/v1/finance")]
[Produces("application/json")]
public class FinanceController : ControllerBase
{
    /// <summary>
    /// Health check endpoint for Finance & General Ledger Service.
    /// </summary>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "ERPInfinity.Finance",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Retrieves summary status for Finance & General Ledger Service (Requires Scope / Permission Policy 'FinanceView').
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "FinanceView")]
    public IActionResult GetSummary()
    {
        return Ok(new
        {
            service = "Finance & General Ledger Service",
            policyRequired = "FinanceView",
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
            service = "Finance & General Ledger Service",
            message = "Scope-protected inter-microservice machine-to-machine communication successful.",
            timestamp = DateTime.UtcNow
        });
    }
}
