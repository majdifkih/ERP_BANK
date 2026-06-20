using System.Net;
using ERP.Bancaire.Application.Interfaces;
using ERP.Bancaire.Application.DTOs.Auth;
using ERP.Bancaire.Domain.Entities;
using ERP.Bancaire.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ERP.Bancaire.Application.Settings;

namespace ERP.Bancaire.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ERPBancaireDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly JwtSettings _settings;
    private readonly ERP.Bancaire.Application.Interfaces.IEmailService _emailService;

    public AuthService(
        ERPBancaireDbContext context,
        IJwtService jwtService,
        IOptions<JwtSettings> settings,
        ERP.Bancaire.Application.Interfaces.IEmailService emailService)
    {
        _context = context;
        _jwtService = jwtService;
        _settings = settings.Value;
        _emailService = emailService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u =>
                u.Username == request.Username ||
                u.Email == request.Username);

        if (user == null)
            throw new Exception("Utilisateur introuvable");

        if (user.LockoutEnd.HasValue &&
            user.LockoutEnd.Value > DateTime.UtcNow)
        {
            var minutesLeft = (int)Math.Ceiling(
                (user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
            throw new Exception($"Compte bloqué pendant {minutesLeft} minute(s). Réessayez plus tard.");
        }

        bool valid =
            BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash);

        if (!valid)
        {
            user.AccessFailedCount += 1;

            if (user.AccessFailedCount >= 3)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                user.AccessFailedCount = 0;
            }

            await _context.SaveChangesAsync();
            throw new Exception("Nom d’utilisateur ou mot de passe incorrect.");
        }

        user.AccessFailedCount = 0;
        user.LockoutEnd = null;

        var accessToken =
            _jwtService.GenerateAccessToken(user);

        var refreshToken =
            _jwtService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenDays),
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Username = user.Username,
            Role = user.Role.Name
        };
    }

    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (tokenEntity == null || tokenEntity.IsRevoked)
            throw new Exception("Refresh token invalide");

        if (tokenEntity.ExpiresAt < DateTime.UtcNow)
            throw new Exception("Refresh token expiré");

        var user = tokenEntity.User;

        var accessToken =
            _jwtService.GenerateAccessToken(user);

        var newRefreshToken =
            _jwtService.GenerateRefreshToken();

        tokenEntity.IsRevoked = true;

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenDays),
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            Username = user.Username,
            Role = user.Role.Name
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var tokenEntity = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (tokenEntity == null)
            return;

        tokenEntity.IsRevoked = true;
        await _context.SaveChangesAsync();
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
{
    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Email == request.Email);

    if (user == null)
        return; // sécurité

    var token = Guid.NewGuid().ToString();

    user.PasswordResetToken = token;
    user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

    await _context.SaveChangesAsync();

    var resetLink =
        $"http://localhost:4200/reset-password?token={token}";

    try
    {
        await _emailService.SendEmailAsync(
            user.Email,
            "Réinitialisation du mot de passe",
            $"""
            <h3>Réinitialisation du mot de passe</h3>
            <p>Cliquez sur le lien suivant :</p>
            <a href="{resetLink}">Réinitialiser mon mot de passe</a>
            """
        );
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erreur email reset password: {ex.Message}");
    }
}
public async Task ResetPasswordAsync(ResetPasswordRequest request)
{
    if (string.IsNullOrWhiteSpace(request.NewPassword))
        throw new Exception("Mot de passe invalide");

    var user = await _context.Users.FirstOrDefaultAsync(u =>
        u.PasswordResetToken == request.Token &&
        u.PasswordResetTokenExpiry != null &&
        u.PasswordResetTokenExpiry > DateTime.UtcNow);

    if (user == null)
        throw new Exception("Token invalide ou expiré");

    user.PasswordHash =
        BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

    user.PasswordResetToken = null;
    user.PasswordResetTokenExpiry = null;

    await _context.SaveChangesAsync();
}
}