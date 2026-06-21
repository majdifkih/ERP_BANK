using System.ComponentModel.DataAnnotations;

namespace ERP.Banking.Application.DTOs.Auth;

/// <summary>
/// Payload required to authenticate a user and obtain a token pair.
/// </summary>
public sealed record LoginRequest
{
    /// <summary>Username or email address.</summary>
    [Required]
    [StringLength(256, MinimumLength = 3)]
    public string Username { get; init; } = string.Empty;

    /// <summary>Plain-text password — never stored or logged.</summary>
    [Required]
    [StringLength(128, MinimumLength = 6)]
    public string Password { get; init; } = string.Empty;
}