using ERPInfinity.Identity.Domain;

namespace ERPInfinity.Identity.Application.Abstractions;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user, List<string> roles, List<string> permissions);
    (string Token, DateTime ExpiresAt) GenerateServiceToken(string serviceName, List<string> scopes);
    RefreshToken GenerateRefreshToken(Guid userId, string ipAddress);
}
