using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Common
{
    /// <summary>
    /// Entity Framework configuration for the ClubManager entity.
    /// </summary>
    public class ClubManagerConfiguration : IEntityTypeConfiguration<ClubManager>
    {
        /// <summary>
        /// Configures the entity mapping for ClubManager.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<ClubManager> builder)
        {
            builder.HasKey(cm => cm.Id);

            builder.Property(cm => cm.PersonId)
                .IsRequired();

            builder.Property(cm => cm.ClubId)
                .IsRequired();

            builder.Property(cm => cm.IsActive)
                .IsRequired();

            builder.HasIndex(cm => new { cm.PersonId, cm.ClubId })
                .IsUnique();

            builder.HasIndex(cm => cm.ClubId);

            builder.HasOne<Club>()
                .WithMany()
                .HasForeignKey(cm => cm.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Person>()
                .WithMany()
                .HasForeignKey(cm => cm.PersonId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
