using System.Security.Claims;
using ERPInfinity.BuildingBlocks.CQRS;
using ERPInfinity.Identity.Application;
using ERPInfinity.Identity.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPInfinity.Identity.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IQueryHandler<AuthenticateUserQuery, AuthResponseDto> _loginHandler;
    private readonly ICommandHandler<RegisterUserCommand, Guid> _registerHandler;
    private readonly ICommandHandler<RefreshTokenCommand, AuthResponseDto> _refreshTokenHandler;
    private readonly IQueryHandler<GetUserByIdQuery, UserDto> _getUserByIdHandler;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IConfiguration _configuration;

    public AuthController(
        IQueryHandler<AuthenticateUserQuery, AuthResponseDto> loginHandler,
        ICommandHandler<RegisterUserCommand, Guid> registerHandler,
        ICommandHandler<RefreshTokenCommand, AuthResponseDto> refreshTokenHandler,
        IQueryHandler<GetUserByIdQuery, UserDto> getUserByIdHandler,
        IJwtTokenGenerator jwtTokenGenerator,
        IConfiguration configuration)
    {
        _loginHandler = loginHandler;
        _registerHandler = registerHandler;
        _refreshTokenHandler = refreshTokenHandler;
        _getUserByIdHandler = getUserByIdHandler;
        _jwtTokenGenerator = jwtTokenGenerator;
        _configuration = configuration;
    }

    /// <summary>
    /// Authenticates a user with username/email and password.
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT Access Token, Refresh Token, User Profile, Roles, and Permissions</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var query = new AuthenticateUserQuery(request.Username, request.Password, ipAddress);

        var result = await _loginHandler.Handle(query);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return Unauthorized(new { error = result.Error });
    }

    /// <summary>
    /// Registers a new user with optional initial role assignments.
    /// </summary>
    /// <param name="command">User registration details</param>
    /// <returns>Guid ID of created user</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var result = await _registerHandler.Handle(command);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(Register), new { id = result.Value }, new { userId = result.Value });
        }

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Generates a Machine-to-Machine (M2M) Scope-Protected JWT for inter-microservice communication.
    /// </summary>
    /// <param name="request">Service credentials and requested scopes</param>
    [HttpPost("service-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GenerateServiceToken([FromBody] ServiceTokenRequest request)
    {
        var expectedSecret = _configuration["JwtSettings:ServiceSecret"] ?? "ERPInfinityServiceToServiceInternalSecret2026!";
        if (request.ServiceSecret != expectedSecret)
        {
            return Unauthorized(new { error = "Invalid service communication secret." });
        }

        var (token, expiresAt) = _jwtTokenGenerator.GenerateServiceToken(request.ServiceName, request.Scopes ?? new List<string>());
        return Ok(new
        {
            token,
            expiresAt,
            tokenType = "Bearer",
            serviceName = request.ServiceName,
            scopes = request.Scopes
        });
    }

    /// <summary>
    /// Exchanges an expired JWT Access Token and valid Refresh Token for a new pair.
    /// </summary>
    /// <param name="request">Refresh Token request</param>
    /// <returns>New JWT Access Token and Refresh Token</returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var command = new RefreshTokenCommand(request.Token, request.RefreshToken, ipAddress);

        var result = await _refreshTokenHandler.Handle(command);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Retrieves full details of currently authenticated user from Bearer Token.
    /// </summary>
    /// <returns>User Profile, Roles, and Permissions</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid claims in token." });
        }

        var result = await _getUserByIdHandler.Handle(new GetUserByIdQuery(userId));
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Retrieves user profile details by Username or Email address.
    /// </summary>
    /// <param name="identifier">Username or Email address</param>
    /// <param name="handler">Query Handler</param>
    [HttpGet("user-by-identifier")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserByIdentifier([FromQuery] string identifier, [FromServices] IQueryHandler<GetUserByUsernameOrEmailQuery, UserDto> handler)
    {
        var result = await handler.Handle(new GetUserByUsernameOrEmailQuery(identifier));
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Resets or generates a new password for a user using their Username or Email address.
    /// </summary>
    /// <param name="request">Username or Email address and optional New Password</param>
    /// <param name="handler">Command Handler</param>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ResetPasswordResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request, [FromServices] ICommandHandler<ResetPasswordByIdentifierCommand, ResetPasswordResponseDto> handler)
    {
        var result = await handler.Handle(new ResetPasswordByIdentifierCommand(request.UsernameOrEmail, request.NewPassword));
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(new { error = result.Error });
    }
}

public record LoginRequest(string Username, string Password);

public record ServiceTokenRequest(string ServiceName, string ServiceSecret, List<string> Scopes);

