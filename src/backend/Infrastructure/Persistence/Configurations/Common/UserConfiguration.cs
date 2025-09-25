using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Common
{
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

            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(u => u.Role)
                .IsRequired()
                .HasConversion<int>();

            // Create unique index on Username
            builder.HasIndex(u => u.Username)
                .IsUnique()
                .HasDatabaseName("IX_User_Username");
        }
    }
} 
