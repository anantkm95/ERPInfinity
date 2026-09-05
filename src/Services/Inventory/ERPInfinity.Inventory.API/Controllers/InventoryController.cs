using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPInfinity.Inventory.API.Controllers;

[ApiController]
[Route("api/v1/inventory")]
[Produces("application/json")]
public class InventoryController : ControllerBase
{
    /// <summary>
    /// Health check endpoint for Inventory & Stock Movement Service.
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
    /// Retrieves summary status for Inventory & Stock Movement Service (Requires Scope / Permission Policy 'InventoryRead').
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "InventoryRead")]
    public IActionResult GetSummary()
    {
        return Ok(new
        {
            service = "Inventory & Stock Movement Service",
            policyRequired = "InventoryRead",
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
            service = "Inventory & Stock Movement Service",
            message = "Scope-protected inter-microservice machine-to-machine communication successful.",
            timestamp = DateTime.UtcNow
        });
    }
}
