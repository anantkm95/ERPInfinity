using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ERPInfinity.Identity.Application.Abstractions;
using ERPInfinity.Identity.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ERPInfinity.Identity.Infrastructure.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiresAt) GenerateAccessToken(User user, List<string> roles, List<string> permissions)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"] ?? "ERPInfinityEnterpriseSuperSecretSecurityKey2026!#$";
        var issuer = _configuration["JwtSettings:Issuer"] ?? "ERPInfinity.Identity";
        var audience = _configuration["JwtSettings:Audience"] ?? "ERPInfinity.Services";
        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("name", $"{user.FirstName} {user.LastName}".Trim()),
            new("scope", "erpinfinity.user")
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
            claims.Add(new Claim("scope", PermissionToScope(permission)));
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expiresAt);
    }

    public (string Token, DateTime ExpiresAt) GenerateServiceToken(string serviceName, List<string> scopes)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"] ?? "ERPInfinityEnterpriseSuperSecretSecurityKey2026!#$";
        var issuer = _configuration["JwtSettings:Issuer"] ?? "ERPInfinity.Identity";
        var audience = _configuration["JwtSettings:Audience"] ?? "ERPInfinity.Services";
        var expiryMinutes = int.Parse(_configuration["JwtSettings:ServiceTokenExpiryMinutes"] ?? "1440"); // 24 hours default for M2M

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, serviceName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("client_id", serviceName),
            new("grant_type", "client_credentials"),
            new("scope", "erpinfinity.internal"),
            new("scope", "microservice.m2m")
        };

        foreach (var scope in scopes)
        {
            claims.Add(new Claim("scope", scope));
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expiresAt);
    }

    public RefreshToken GenerateRefreshToken(Guid userId, string ipAddress)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(randomBytes);
        var expiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
    }

    private static string PermissionToScope(string permission)
    {
        return permission.ToLowerInvariant() switch
        {
            "product.view" => "product.read",
            "product.create" => "product.write",
            "inventory.view" => "inventory.read",
            "inventory.adjust" => "inventory.adjust",
            "sales.create" => "sales.create",
            "finance.view" => "finance.read",
            "user.manage" => "identity.manage",
            "role.manage" => "identity.manage",
            _ => permission.ToLowerInvariant().Replace(".", ":")
        };
    }
}
