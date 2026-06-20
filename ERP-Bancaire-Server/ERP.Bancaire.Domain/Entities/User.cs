namespace ERP.Bancaire.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int AccessFailedCount { get; set; } = 0;

    public DateTime? LockoutEnd { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid RoleId { get; set; }

    public Role Role { get; set; } = null!;

    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetTokenExpiry { get; set; }
}