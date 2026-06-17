using ERP.Bancaire.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP.Bancaire.Persistence;

public class ERPBancaireDbContext : DbContext
{
	public ERPBancaireDbContext(
		DbContextOptions<ERPBancaireDbContext> options)
		: base(options)
	{
	}

	public DbSet<User> Users => Set<User>();

	public DbSet<Role> Roles => Set<Role>();

	public DbSet<Permission> Permissions => Set<Permission>();

	public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

	public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		builder.Entity<RolePermission>()
			.HasKey(x => new
			{
				x.RoleId,
				x.PermissionId
			});
	}
}