using ERPInfinity.BuildingBlocks.CQRS;
using ERPInfinity.Identity.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPInfinity.Identity.API.Controllers;

[ApiController]
[Route("api/v1/roles")]
[Authorize]
[Produces("application/json")]
public class RolesController : ControllerBase
{
    private readonly IQueryHandler<GetAllRolesQuery, List<RoleDto>> _getAllRolesHandler;
    private readonly IQueryHandler<GetUserPermissionsQuery, List<string>> _getUserPermissionsHandler;

    public RolesController(
        IQueryHandler<GetAllRolesQuery, List<RoleDto>> getAllRolesHandler,
        IQueryHandler<GetUserPermissionsQuery, List<string>> getUserPermissionsHandler)
    {
        _getAllRolesHandler = getAllRolesHandler;
        _getUserPermissionsHandler = getUserPermissionsHandler;
    }

    /// <summary>
    /// Gets list of all system roles and their assigned permission codes.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _getAllRolesHandler.Handle(new GetAllRolesQuery());
        return Ok(result.Value);
    }

    /// <summary>
    /// Gets list of all permission codes assigned to a user across all their roles.
    /// </summary>
    /// <param name="userId">User Guid</param>
    [HttpGet("permissions/{userId:guid}")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserPermissions([FromRoute] Guid userId)
    {
        var result = await _getUserPermissionsHandler.Handle(new GetUserPermissionsQuery(userId));
        return Ok(result.Value);
    }
}
