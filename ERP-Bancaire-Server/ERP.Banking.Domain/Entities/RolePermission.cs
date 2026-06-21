namespace ERP.Banking.Domain.Entities;

/// <summary>
/// Join entity linking roles to permissions (many-to-many).
/// </summary>
public sealed class RolePermission
{
    public Guid RoleId { get; init; }
    public Role Role { get; init; } = null!;

    public Guid PermissionId { get; init; }
    public Permission Permission { get; init; } = null!;

    // ── Factory ────────────────────────────────────────────────────

    public static RolePermission Create(Guid roleId, Guid permissionId) =>
        new() { RoleId = roleId, PermissionId = permissionId };
}