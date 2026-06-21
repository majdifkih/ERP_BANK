using ERP.Banking.Domain.Constants;
using ERP.Banking.Domain.Entities;
using ERP.Banking.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Banking.Infrastructure.Persistence.Seed;

internal static class RoleSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Roles.AnyAsync()) return;

        var roles = new[]
        {
            Role.Create(Roles.SuperAdmin,     "Full system access — all permissions granted."),
            Role.Create(Roles.BusinessAdmin,  "Business administration — user and role management."),
            Role.Create(Roles.BranchDirector, "Branch director — oversight of branch operations."),
            Role.Create(Roles.ServiceManager, "Service manager — manages service-level operations."),
            Role.Create(Roles.BankingAgent,   "Banking agent — handles client and account operations."),
            Role.Create(Roles.Auditor,        "Read-only access for audit and compliance purposes."),
            Role.Create(Roles.ReadOnly,       "Read-only consultation access.")
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
    }
}