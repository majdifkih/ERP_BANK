using ERP.Banking.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ERP.Banking.Infrastructure.Persistence.Seed;

/// <summary>
/// Orchestrates database migration and seed data on application startup.
/// Call <see cref="InitialiseAsync"/> from Program.cs once the host is built.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task InitialiseAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider
                                       .GetRequiredService<ILogger<ApplicationDbContext>>();
        try
        {
            logger.LogInformation("Applying pending migrations…");
            await context.Database.MigrateAsync();

            logger.LogInformation("Seeding reference data…");
            await RoleSeeder.SeedAsync(context);
            await PermissionSeeder.SeedAsync(context);
            await RolePermissionSeeder.SeedAsync(context);
            await AdminSeeder.SeedAsync(context);

            logger.LogInformation("Database initialisation complete.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }
}