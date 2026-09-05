using ERPInfinity.BuildingBlocks.CQRS;
using ERPInfinity.Identity.Application.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace ERPInfinity.Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Auth Handlers
        services.AddScoped<IQueryHandler<AuthenticateUserQuery, AuthResponseDto>, AuthenticateUserQueryHandler>();
        services.AddScoped<ICommandHandler<RegisterUserCommand, Guid>, RegisterUserCommandHandler>();
        services.AddScoped<ICommandHandler<RefreshTokenCommand, AuthResponseDto>, RefreshTokenCommandHandler>();

        // User Handlers
        services.AddScoped<IQueryHandler<GetUserByIdQuery, UserDto>, GetUserByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetAllUsersQuery, List<UserDto>>, GetAllUsersQueryHandler>();
        services.AddScoped<IQueryHandler<GetUserByUsernameOrEmailQuery, UserDto>, GetUserByUsernameOrEmailQueryHandler>();
        services.AddScoped<ICommandHandler<AssignUserRoleCommand, bool>, AssignUserRoleCommandHandler>();
        services.AddScoped<ICommandHandler<ChangePasswordCommand, bool>, ChangePasswordCommandHandler>();
        services.AddScoped<ICommandHandler<ResetPasswordByIdentifierCommand, ResetPasswordResponseDto>, ResetPasswordByIdentifierCommandHandler>();


        // Role & Permission Handlers
        services.AddScoped<IQueryHandler<GetAllRolesQuery, List<RoleDto>>, GetAllRolesQueryHandler>();
        services.AddScoped<IQueryHandler<GetUserPermissionsQuery, List<string>>, GetUserPermissionsQueryHandler>();

        return services;
    }
}
