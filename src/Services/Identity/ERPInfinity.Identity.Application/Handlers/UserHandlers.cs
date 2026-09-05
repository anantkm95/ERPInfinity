using ERPInfinity.BuildingBlocks.CQRS;
using ERPInfinity.Identity.Application.Abstractions;
using ERPInfinity.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERPInfinity.Identity.Application.Handlers;

public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    private readonly IIdentityDbContext _dbContext;

    public GetUserByIdQueryHandler(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r!.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == query.UserId, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure("User not found.");
        }

        var roles = user.UserRoles.Select(ur => ur.Role?.Name ?? "").Where(r => !string.IsNullOrEmpty(r)).Distinct().ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role?.RolePermissions ?? new List<RolePermission>())
            .Select(rp => rp.Permission?.PermissionCode ?? "")
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct()
            .ToList();

        var dto = new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt,
            roles,
            permissions
        );

        return Result<UserDto>.Success(dto);
    }
}

public class GetAllUsersQueryHandler : IQueryHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly IIdentityDbContext _dbContext;

    public GetAllUsersQueryHandler(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<UserDto>>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken = default)
    {
        var dbQuery = _dbContext.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r!.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .AsNoTracking();

        if (query.OnlyActive)
        {
            dbQuery = dbQuery.Where(u => u.IsActive);
        }

        var users = await dbQuery.ToListAsync(cancellationToken);

        var dtos = users.Select(user =>
        {
            var roles = user.UserRoles.Select(ur => ur.Role?.Name ?? "").Where(r => !string.IsNullOrEmpty(r)).Distinct().ToList();
            var permissions = user.UserRoles
                .SelectMany(ur => ur.Role?.RolePermissions ?? new List<RolePermission>())
                .Select(rp => rp.Permission?.PermissionCode ?? "")
                .Where(p => !string.IsNullOrEmpty(p))
                .Distinct()
                .ToList();

            return new UserDto(
                user.Id,
                user.Username,
                user.Email,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.IsActive,
                user.CreatedAt,
                user.LastLoginAt,
                roles,
                permissions
            );
        }).ToList();

        return Result<List<UserDto>>.Success(dtos);
    }
}

public class AssignUserRoleCommandHandler : ICommandHandler<AssignUserRoleCommand, bool>
{
    private readonly IIdentityDbContext _dbContext;

    public AssignUserRoleCommandHandler(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(AssignUserRoleCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user == null)
        {
            return Result<bool>.Failure("User not found.");
        }

        // Remove existing roles
        _dbContext.UserRoles.RemoveRange(user.UserRoles);

        // Add new roles
        foreach (var roleId in command.RoleIds)
        {
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId, AssignedAt = DateTime.UtcNow });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}

public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, bool>
{
    private readonly IIdentityDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(IIdentityDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<bool>> Handle(ChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);
        if (user == null)
        {
            return Result<bool>.Failure("User not found.");
        }

        if (!_passwordHasher.VerifyPassword(command.CurrentPassword, user.PasswordHash))
        {
            return Result<bool>.Failure("Current password is incorrect.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(command.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}

public class GetUserByUsernameOrEmailQueryHandler : IQueryHandler<GetUserByUsernameOrEmailQuery, UserDto>
{
    private readonly IIdentityDbContext _dbContext;

    public GetUserByUsernameOrEmailQueryHandler(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<UserDto>> Handle(GetUserByUsernameOrEmailQuery query, CancellationToken cancellationToken = default)
    {
        var identifier = query.Identifier.Trim();
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r!.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Username == identifier || u.Email == identifier, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure($"User with username or email '{query.Identifier}' not found.");
        }

        var roles = user.UserRoles.Select(ur => ur.Role?.Name ?? "").Where(r => !string.IsNullOrEmpty(r)).Distinct().ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role?.RolePermissions ?? new List<RolePermission>())
            .Select(rp => rp.Permission?.PermissionCode ?? "")
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct()
            .ToList();

        var dto = new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt,
            roles,
            permissions
        );

        return Result<UserDto>.Success(dto);
    }
}

public class ResetPasswordByIdentifierCommandHandler : ICommandHandler<ResetPasswordByIdentifierCommand, ResetPasswordResponseDto>
{
    private readonly IIdentityDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordByIdentifierCommandHandler(IIdentityDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<ResetPasswordResponseDto>> Handle(ResetPasswordByIdentifierCommand command, CancellationToken cancellationToken = default)
    {
        var identifier = command.UsernameOrEmail.Trim();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == identifier || u.Email == identifier, cancellationToken);

        if (user == null)
        {
            return Result<ResetPasswordResponseDto>.Failure($"User with username or email '{command.UsernameOrEmail}' not found.");
        }

        string newPassword = string.IsNullOrWhiteSpace(command.NewPassword)
            ? GenerateSecureRandomPassword()
            : command.NewPassword.Trim();

        user.PasswordHash = _passwordHasher.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Username = user.Username,
            Action = "ResetPassword",
            IpAddress = "127.0.0.1",
            Details = $"Password reset performed for user {user.Username} ({user.Email}).",
            Timestamp = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new ResetPasswordResponseDto(
            user.Id,
            user.Username,
            user.Email,
            newPassword,
            $"Password has been successfully updated for user '{user.Username}'."
        );

        return Result<ResetPasswordResponseDto>.Success(response);
    }

    private static string GenerateSecureRandomPassword(int length = 12)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
        var bytes = new byte[length];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
    }
}

