using ERP.Bancaire.Domain.Constants;
using ERP.Bancaire.Domain.Entities;

namespace ERP.Bancaire.Persistence.Seed;

public static class RoleSeeder
{
    public static async Task SeedAsync(
        ERPBancaireDbContext context)
    {
        if (context.Roles.Any())
            return;

        var roles = new List<Role>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = Roles.SuperAdmin,
                Description = "Administrateur global"
            },

            new()
            {
                Id = Guid.NewGuid(),
                Name = Roles.AdminMetier,
                Description = "Administrateur métier"
            },

            new()
            {
                Id = Guid.NewGuid(),
                Name = Roles.DirecteurAgence,
                Description = "Directeur d'agence"
            },

            new()
            {
                Id = Guid.NewGuid(),
                Name = Roles.ChefService,
                Description = "Chef de service"
            },

            new()
            {
                Id = Guid.NewGuid(),
                Name = Roles.AgentBancaire,
                Description = "Agent bancaire"
            },

            new()
            {
                Id = Guid.NewGuid(),
                Name = Roles.Auditeur,
                Description = "Auditeur"
            },

            new()
            {
                Id = Guid.NewGuid(),
                Name = Roles.Consultation,
                Description = "Consultation uniquement"
            }
        };

        await context.Roles.AddRangeAsync(roles);

        await context.SaveChangesAsync();
    }
}