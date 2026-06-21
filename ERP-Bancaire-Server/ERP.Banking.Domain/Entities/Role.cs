using ERP.Banking.Domain.Entities;
using ERP.Banking.Domain.Common;

namespace ERP.Banking.Domain.Entities;

/// <summary>
/// Represents a named set of permissions that can be assigned to users.
/// </summary>
public sealed class Role : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public IReadOnlyCollection<User> Users
        => _users.AsReadOnly();

    public IReadOnlyCollection<RolePermission> RolePermissions
        => _rolePermissions.AsReadOnly();

    private readonly List<User> _users = [];
    private readonly List<RolePermission> _rolePermissions = [];

    // ── Factory ────────────────────────────────────────────────────

    public static Role Create(string name, string description = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Role
        {
            Name = name.Trim(),
            Description = description.Trim()
        };
    }
}