using ERP.Banking.Domain.Constants;
using ERP.Banking.Domain.Entities;
using ERP.Banking.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Banking.Infrastructure.Persistence.Seed;

internal static class PermissionSeeder
{
    private static readonly (string Code, string Description)[] Definitions =
    [
        (Permissions.UserCreate,    "Create a new user account."),
        (Permissions.UserUpdate,    "Update an existing user account."),
        (Permissions.UserDelete,    "Delete a user account."),
        (Permissions.UserRead,      "View user account details."),
        (Permissions.RoleManage,    "Create, update, and assign roles."),
        (Permissions.ClientCreate,  "Register a new bank client."),
        (Permissions.ClientUpdate,  "Update an existing client record."),
        (Permissions.ClientRead,    "View client information."),
        (Permissions.AccountCreate, "Open a new bank account."),
        (Permissions.AccountRead,   "View bank account details."),
        (Permissions.CreditApprove, "Approve or reject credit requests.")
    ];

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Permissions.AnyAsync()) return;

        var permissions = Definitions
            .Select(d => Permission.Create(d.Code, d.Description))
            .ToArray();

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();
    }
}