using ERPInfinity.BuildingBlocks.CQRS;
using ERPInfinity.Identity.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ERPInfinity.Identity.Application.Handlers;

public class GetAllRolesQueryHandler : IQueryHandler<GetAllRolesQuery, List<RoleDto>>
{
    private readonly IIdentityDbContext _dbContext;

    public GetAllRolesQueryHandler(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<RoleDto>>> Handle(GetAllRolesQuery query, CancellationToken cancellationToken = default)
    {
        var roles = await _dbContext.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var dtos = roles.Select(role => new RoleDto(
            role.Id,
            role.Name,
            role.Description,
            role.IsSystemRole,
            role.RolePermissions.Select(rp => rp.Permission?.PermissionCode ?? "").Where(p => !string.IsNullOrEmpty(p)).ToList()
        )).ToList();

        return Result<List<RoleDto>>.Success(dtos);
    }
}

public class GetUserPermissionsQueryHandler : IQueryHandler<GetUserPermissionsQuery, List<string>>
{
    private readonly IIdentityDbContext _dbContext;

    public GetUserPermissionsQueryHandler(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<string>>> Handle(GetUserPermissionsQuery query, CancellationToken cancellationToken = default)
    {
        var userRoles = await _dbContext.UserRoles
            .Where(ur => ur.UserId == query.UserId)
            .Include(ur => ur.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var permissions = userRoles
            .SelectMany(ur => ur.Role?.RolePermissions ?? new List<Domain.RolePermission>())
            .Select(rp => rp.Permission?.PermissionCode ?? "")
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct()
            .ToList();

        return Result<List<string>>.Success(permissions);
    }
}
