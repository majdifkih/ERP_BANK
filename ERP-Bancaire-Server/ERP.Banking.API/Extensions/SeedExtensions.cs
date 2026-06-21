using ERP.Banking.Infrastructure.Persistence.Seed;

namespace ERP.Banking.API.Extensions;

/// <summary>
/// Provides a clean extension method to run database seeding from Program.cs.
/// </summary>
public static class SeedExtensions
{
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        await DatabaseSeeder.InitialiseAsync(app.Services);
    }
}