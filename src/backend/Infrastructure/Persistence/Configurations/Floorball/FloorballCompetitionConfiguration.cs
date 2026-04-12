using Domain.Entities.Floorball;
using Domain.ValueObjects.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballCompetition entity hierarchy (TPH).
    /// </summary>
    public class FloorballCompetitionConfiguration : IEntityTypeConfiguration<FloorballCompetition>
    {
        /// <summary>
        /// Configures the entity mapping for FloorballCompetition.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
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

            // Tournament-specific TPH columns (nullable because not all competition rows are tournaments)
            builder.Property<string?>("ContentHtml")
                .HasMaxLength(50000)
                .IsRequired(false);

            builder.Property<string?>("Venue")
                .HasMaxLength(200)
                .IsRequired(false);

            builder.Property<int?>("TournamentStatus")
                .IsRequired(false);

            // TournamentRules owned entity for FloorballTournament
            builder.OwnsOne<FloorballTournamentRules>("TournamentRules", tr =>
            {
                tr.OwnsOne(r => r.GroupStageMatchRules, gsm =>
                {
                    gsm.Property(r => r.NumberOfPeriods)
                        .HasColumnName("TournamentRules_GroupStage_NumberOfPeriods")
                        .IsRequired(false);

                    gsm.Property(r => r.PeriodDurationMinutes)
                        .HasColumnName("TournamentRules_GroupStage_PeriodDurationMinutes")
                        .IsRequired(false);

                    gsm.Property(r => r.AllowOvertime)
                        .HasColumnName("TournamentRules_GroupStage_AllowOvertime")
                        .IsRequired(false);

                    gsm.Property(r => r.OvertimeDurationMinutes)
                        .HasColumnName("TournamentRules_GroupStage_OvertimeDurationMinutes")
                        .IsRequired(false);

                    gsm.Property(r => r.AllowShootout)
                        .HasColumnName("TournamentRules_GroupStage_AllowShootout")
                        .IsRequired(false);
                });

                tr.OwnsOne(r => r.PlayoffMatchRules, pm =>
                {
                    pm.Property(r => r.NumberOfPeriods)
                        .HasColumnName("TournamentRules_Playoff_NumberOfPeriods")
                        .IsRequired(false);

                    pm.Property(r => r.PeriodDurationMinutes)
                        .HasColumnName("TournamentRules_Playoff_PeriodDurationMinutes")
                        .IsRequired(false);

                    pm.Property(r => r.AllowOvertime)
                        .HasColumnName("TournamentRules_Playoff_AllowOvertime")
                        .IsRequired(false);

                    pm.Property(r => r.OvertimeDurationMinutes)
                        .HasColumnName("TournamentRules_Playoff_OvertimeDurationMinutes")
                        .IsRequired(false);

                    pm.Property(r => r.AllowShootout)
                        .HasColumnName("TournamentRules_Playoff_AllowShootout")
                        .IsRequired(false);
                });

                tr.Property(r => r.TeamsAdvancingPerGroup)
                    .HasColumnName("TournamentRules_TeamsAdvancingPerGroup")
                    .IsRequired(false);

                tr.Property(r => r.HasPlayoffStage)
                    .HasColumnName("TournamentRules_HasPlayoffStage")
                    .IsRequired(false);

                tr.Property(r => r.HasThirdPlaceMatch)
                    .HasColumnName("TournamentRules_HasThirdPlaceMatch")
                    .IsRequired(false);
            });

            // Configure the Groups navigation for FloorballTournament
            builder.HasMany<FloorballTournamentGroup>("Groups")
                .WithOne()
                .HasForeignKey(g => g.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
