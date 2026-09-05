using ERPInfinity.BuildingBlocks.CQRS;
using ERPInfinity.Identity.Application.Abstractions;
using ERPInfinity.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERPInfinity.Identity.Application.Handlers;

public class AuthenticateUserQueryHandler : IQueryHandler<AuthenticateUserQuery, AuthResponseDto>
{
    private readonly IIdentityDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthenticateUserQueryHandler(
        IIdentityDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<Result<AuthResponseDto>> Handle(AuthenticateUserQuery query, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r!.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Username == query.Username || u.Email == query.Username, cancellationToken);

        if (user == null || !user.IsActive)
        {
            return Result<AuthResponseDto>.Failure("Invalid username or password credentials.");
        }

        if (!_passwordHasher.VerifyPassword(query.Password, user.PasswordHash))
        {
            return Result<AuthResponseDto>.Failure("Invalid username or password credentials.");
        }

        var roles = user.UserRoles.Select(ur => ur.Role?.Name ?? "").Where(r => !string.IsNullOrEmpty(r)).Distinct().ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role?.RolePermissions ?? new List<RolePermission>())
            .Select(rp => rp.Permission?.PermissionCode ?? "")
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct()
            .ToList();

        var (accessToken, expiresAt) = _tokenGenerator.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _tokenGenerator.GenerateRefreshToken(user.Id, query.IpAddress);

        _dbContext.RefreshTokens.Add(refreshToken);
        user.LastLoginAt = DateTime.UtcNow;

        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Username = user.Username,
            Action = "LOGIN_SUCCESS",
            IpAddress = query.IpAddress,
            Details = $"User {user.Username} logged in successfully.",
            Timestamp = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new AuthResponseDto(
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            accessToken,
            refreshToken.Token,
            expiresAt,
            roles,
            permissions
        );

        return Result<AuthResponseDto>.Success(response);
    }
}

public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Guid>
{
    private readonly IIdentityDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(IIdentityDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        var existingUser = await _dbContext.Users
            .AnyAsync(u => u.Username == command.Username || u.Email == command.Email, cancellationToken);

        if (existingUser)
        {
            return Result<Guid>.Failure("Username or Email is already registered.");
        }

        var passwordHash = _passwordHasher.HashPassword(command.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = command.Username,
            Email = command.Email,
            PasswordHash = passwordHash,
            FirstName = command.FirstName,
            LastName = command.LastName,
            PhoneNumber = command.PhoneNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        if (command.RoleIds != null && command.RoleIds.Count != 0)
        {
            foreach (var roleId in command.RoleIds)
            {
                user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId, AssignedAt = DateTime.UtcNow });
            }
        }
        else
        {
            // Default role: Cashier (Role ID 3)
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = 3, AssignedAt = DateTime.UtcNow });
        }

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(user.Id);
    }
}

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IIdentityDbContext _dbContext;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public RefreshTokenCommandHandler(IIdentityDbContext dbContext, IJwtTokenGenerator tokenGenerator)
    {
        _dbContext = dbContext;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        var existingToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u!.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r!.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(rt => rt.Token == command.RefreshToken, cancellationToken);

        if (existingToken == null || !existingToken.IsActive || existingToken.User == null || !existingToken.User.IsActive)
        {
            return Result<AuthResponseDto>.Failure("Invalid or expired refresh token.");
        }

        // Revoke old refresh token & generate new pair
        existingToken.IsRevoked = true;
        existingToken.RevokedAt = DateTime.UtcNow;

        var user = existingToken.User;
        var roles = user.UserRoles.Select(ur => ur.Role?.Name ?? "").Where(r => !string.IsNullOrEmpty(r)).Distinct().ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role?.RolePermissions ?? new List<RolePermission>())
            .Select(rp => rp.Permission?.PermissionCode ?? "")
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct()
            .ToList();

        var (accessToken, expiresAt) = _tokenGenerator.GenerateAccessToken(user, roles, permissions);
        var newRefreshToken = _tokenGenerator.GenerateRefreshToken(user.Id, command.IpAddress);
        existingToken.ReplacedByToken = newRefreshToken.Token;

        _dbContext.RefreshTokens.Add(newRefreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new AuthResponseDto(
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            accessToken,
            newRefreshToken.Token,
            expiresAt,
            roles,
            permissions
        );

        return Result<AuthResponseDto>.Success(response);
    }
}
