using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Common
{
    /// <summary>
    /// Entity Framework configuration for the Club entity.
    /// </summary>
    public class ClubConfiguration : IEntityTypeConfiguration<Club>
    {
        /// <summary>
        /// Configures the entity mapping for Club.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<Club> builder)
        {

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Country)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.FoundingDate)
                .IsRequired();

            builder.Property(c => c.WebsiteUrl)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => new Uri(v));

            builder.Property(c => c.LogoUrl)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => new Uri(v));

            builder.Property(c => c.ContactEmail)
                .IsRequired()
                .HasMaxLength(255);

            // Ignore navigation properties to prevent cross-context entity discovery
            builder.Ignore(c => c.FloorballTeams);
            builder.Ignore(c => c.HockeyTeams);
        }
    }
} 
