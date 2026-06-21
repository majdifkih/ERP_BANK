using ERP.Banking.Domain.Entities;
using ERP.Banking.Domain.Common;

namespace ERP.Banking.Domain.Entities;

/// <summary>
/// Represents a discrete action or resource access right within the system.
/// Permissions are assigned to roles, not directly to users.
/// </summary>
public sealed class Permission : BaseEntity
{
    /// <summary>Machine-readable code (e.g. "USER_CREATE"). Must be unique.</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>Human-readable description for admin UIs and audits.</summary>
    public string Description { get; private set; } = string.Empty;

    public IReadOnlyCollection<RolePermission> RolePermissions
        => _rolePermissions.AsReadOnly();

    private readonly List<RolePermission> _rolePermissions = [];

    // ── Factory ────────────────────────────────────────────────────

    public static Permission Create(string code, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new Permission
        {
            Code = code.ToUpperInvariant().Trim(),
            Description = description.Trim()
        };
    }
}