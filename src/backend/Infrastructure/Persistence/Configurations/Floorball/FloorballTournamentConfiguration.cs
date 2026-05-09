using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballTournament entity.
    /// Handles tournament-specific properties in the TPH FloorballCompetition hierarchy.
    /// Shared properties (Name, StartDate, MatchRules, Teams, etc.) are configured in <see cref="FloorballCompetitionConfiguration"/>.
    /// </summary>
    public class FloorballTournamentConfiguration : IEntityTypeConfiguration<FloorballTournament>
    {
        public void Configure(EntityTypeBuilder<FloorballTournament> builder)
        {
            builder.Property(t => t.ContentHtml)
                .HasMaxLength(50000);

            builder.Property(t => t.Venue)
                .HasMaxLength(200);

            builder.Property(t => t.TournamentStatus)
                .HasConversion<int>();

            builder.OwnsOne(t => t.TournamentRules, tr =>
            {
                tr.OwnsOne(r => r.GroupStageMatchRules, gsm =>
                {
                    gsm.Property(r => r.NumberOfPeriods)
                        .HasColumnName("TournamentRules_GroupStage_NumberOfPeriods");

                    gsm.Property(r => r.PeriodDurationMinutes)
                        .HasColumnName("TournamentRules_GroupStage_PeriodDurationMinutes");

                    gsm.Property(r => r.AllowOvertime)
                        .HasColumnName("TournamentRules_GroupStage_AllowOvertime");

                    gsm.Property(r => r.OvertimeDurationMinutes)
                        .HasColumnName("TournamentRules_GroupStage_OvertimeDurationMinutes");

                    gsm.Property(r => r.AllowShootout)
                        .HasColumnName("TournamentRules_GroupStage_AllowShootout");
                });

                tr.OwnsOne(r => r.PlayoffMatchRules, pm =>
                {
                    pm.Property(r => r.NumberOfPeriods)
                        .HasColumnName("TournamentRules_Playoff_NumberOfPeriods");

                    pm.Property(r => r.PeriodDurationMinutes)
                        .HasColumnName("TournamentRules_Playoff_PeriodDurationMinutes");

                    pm.Property(r => r.AllowOvertime)
                        .HasColumnName("TournamentRules_Playoff_AllowOvertime");

                    pm.Property(r => r.OvertimeDurationMinutes)
                        .HasColumnName("TournamentRules_Playoff_OvertimeDurationMinutes");

                    pm.Property(r => r.AllowShootout)
                        .HasColumnName("TournamentRules_Playoff_AllowShootout");
                });

                tr.Property(r => r.TeamsAdvancingPerGroup)
                    .HasColumnName("TournamentRules_TeamsAdvancingPerGroup");

                tr.Property(r => r.HasPlayoffStage)
                    .HasColumnName("TournamentRules_HasPlayoffStage");

                tr.Property(r => r.HasThirdPlaceMatch)
                    .HasColumnName("TournamentRules_HasThirdPlaceMatch");
            });

            builder.HasMany(t => t.Groups)
                .WithOne()
                .HasForeignKey(g => g.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
