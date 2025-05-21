using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballReferee entity.
    /// </summary>
    public class FloorballRefereeConfiguration : IEntityTypeConfiguration<FloorballReferee>
    {
        /// <summary>
        /// Configures the entity mapping for FloorballReferee.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<FloorballReferee> builder)
        {
            builder.Property(r => r.IsActive)
                .IsRequired();

            builder.Property(r => r.LicenseIssueDate)
                .HasColumnType("timestamp")
                .IsRequired(false);

            builder.Property(r => r.LicenseExpiryDate)
                .HasColumnType("timestamp")
                .IsRequired(false);

            builder.Property(r => r.MatchesOfficiated)
                .IsRequired();
        }
    }
}
