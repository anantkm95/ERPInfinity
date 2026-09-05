using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPInfinity.Reporting.API.Controllers;

[ApiController]
[Route("api/v1/reporting")]
[Produces("application/json")]
public class ReportingController : ControllerBase
{
    /// <summary>
    /// Health check endpoint for Analytics & Reporting Service.
    /// </summary>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "ERPInfinity.Reporting",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Retrieves summary status for Analytics & Reporting Service (Requires Scope / Permission Policy 'InternalServiceOnly').
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "InternalServiceOnly")]
    public IActionResult GetSummary()
    {
        return Ok(new
        {
            service = "Analytics & Reporting Service",
            policyRequired = "InternalServiceOnly",
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
            service = "Analytics & Reporting Service",
            message = "Scope-protected inter-microservice machine-to-machine communication successful.",
            timestamp = DateTime.UtcNow
        });
    }
}
