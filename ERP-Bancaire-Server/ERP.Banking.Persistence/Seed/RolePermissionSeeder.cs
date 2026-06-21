using ERP.Bancaire.Domain.Constants;
using ERP.Bancaire.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP.Bancaire.Persistence.Seed;

public static class RolePermissionSeeder
{
    public static async Task SeedAsync(
        ERPBancaireDbContext context)
    {
        if (context.RolePermissions.Any())
            return;

        var roles = await context.Roles.ToListAsync();
        var permissions = await context.Permissions.ToListAsync();

        if (!roles.Any() || !permissions.Any())
            return;

        var rolePermissions = new List<RolePermission>();

        var superAdminRole = roles.FirstOrDefault(r => r.Name == Roles.SuperAdmin);
        if (superAdminRole != null)
        {
            rolePermissions.AddRange(permissions.Select(permission => new RolePermission
            {
                RoleId = superAdminRole.Id,
                PermissionId = permission.Id
            }));
        }

        var consultationRole = roles.FirstOrDefault(r => r.Name == Roles.Consultation);
        if (consultationRole != null)
        {
            var allowed = new[] { Permissions.ClientRead, Permissions.AccountRead };
            rolePermissions.AddRange(permissions
                .Where(p => allowed.Contains(p.Code))
                .Select(permission => new RolePermission
                {
                    RoleId = consultationRole.Id,
                    PermissionId = permission.Id
                }));
        }

        var agentRole = roles.FirstOrDefault(r => r.Name == Roles.AgentBancaire);
        if (agentRole != null)
        {
            var allowed = new[] { Permissions.ClientCreate, Permissions.ClientRead, Permissions.AccountCreate, Permissions.AccountRead };
            rolePermissions.AddRange(permissions
                .Where(p => allowed.Contains(p.Code))
                .Select(permission => new RolePermission
                {
                    RoleId = agentRole.Id,
                    PermissionId = permission.Id
                }));
        }

        var adminMetierRole = roles.FirstOrDefault(r => r.Name == Roles.AdminMetier);
        if (adminMetierRole != null)
        {
            var allowed = new[] { Permissions.UserCreate, Permissions.UserUpdate, Permissions.UserDelete, Permissions.UserRead, Permissions.RoleManage, Permissions.ClientCreate, Permissions.ClientRead };
            rolePermissions.AddRange(permissions
                .Where(p => allowed.Contains(p.Code))
                .Select(permission => new RolePermission
                {
                    RoleId = adminMetierRole.Id,
                    PermissionId = permission.Id
                }));
        }

        if (rolePermissions.Any())
        {
            await context.RolePermissions.AddRangeAsync(rolePermissions);
            await context.SaveChangesAsync();
        }
    }
}
