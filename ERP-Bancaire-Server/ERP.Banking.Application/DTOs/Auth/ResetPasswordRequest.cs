using System.ComponentModel.DataAnnotations;

namespace ERP.Banking.Application.DTOs.Auth;

/// <summary>
/// Payload to complete a password reset using a valid reset token.
/// </summary>
public sealed record ResetPasswordRequest
{
    /// <summary>One-time token received via email.</summary>
    [Required]
    public string Token { get; init; } = string.Empty;

    /// <summary>The new plain-text password to set.</summary>
    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string NewPassword { get; init; } = string.Empty;

    /// <summary>Must match <see cref="NewPassword"/>.</summary>
    [Required]
    [Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; init; } = string.Empty;
}