using ERP.Banking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Banking.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token)
               .IsRequired()
               .HasMaxLength(512);

        builder.HasIndex(rt => rt.Token)
               .IsUnique();

        builder.Property(rt => rt.ExpiresAt)
               .IsRequired();

        builder.Property(rt => rt.IsRevoked)
               .IsRequired()
               .HasDefaultValue(false);
    }
}