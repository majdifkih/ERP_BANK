using BCrypt.Net;
using ERP.Bancaire.Domain.Constants;
using ERP.Bancaire.Domain.Entities;

namespace ERP.Bancaire.Persistence.Seed;

public static class AdminSeeder
{
    public static async Task SeedAsync(ERPBancaireDbContext context)
    {
        if (context.Users.Any())
            return;

        var superAdminRole = context.Roles
            .FirstOrDefault(r => r.Name == Roles.SuperAdmin);

        if (superAdminRole == null)
            return;

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            Email = "admin@erp-bancaire.tn",
            FirstName = "Super",
            LastName = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            IsActive = true,
            RoleId = superAdminRole.Id,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(admin);

        await context.SaveChangesAsync();
    }
}