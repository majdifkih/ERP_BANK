using ERP.Banking.Domain.Common;
using ERP.Banking.Domain.ValueObjects;

namespace ERP.Banking.Domain.Entities;

/// <summary>
/// Represents an authenticated system user with a single role and account lifecycle state.
/// </summary>
public sealed class User : BaseEntity
{
    private const int MaxFailedAttempts = 5;

    // ── Identity ───────────────────────────────────────────────────

    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    // ── Profile ────────────────────────────────────────────────────

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    // ── Account State ──────────────────────────────────────────────

    public bool IsActive { get; private set; } = true;
    public int AccessFailedCount { get; private set; } = 0;
    public DateTime? LockoutEnd { get; private set; }

    // ── Password Reset ─────────────────────────────────────────────

    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiry { get; private set; }

    // ── Navigation Properties ──────────────────────────────────────

    public Guid RoleId { get; private set; }
    public Role? Role { get; private set; }      // ← une seule propriété Role, nullable

    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    // ── Computed ───────────────────────────────────────────────────

    public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
    public string FullName => $"{FirstName} {LastName}".Trim();

    // ── Constructor (private — use factory) ────────────────────────

    private User() { }   // ← requis par EF Core

    // ── Factory ────────────────────────────────────────────────────

    public static User Create(
        string username,
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        Guid roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        var validatedEmail = ValueObjects.Email.From(email);
        return new User                           
        {
            Username = username.Trim(),
            Email = validatedEmail.Value,
            PasswordHash = passwordHash,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            RoleId = roleId
        };
    }

    // ── Behaviour ──────────────────────────────────────────────────

    public void RecordFailedLogin()
    {
        AccessFailedCount++;

        if (AccessFailedCount >= MaxFailedAttempts)
            LockoutEnd = DateTime.UtcNow.AddMinutes(30);

        MarkAsUpdated();
    }

    public void ResetFailedLoginCount()
    {
        AccessFailedCount = 0;
        LockoutEnd = null;
        MarkAsUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public void UpdatePassword(string newPasswordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash);
        PasswordHash = newPasswordHash;
        ClearPasswordResetToken();
    }

    public void SetPasswordResetToken(string token, TimeSpan expiry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        PasswordResetToken = token;
        PasswordResetTokenExpiry = DateTime.UtcNow.Add(expiry);
        MarkAsUpdated();
    }

    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;
        MarkAsUpdated();
    }

    public void AddRefreshToken(RefreshToken token)
    {
        ArgumentNullException.ThrowIfNull(token);
        _refreshTokens.Add(token);
    }
}