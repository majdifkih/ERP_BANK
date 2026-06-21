using System.ComponentModel.DataAnnotations;

namespace ERP.Banking.Application.DTOs.Auth;

/// <summary>
/// Payload required to revoke a refresh token on logout.
/// </summary>
public sealed record LogoutRequest
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}