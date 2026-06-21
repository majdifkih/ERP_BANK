using System.ComponentModel.DataAnnotations;

namespace ERP.Banking.Application.DTOs.Auth;

/// <summary>
/// Payload required to rotate an existing refresh token.
/// </summary>
public sealed record RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}