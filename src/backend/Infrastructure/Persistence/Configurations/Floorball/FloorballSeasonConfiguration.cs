using Domain.Entities.Floorball;
using Domain.ValueObjects.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballSeason entity.
    /// </summary>
    public class FloorballSeasonConfiguration : IEntityTypeConfiguration<FloorballSeason>
    {
        /// <summary>
        /// Configures the entity mapping for FloorballSeason.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<FloorballSeason> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.StartDate)
                .IsRequired();

            builder.Property(s => s.EndDate)
                .IsRequired();

            builder.Property(s => s.IsActive)
                .IsRequired();

            builder.Property(s => s.IsCompleted)
                .IsRequired();

            // Configure MatchRules as an owned entity (stored as columns in the same table)
            builder.OwnsOne(s => s.MatchRules, rules =>
            {
                rules.Property(r => r.NumberOfPeriods)
                    .HasColumnName("MatchRules_NumberOfPeriods")
                    .IsRequired()
                    .HasDefaultValue(2);

                rules.Property(r => r.PeriodDurationMinutes)
                    .HasColumnName("MatchRules_PeriodDurationMinutes")
                    .IsRequired()
                    .HasDefaultValue(15);

                rules.Property(r => r.AllowOvertime)
                    .HasColumnName("MatchRules_AllowOvertime")
                    .IsRequired()
                    .HasDefaultValue(true);

                rules.Property(r => r.OvertimeDurationMinutes)
                    .HasColumnName("MatchRules_OvertimeDurationMinutes")
                    .IsRequired()
                    .HasDefaultValue(5);

                rules.Property(r => r.AllowShootout)
                    .HasColumnName("MatchRules_AllowShootout")
                    .IsRequired()
                    .HasDefaultValue(true);
            });

            // Configure many-to-many relationship with FloorballTeam
            builder.HasMany(s => s.Teams)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "FloorballSeasonTeam",
                    j => j.HasOne<FloorballTeam>().WithMany().HasForeignKey("TeamsId"),
                    j => j.HasOne<FloorballSeason>().WithMany().HasForeignKey("SeasonsId")
                );

            // Configure one-to-many relationship with FloorballMatch
            // Note: The inverse relationship is configured in FloorballMatchConfiguration
        }
    }
} 