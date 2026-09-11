using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballCompetition entity hierarchy (TPH).
    /// Subclass-specific properties (FloorballSeason, FloorballTournament) are configured in their own configuration classes.
    /// </summary>
    public class FloorballCompetitionConfiguration : IEntityTypeConfiguration<FloorballCompetition>
    {
        public void Configure(EntityTypeBuilder<FloorballCompetition> builder)
        {
            builder.HasKey(s => s.Id);

            builder.HasDiscriminator<string>("CompetitionType")
                .HasValue<FloorballSeason>("Season")
                .HasValue<FloorballTournament>("Tournament");

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

            builder.Property(s => s.TeamCategory)
                .IsRequired()
                .HasConversion<string>()
                .HasDefaultValue(Domain.Enums.Common.TeamCategory.Adult);

            builder.HasIndex(s => s.TeamCategory);

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

            builder.HasMany(s => s.Teams)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "FloorballCompetitionTeam",
                    j => j.HasOne<FloorballTeam>().WithMany().HasForeignKey("TeamsId"),
                    j => j.HasOne<FloorballCompetition>().WithMany().HasForeignKey("CompetitionsId")
                );
        }
    }
}
