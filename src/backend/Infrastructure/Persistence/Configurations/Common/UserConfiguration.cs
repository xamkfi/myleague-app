using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// Entity Framework configuration for the User entity.
/// </summary>
public class UserConfiguration : BaseEntityConfiguration<User>
{
    /// <summary>
    /// Configures User-specific properties.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    protected override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.PersonId)
            .IsRequired();

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.LastLoginAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(u => u.LoginCode)
            .HasMaxLength(10);

        builder.Property(u => u.LoginCodeExpiresAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(u => u.LoginCodeAttempts)
            .IsRequired()
            .HasDefaultValue(0);

        // Configure relationship with Person
        builder.HasOne(u => u.Person)
            .WithMany()
            .HasForeignKey(u => u.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship with RefreshTokens
        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Create unique index on Email
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_User_Email");

        // Create index on PersonId
        builder.HasIndex(u => u.PersonId)
            .HasDatabaseName("IX_User_PersonId");
    }
}
