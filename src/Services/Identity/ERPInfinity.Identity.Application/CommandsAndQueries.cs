using ERPInfinity.BuildingBlocks.CQRS;

namespace ERPInfinity.Identity.Application;

// Queries
public record AuthenticateUserQuery(string Username, string Password, string IpAddress) : IQuery<AuthResponseDto>;

public record GetUserByIdQuery(Guid UserId) : IQuery<UserDto>;

public record GetAllUsersQuery(bool OnlyActive = false) : IQuery<List<UserDto>>;

public record GetAllRolesQuery() : IQuery<List<RoleDto>>;

public record GetUserPermissionsQuery(Guid UserId) : IQuery<List<string>>;

public record GetUserByUsernameOrEmailQuery(string Identifier) : IQuery<UserDto>;

// Commands
public record RegisterUserCommand(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PhoneNumber,
    List<int> RoleIds
) : ICommand<Guid>;

public record RefreshTokenCommand(
    string Token,
    string RefreshToken,
    string IpAddress
) : ICommand<AuthResponseDto>;

public record RevokeTokenCommand(
    string RefreshToken,
    string IpAddress
) : ICommand<bool>;

public record AssignUserRoleCommand(
    Guid UserId,
    List<int> RoleIds
) : ICommand<bool>;

public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : ICommand<bool>;

public record ResetPasswordByIdentifierCommand(
    string UsernameOrEmail,
    string? NewPassword = null
) : ICommand<ResetPasswordResponseDto>;

