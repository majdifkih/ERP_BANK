using ERP.Banking.Domain.Common;

namespace ERP.Banking.Domain.Entities;

/// <summary>
/// Represents an OAuth-style refresh token tied to a user session.
/// Tokens are single-use and can be revoked explicitly.
/// </summary>
public sealed class RefreshToken : BaseEntity
{
    public string Token { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }

    // ── Computed ───────────────────────────────────────────────────

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    // ── Factory ────────────────────────────────────────────────────

    public static RefreshToken Create(string token, Guid userId, TimeSpan lifetime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        return new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.Add(lifetime)
        };
    }

    // ── Behaviour ──────────────────────────────────────────────────

    public void Revoke() => IsRevoked = true;
}