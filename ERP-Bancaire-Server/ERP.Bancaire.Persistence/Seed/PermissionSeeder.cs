using ERP.Bancaire.Domain.Constants;
using ERP.Bancaire.Domain.Entities;

namespace ERP.Bancaire.Persistence.Seed;

public static class PermissionSeeder
{
    public static async Task SeedAsync(
        ERPBancaireDbContext context)
    {
        if (context.Permissions.Any())
            return;

        var permissions = new List<Permission>
        {
            new() { Id = Guid.NewGuid(), Code = Permissions.UserCreate },
            new() { Id = Guid.NewGuid(), Code = Permissions.UserUpdate },
            new() { Id = Guid.NewGuid(), Code = Permissions.UserDelete },
            new() { Id = Guid.NewGuid(), Code = Permissions.UserRead },

            new() { Id = Guid.NewGuid(), Code = Permissions.RoleManage },

            new() { Id = Guid.NewGuid(), Code = Permissions.ClientCreate },
            new() { Id = Guid.NewGuid(), Code = Permissions.ClientUpdate },
            new() { Id = Guid.NewGuid(), Code = Permissions.ClientRead },

            new() { Id = Guid.NewGuid(), Code = Permissions.AccountCreate },
            new() { Id = Guid.NewGuid(), Code = Permissions.AccountRead },

            new() { Id = Guid.NewGuid(), Code = Permissions.CreditApprove }
        };

        await context.Permissions.AddRangeAsync(permissions);

        await context.SaveChangesAsync();
    }
}