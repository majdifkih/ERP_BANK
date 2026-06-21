using ERP.Banking.Application.DTOs.Auth;
using ERP.Banking.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Banking.API.Controllers;

/// <summary>
/// Handles authentication — login, token refresh, logout, and password reset.
/// All exception handling is delegated to ExceptionHandlingMiddleware.
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

    /// <summary>Authenticates a user and returns a JWT access + refresh token pair.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }

    // ── POST api/auth/refresh ──────────────────────────────────────

    /// <summary>Rotates a refresh token and returns a new token pair.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(
            request.RefreshToken, cancellationToken);
        return Ok(result);
    }

    // ── POST api/auth/logout ───────────────────────────────────────

    /// <summary>Revokes the provided refresh token, ending the session.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

    // ── POST api/auth/forgot-password ─────────────────────────────

    /// <summary>
    /// Sends a password reset link to the provided email.
    /// Always returns 200 to prevent user enumeration.
    /// </summary>
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

    /// <summary>Validates the reset token and sets a new password.</summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _authService.ResetPasswordAsync(request, cancellationToken);

        return Ok(new { message = "Password has been reset successfully." });
    }
}