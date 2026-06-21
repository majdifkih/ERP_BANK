using ERP.Banking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ERP.Banking.Infrastructure.Persistence.Context;

/// <summary>
/// EF Core DbContext for the Banking ERP application.
/// All entity configurations are loaded automatically via IEntityTypeConfiguration.
/// </summary>
public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Auto-loads all IEntityTypeConfiguration<T> in this assembly
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}