using System.Security.Claims;
using ERPInfinity.BuildingBlocks.CQRS;
using ERPInfinity.Identity.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPInfinity.Identity.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IQueryHandler<GetAllUsersQuery, List<UserDto>> _getAllUsersHandler;
    private readonly IQueryHandler<GetUserByIdQuery, UserDto> _getUserByIdHandler;
    private readonly ICommandHandler<AssignUserRoleCommand, bool> _assignRoleHandler;
    private readonly ICommandHandler<ChangePasswordCommand, bool> _changePasswordHandler;

    public UsersController(
        IQueryHandler<GetAllUsersQuery, List<UserDto>> getAllUsersHandler,
        IQueryHandler<GetUserByIdQuery, UserDto> getUserByIdHandler,
        ICommandHandler<AssignUserRoleCommand, bool> assignRoleHandler,
        ICommandHandler<ChangePasswordCommand, bool> changePasswordHandler)
    {
        _getAllUsersHandler = getAllUsersHandler;
        _getUserByIdHandler = getUserByIdHandler;
        _assignRoleHandler = assignRoleHandler;
        _changePasswordHandler = changePasswordHandler;
    }

    /// <summary>
    /// Gets list of all system users.
    /// </summary>
    /// <param name="onlyActive">Filter only active users</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = false)
    {
        var result = await _getAllUsersHandler.Handle(new GetAllUsersQuery(onlyActive));
        return Ok(result.Value);
    }

    /// <summary>
    /// Gets detailed profile of a specific user by ID.
    /// </summary>
    /// <param name="id">User Guid</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var result = await _getUserByIdHandler.Handle(new GetUserByIdQuery(id));
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Assigns or updates roles assigned to a user.
    /// </summary>
    /// <param name="id">User Guid</param>
    /// <param name="request">Role IDs list</param>
    [HttpPost("{id:guid}/roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignRoles([FromRoute] Guid id, [FromBody] AssignRoleRequestDto request)
    {
        var command = new AssignUserRoleCommand(id, request.RoleIds);
        var result = await _assignRoleHandler.Handle(command);
        if (result.IsSuccess)
        {
            return Ok(new { message = "User roles updated successfully." });
        }
        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Changes current authenticated user's password.
    /// </summary>
    /// <param name="request">Current and New Password</param>
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid user token credentials." });
        }

        var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
        var result = await _changePasswordHandler.Handle(command);
        if (result.IsSuccess)
        {
            return Ok(new { message = "Password updated successfully." });
        }
        return BadRequest(new { error = result.Error });
    }
}
