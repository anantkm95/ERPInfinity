using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPInfinity.Notification.API.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Produces("application/json")]
public class NotificationController : ControllerBase
{
    /// <summary>
    /// Health check endpoint for Notification & Alerts Service.
    /// </summary>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "ERPInfinity.Notification",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Retrieves summary status for Notification & Alerts Service (Requires Scope / Permission Policy 'InternalServiceOnly').
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "InternalServiceOnly")]
    public IActionResult GetSummary()
    {
        return Ok(new
        {
            service = "Notification & Alerts Service",
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
            service = "Notification & Alerts Service",
            message = "Scope-protected inter-microservice machine-to-machine communication successful.",
            timestamp = DateTime.UtcNow
        });
    }
}
