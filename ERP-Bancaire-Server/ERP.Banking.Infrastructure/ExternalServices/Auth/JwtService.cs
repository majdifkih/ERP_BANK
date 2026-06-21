using ERP.Banking.Application.Interfaces.Auth;  
using ERP.Banking.Application.Settings;
using ERP.Banking.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ERP.Banking.Infrastructure.ExternalServices.Auth;

/// <summary>
/// Generates JWT access tokens and cryptographically random refresh tokens.
/// </summary>
internal sealed class JwtService : IJwtService
{
    private readonly JwtSettings _settings;

    public JwtService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public string GenerateAccessToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: BuildClaims(user),
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken() =>
        Convert.ToBase64String(
            System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));

    // ── Helpers ────────────────────────────────────────────────────

    private static IEnumerable<Claim> BuildClaims(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier,   user.Id.ToString()),
            new(ClaimTypes.Name,             user.Username),
            new(ClaimTypes.Email,            user.Email),
            new(ClaimTypes.Role,             user.Role?.Name ?? string.Empty)
        };

        if (user.Role?.RolePermissions is not null)
        {
            claims.AddRange(
                user.Role.RolePermissions
                    .Where(rp => rp.Permission is not null)
                    .Select(rp => new Claim("permission", rp.Permission.Code)));
        }

        return claims;
    }
}