using ERP.Bancaire.Application.Interfaces;
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

    public AuthService(
        ERPBancaireDbContext context,
        IJwtService jwtService,
        IOptions<JwtSettings> settings)
    {
        _context = context;
        _jwtService = jwtService;
        _settings = settings.Value;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u =>
                u.Username == request.Username);

        if (user == null)
            throw new Exception("Utilisateur introuvable");

        bool valid =
            BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash);

        if (!valid)
            throw new Exception("Mot de passe invalide");

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
}