using ERP.Banking.Application.DTOs.Auth;

namespace ERP.Banking.Application.Interfaces.Auth;

/// <summary>
/// Defines the authentication contract implemented by the Infrastructure layer.
/// Covers login, token rotation, logout, and password reset flows.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Validates credentials and returns a JWT access token + refresh token pair.
    /// Throws <see cref="ERP.Banking.Domain.Exceptions.DomainException"/> on failure.
    /// </summary>
    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotates a refresh token — revokes the old one and issues a new pair.
    /// </summary>
    Task<LoginResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes the given refresh token, effectively ending the session.
    /// </summary>
    Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a password reset token and sends it to the user's email.
    /// Returns silently if the email is not found (prevents user enumeration).
    /// </summary>
    Task ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the reset token and updates the user's password.
    /// </summary>
    Task ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default);
}