using ERP.Banking.Domain.Constants;
using ERP.Banking.Domain.Entities;
using ERP.Banking.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Banking.Infrastructure.Persistence.Seed;

internal static class RolePermissionSeeder
{
    private static readonly Dictionary<string, string[]> RolePermissionMap = new()
    {
        [Roles.ReadOnly] =
        [
            Permissions.ClientRead,
            Permissions.AccountRead
        ],
        [Roles.BankingAgent] =
        [
            Permissions.ClientCreate, Permissions.ClientRead,
            Permissions.AccountCreate, Permissions.AccountRead
        ],
        [Roles.BusinessAdmin] =
        [
            Permissions.UserCreate, Permissions.UserUpdate,
            Permissions.UserDelete, Permissions.UserRead,
            Permissions.RoleManage,
            Permissions.ClientCreate, Permissions.ClientRead
        ]
    };

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.RolePermissions.AnyAsync()) return;

        var roles = await context.Roles.ToListAsync();
        var permissions = await context.Permissions.ToListAsync();

        if (roles.Count == 0 || permissions.Count == 0) return;

        var entries = new List<RolePermission>();

        // SuperAdmin gets every permission
        var superAdmin = roles.FirstOrDefault(r => r.Name == Roles.SuperAdmin);
        if (superAdmin is not null)
        {
            entries.AddRange(
                permissions.Select(p => RolePermission.Create(superAdmin.Id, p.Id)));
        }

        // All other roles from the map
        foreach (var (roleName, permissionCodes) in RolePermissionMap)
        {
            var role = roles.FirstOrDefault(r => r.Name == roleName);
            if (role is null) continue;

            entries.AddRange(
                permissions
                    .Where(p => permissionCodes.Contains(p.Code))
                    .Select(p => RolePermission.Create(role.Id, p.Id)));
        }

        if (entries.Count > 0)
        {
            await context.RolePermissions.AddRangeAsync(entries);
            await context.SaveChangesAsync();
        }
    }
}