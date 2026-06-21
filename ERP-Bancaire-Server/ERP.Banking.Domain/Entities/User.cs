using ERP.Banking.Domain.Common;
using ERP.Banking.Domain.ValueObjects;

namespace ERP.Banking.Domain.Entities;

/// <summary>
/// Represents an authenticated system user with a single role,
/// account lifecycle state, and lockout protection.
/// </summary>
public sealed class User : BaseEntity
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutDurationMinutes = 30;

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

    // ── Navigation ─────────────────────────────────────────────────

    public Guid RoleId { get; private set; }
    public Role? Role { get; private set; }

    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    // ── Computed ───────────────────────────────────────────────────

    public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
    public string FullName => $"{FirstName} {LastName}".Trim();

    // ── EF Core constructor ────────────────────────────────────────

    private User() { }

    // ── Factory ────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="User"/> with validated identity fields.
    /// The password must already be hashed by the caller.
    /// </summary>
    public static User Create(
        string username,
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        Guid roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username, nameof(username));
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash, nameof(passwordHash));
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName, nameof(firstName));
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName, nameof(lastName));

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

    /// <summary>
    /// Increments the failed login counter and locks the account
    /// after <see cref="MaxFailedAttempts"/> consecutive failures.
    /// </summary>
    public void RecordFailedLogin()
    {
        AccessFailedCount++;

        if (AccessFailedCount >= MaxFailedAttempts)
        {
            LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);
            AccessFailedCount = 0;
        }

        MarkAsUpdated();
    }

    /// <summary>Clears lockout state after a successful login.</summary>
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

    /// <summary>
    /// Replaces the current password hash and invalidates any
    /// pending password reset token.
    /// </summary>
    public void UpdatePassword(string newPasswordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash, nameof(newPasswordHash));
        PasswordHash = newPasswordHash;
        ClearPasswordResetToken();
        MarkAsUpdated();
    }

    /// <summary>
    /// Issues a one-time password reset token with an expiry window.
    /// Any previously issued token is overwritten.
    /// </summary>
    public void SetPasswordResetToken(string token, TimeSpan expiry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token, nameof(token));
        if (expiry <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(expiry), "Expiry must be positive.");

        PasswordResetToken = token;
        PasswordResetTokenExpiry = DateTime.UtcNow.Add(expiry);
        MarkAsUpdated();
    }

    /// <summary>Invalidates the current password reset token.</summary>
    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;
        MarkAsUpdated();
    }

    public void AddRefreshToken(RefreshToken token)
    {
        ArgumentNullException.ThrowIfNull(token, nameof(token));
        _refreshTokens.Add(token);
    }
}