using ERP.Banking.Application.DTOs.Auth;
using ERP.Banking.Application.Interfaces.Auth;   
using ERP.Banking.Application.Interfaces.Email;  
using ERP.Banking.Application.Settings;
using ERP.Banking.Domain.Entities;
using ERP.Banking.Domain.Exceptions;
using ERP.Banking.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ERP.Banking.Infrastructure.ExternalServices.Auth;

/// <summary>
/// Handles user authentication, token management, and password reset flows.
/// </summary>
internal sealed class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        ApplicationDbContext context,
        IJwtService jwtService,
        IEmailService emailService,
        IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _jwtService = jwtService;
        _emailService = emailService;
        _jwtSettings = jwtSettings.Value;
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
            user.RecordFailedLogin();
            await _context.SaveChangesAsync(cancellationToken);
            throw new DomainException("Invalid username or password.");
        }

        user.ResetFailedLoginCount();

        var (accessToken, refreshToken) = await IssueTokenPairAsync(user, cancellationToken);

        return BuildLoginResponse(user, accessToken, refreshToken);
    }

    // ── Refresh Token ──────────────────────────────────────────────

    public async Task<LoginResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var tokenEntity = await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.Role!)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken)
            ?? throw new DomainException("Refresh token not found.");

        if (!tokenEntity.IsActive)
            throw new DomainException(
                tokenEntity.IsRevoked
                    ? "Refresh token has been revoked."
                    : "Refresh token has expired.");

        tokenEntity.Revoke();

        var user = tokenEntity.User;
        var (accessToken, newRefreshToken) = await IssueTokenPairAsync(user, cancellationToken);

        return BuildLoginResponse(user, accessToken, newRefreshToken);
    }

    // ── Logout ─────────────────────────────────────────────────────

    public async Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var tokenEntity = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (tokenEntity is null || tokenEntity.IsRevoked) return;

        tokenEntity.Revoke();
        await _context.SaveChangesAsync(cancellationToken);
    }

    // ── Forgot Password ────────────────────────────────────────────

    public async Task ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null) return; // Silent — prevents user enumeration

        user.SetPasswordResetToken(
            token: Guid.NewGuid().ToString("N"),
            expiry: TimeSpan.FromHours(1));

        await _context.SaveChangesAsync(cancellationToken);
        await SendPasswordResetEmailAsync(user, cancellationToken);
    }

    // ── Reset Password ─────────────────────────────────────────────

    public async Task ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.PasswordResetToken == request.Token &&
            u.PasswordResetTokenExpiry != null &&
            u.PasswordResetTokenExpiry > DateTime.UtcNow,
            cancellationToken)
            ?? throw new DomainException("Password reset token is invalid or has expired.");

        user.UpdatePassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
        await _context.SaveChangesAsync(cancellationToken);
    }

    // ── Private Helpers ────────────────────────────────────────────

    private Task<User?> FindUserWithRoleAsync(
        string usernameOrEmail,
        CancellationToken cancellationToken) =>
        _context.Users
            .Include(u => u.Role!)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u =>
                u.Username == usernameOrEmail ||
                u.Email == usernameOrEmail,
                cancellationToken);

    private static void EnforceNotLockedOut(User user)
    {
        if (!user.IsLockedOut) return;

        var minutesLeft = (int)Math.Ceiling(
            (user.LockoutEnd!.Value - DateTime.UtcNow).TotalMinutes);

        throw new DomainException(
            $"Account is locked. Please try again in {minutesLeft} minute(s).");
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
        var resetLink = $"http://localhost:4200/reset-password?token={user.PasswordResetToken}";

        await _emailService.SendEmailAsync(
            to: user.Email,
            subject: "Password Reset Request",
            body: $"""
                   <h3>Password Reset</h3>
                   <p>You requested a password reset. Click the link below to proceed:</p>
                   <a href="{resetLink}">Reset my password</a>
                   <p>This link expires in 1 hour. If you did not request this, ignore this email.</p>
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