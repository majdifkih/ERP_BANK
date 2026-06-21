using ERP.Banking.Domain.Constants;
using ERP.Banking.Domain.Entities;
using ERP.Banking.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Banking.Infrastructure.Persistence.Seed;

internal static class AdminSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var superAdminRole = await context.Roles
            .FirstOrDefaultAsync(r => r.Name == Roles.SuperAdmin);

        if (superAdminRole is null) return;

        var admin = User.Create(
            username: "admin",
            email: "admin@erp-banking.com",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            firstName: "Super",
            lastName: "Admin",
            roleId: superAdminRole.Id);

        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}