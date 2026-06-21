using ERP.Banking.Application.DTOs.Auth;
using ERP.Banking.Application.Interfaces.Auth;
using ERP.Banking.Application.Interfaces.Email;
using ERP.Banking.Application.Settings;
using ERP.Banking.Domain.Entities;
using ERP.Banking.Domain.Exceptions;
using ERP.Banking.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ERP.Banking.Infrastructure.ExternalServices.Auth;

/// <summary>
/// Handles user authentication, JWT token lifecycle, and password reset flows.
/// All write operations are atomic — SaveChangesAsync is called once per use-case.
/// </summary>
internal sealed class AuthService : IAuthService
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutDurationMinutes = 30;

    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext context,
        IJwtService jwtService,
        IEmailService emailService,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _emailService = emailService;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    // ── Login ──────────────────────────────────────────────────────

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await FindUserWithRoleAsync(request.Username, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(User), request.Username);

        EnforceNotLockedOut(user);

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            // ✅ Calculate remaining attempts BEFORE mutating the entity
            var attemptsAfter = user.AccessFailedCount + 1;
            var willLockout = attemptsAfter >= MaxFailedAttempts;
            var remaining = willLockout ? 0 : MaxFailedAttempts - attemptsAfter;

            user.RecordFailedLogin();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Failed login attempt for user {Username}. " +
                "{Remaining} attempt(s) remaining. Lockout triggered: {WillLockout}.",
                request.Username, remaining, willLockout);

            var message = willLockout
                ? $"Too many failed attempts. Account locked for {LockoutDurationMinutes} minutes."
                : $"Invalid credentials. {remaining} attempt(s) remaining before lockout.";

            throw new DomainException(message);
        }

        // ✅ Successful login — clear lockout state
        user.ResetFailedLoginCount();
        await _context.SaveChangesAsync(cancellationToken);

        var (accessToken, refreshToken) = await IssueTokenPairAsync(user, cancellationToken);

        _logger.LogInformation(
            "User {Username} authenticated successfully.", user.Username);

        return BuildLoginResponse(user, accessToken, refreshToken);
    }

    // ── Refresh Token ──────────────────────────────────────────────

    public async Task<LoginResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        var tokenEntity = await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.Role!)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken)
            ?? throw new DomainException("Refresh token not found.");

        if (!tokenEntity.IsActive)
        {
            var reason = tokenEntity.IsRevoked ? "revoked" : "expired";

            _logger.LogWarning(
                "Inactive refresh token used for user {UserId}. Reason: {Reason}.",
                tokenEntity.UserId, reason);

            throw new DomainException($"Refresh token has been {reason}.");
        }

        // Rotate — revoke old, issue new
        tokenEntity.Revoke();

        var user = tokenEntity.User;
        var (accessToken, newRefreshToken) = await IssueTokenPairAsync(user, cancellationToken);

        _logger.LogInformation(
            "Refresh token rotated for user {Username}.", user.Username);

        return BuildLoginResponse(user, accessToken, newRefreshToken);
    }

    // ── Logout ─────────────────────────────────────────────────────

    public async Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        var tokenEntity = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (tokenEntity is null || tokenEntity.IsRevoked)
        {
            _logger.LogDebug(
                "Logout called with an already-revoked or unknown token.");
            return;
        }

        tokenEntity.Revoke();
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} logged out successfully.", tokenEntity.UserId);
    }

    // ── Forgot Password ────────────────────────────────────────────

    public async Task ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        // Always return silently — prevents user enumeration
        if (user is null)
        {
            _logger.LogDebug(
                "Password reset requested for unknown email {Email}.", request.Email);
            return;
        }

        user.SetPasswordResetToken(
            token: Guid.NewGuid().ToString("N"),
            expiry: TimeSpan.FromHours(1));

        await _context.SaveChangesAsync(cancellationToken);
        await SendPasswordResetEmailAsync(user, cancellationToken);

        _logger.LogInformation(
            "Password reset email dispatched for user {Username}.", user.Username);
    }

    // ── Reset Password ─────────────────────────────────────────────

    public async Task ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Token);

        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.PasswordResetToken == request.Token &&
            u.PasswordResetTokenExpiry != null &&
            u.PasswordResetTokenExpiry > DateTime.UtcNow,
            cancellationToken)
            ?? throw new DomainException(
                "Password reset token is invalid or has expired.");

        user.UpdatePassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Password successfully reset for user {Username}.", user.Username);
    }

    // ── Private Helpers ────────────────────────────────────────────

    private Task<User?> FindUserWithRoleAsync(
        string usernameOrEmail,
        CancellationToken cancellationToken) =>
        _context.Users
            .Include(u => u.Role!)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(
                u => u.Username == usernameOrEmail || u.Email == usernameOrEmail,
                cancellationToken);

    private static void EnforceNotLockedOut(User user)
    {
        if (!user.IsLockedOut) return;

        var minutesLeft = (int)Math.Ceiling(
            (user.LockoutEnd!.Value - DateTime.UtcNow).TotalMinutes);

        throw new DomainException(
            $"Account is temporarily locked. Try again in {minutesLeft} minute(s).");
    }

    private async Task<(string AccessToken, string RefreshToken)> IssueTokenPairAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        var entity = RefreshToken.Create(
            token: refreshToken,
            userId: user.Id,
            lifetime: TimeSpan.FromDays(_jwtSettings.RefreshTokenDays));

        _context.RefreshTokens.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return (accessToken, refreshToken);
    }

    private async Task SendPasswordResetEmailAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var resetLink =
            $"http://localhost:4200/reset-password?token={user.PasswordResetToken}";

        await _emailService.SendEmailAsync(
            to: user.Email,
            subject: "Password Reset Request",
            body: $"""
                   <h3>Password Reset</h3>
                   <p>You requested a password reset. Click the link below to proceed:</p>
                   <a href="{resetLink}">Reset my password</a>
                   <p>This link expires in <strong>1 hour</strong>.</p>
                   <p>If you did not request this, please ignore this email or contact support.</p>
                   """,
            cancellationToken: cancellationToken);
    }

    private static LoginResponse BuildLoginResponse(
        User user,
        string accessToken,
        string refreshToken) => new()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Username = user.Username,
            Role = user.Role?.Name ?? string.Empty
        };
}