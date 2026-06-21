using System.ComponentModel.DataAnnotations;

namespace ERP.Banking.Application.DTOs.Auth;

/// <summary>
/// Payload to initiate a password reset flow.
/// A reset link will be sent to the registered email address.
/// </summary>
public sealed record ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } = string.Empty;
}