using ERP.Banking.Application.DTOs.Auth;
using ERP.Banking.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Banking.API.Controllers;

/// <summary>
/// Exposes authentication endpoints: login, token refresh, logout, and password reset.
/// Business logic and exception handling are fully delegated to the service layer
/// and <c>ExceptionHandlingMiddleware</c> respectively.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // ── POST api/auth/login ────────────────────────────────────────

    /// <summary>
    /// Authenticates a user by username or email and returns a JWT
    /// access token paired with a rotating refresh token.
    /// </summary>
    /// <response code="200">Authentication succeeded.</response>
    /// <response code="400">Invalid credentials or account locked.</response>
    /// <response code="404">User not found.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }

    // ── POST api/auth/refresh ──────────────────────────────────────

    /// <summary>
    /// Rotates a valid refresh token and returns a new token pair.
    /// The old refresh token is immediately revoked.
    /// </summary>
    /// <response code="200">Token rotated successfully.</response>
    /// <response code="400">Token is missing, revoked, or expired.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(
            request.RefreshToken, cancellationToken);
        return Ok(result);
    }

    // ── POST api/auth/logout ───────────────────────────────────────

    /// <summary>
    /// Revokes the supplied refresh token, ending the current session.
    /// Idempotent — calling this with an already-revoked token is safe.
    /// </summary>
    /// <response code="204">Session terminated.</response>
    /// <response code="400">Refresh token is required.</response>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

    // ── POST api/auth/forgot-password ─────────────────────────────

    /// <summary>
    /// Dispatches a password reset link to the registered email address.
    /// Always returns 200 regardless of whether the email exists,
    /// to prevent user enumeration.
    /// </summary>
    /// <response code="200">Request processed (email sent if account exists).</response>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _authService.ForgotPasswordAsync(request, cancellationToken);
        return Ok(new
        {
            message = "If the account exists, a password reset email has been sent."
        });
    }

    // ── POST api/auth/reset-password ──────────────────────────────

    /// <summary>
    /// Validates a one-time reset token and sets a new password.
    /// The token is invalidated upon successful use.
    /// </summary>
    /// <response code="200">Password updated successfully.</response>
    /// <response code="400">Token is invalid, expired, or passwords do not match.</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _authService.ResetPasswordAsync(request, cancellationToken);
        return Ok(new { message = "Password has been reset successfully." });
    }
}