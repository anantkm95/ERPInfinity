namespace ERPInfinity.Identity.Application;

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    List<string> Roles,
    List<string> Permissions
);

public record RoleDto(
    int Id,
    string Name,
    string Description,
    bool IsSystemRole,
    List<string> Permissions
);

public record PermissionDto(
    int Id,
    string PermissionCode,
    string Module,
    string Description
);

public record AuthResponseDto(
    Guid UserId,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    List<string> Roles,
    List<string> Permissions
);

public record RefreshTokenRequestDto(
    string Token,
    string RefreshToken
);

public record ChangePasswordRequestDto(
    string CurrentPassword,
    string NewPassword
);

public record AssignRoleRequestDto(
    List<int> RoleIds
);

public record ResetPasswordResponseDto(
    Guid UserId,
    string Username,
    string Email,
    string NewPassword,
    string Message
);

public record ResetPasswordRequestDto(
    string UsernameOrEmail,
    string? NewPassword
);

