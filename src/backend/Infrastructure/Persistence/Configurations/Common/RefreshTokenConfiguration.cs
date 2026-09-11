using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// Entity Framework configuration for the RefreshToken entity.
/// </summary>
public class RefreshTokenConfiguration : BaseEntityConfiguration<RefreshToken>
{
    /// <summary>
    /// Configures RefreshToken-specific properties.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    protected override void ConfigureEntity(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(rt => rt.UserId)
            .IsRequired();

        builder.Property(rt => rt.TokenHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(rt => rt.RevokedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(rt => rt.ReplacedByTokenId);

        // Index on TokenHash for fast lookups
        builder.HasIndex(rt => rt.TokenHash)
            .IsUnique()
            .HasDatabaseName("IX_RefreshToken_TokenHash");

        // Index on UserId for finding user's tokens
        builder.HasIndex(rt => rt.UserId)
            .HasDatabaseName("IX_RefreshToken_UserId");

        // Composite index for active token queries
        builder.HasIndex(rt => new { rt.UserId, rt.RevokedAt, rt.ExpiresAt })
            .HasDatabaseName("IX_RefreshToken_ActiveByUser");
    }
}
