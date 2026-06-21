using ERP.Banking.Domain.Entities;

namespace ERP.Banking.Application.Interfaces.Auth;

/// <summary>
/// Token generation contract implemented in the Infrastructure layer.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a signed JWT access token embedding the user's identity,
    /// role, and permission claims.
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a cryptographically random, Base64-encoded opaque refresh token.
    /// </summary>
    string GenerateRefreshToken();
}