using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.ValueObjects.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballMatch entity.
    /// </summary>
    public class FloorballMatchConfiguration : IEntityTypeConfiguration<FloorballMatch>
    {
        /// <summary>
        /// Configures the entity mapping for FloorballMatch.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<FloorballMatch> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.ScheduledDateTime)
                .IsRequired();

            builder.Property(m => m.Venue)
                .HasMaxLength(200);

            builder.Property(m => m.HomeScore)
                .IsRequired();

            builder.Property(m => m.AwayScore)
                .IsRequired();

            builder.Property(m => m.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(m => m.WentToOvertime)
                .IsRequired();

            builder.Property(m => m.WentToShootout)
                .IsRequired();

            // Configure MatchRules as an owned entity (stored as columns in the same table)
            builder.OwnsOne(m => m.MatchRules, rules =>
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

            // Home and away team IDs are nullable so future fixtures can be published before the
            // participants are known (e.g. season league round 12 announced in August, or playoff
            // slots whose feeder match hasn't completed yet). Start() in the domain layer enforces
            // that both slots are filled before a match can transition to InProgress.
            builder.Property(m => m.HomeTeamId)
                .IsRequired(false);

            builder.Property(m => m.AwayTeamId)
                .IsRequired(false);

            builder.Property(m => m.CompetitionId)
                .IsRequired();

            // Configure relationships within FloorballDbContext
            builder.HasOne(m => m.Competition)
                .WithMany(s => s.Matches)
                .HasForeignKey(m => m.CompetitionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.HomeTeam)
                .WithMany()
                .HasForeignKey(m => m.HomeTeamId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.AwayTeam)
                .WithMany()
                .HasForeignKey(m => m.AwayTeamId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure the relationship with referees using a simple many-to-many join table
            builder.HasMany(m => m.Officials)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "FloorballMatchOfficial",
                    j => j.HasOne<FloorballReferee>().WithMany().HasForeignKey("OfficialsId"),
                    j => j.HasOne<FloorballMatch>().WithMany().HasForeignKey("MatchesId")
                );

            // Configure relationship with PeriodScores - they are now separate entities
            builder.HasMany(m => m.PeriodScores)
                .WithOne()
                .HasForeignKey(p => p.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the backing field for PeriodScores so EF can populate it properly
            builder.Navigation(m => m.PeriodScores)
                .HasField("_periodScores")
                .EnableLazyLoading(false);

            // Configure relationship with active players (per-match field player lineup)
            builder.HasMany(m => m.ActivePlayers)
                .WithOne()
                .HasForeignKey(p => p.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(m => m.ActivePlayers)
                .HasField("_activePlayers")
                .EnableLazyLoading(false);

            builder.Ignore(m => m.HomeActivePlayerIds);
            builder.Ignore(m => m.AwayActivePlayerIds);

            // Ignore computed collections so EF doesn't treat them as navigations
            builder.Ignore(m => m.GoalEvents);
            builder.Ignore(m => m.PenaltyEvents);
            builder.Ignore(m => m.SaveEvents);

            builder.HasMany(m => m.Events)
               .WithOne()                        // no back-link on the event entity
               .HasForeignKey("MatchId")         // ← Explicitly specify the column name
               .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(m => m.Events)
                .HasField("_events")
                .EnableLazyLoading(false);

            builder.Property(m => m.TournamentStage)
                .HasConversion<int?>()
                .IsRequired(false);

            builder.Property(m => m.TournamentGroupId)
                .IsRequired(false);

            builder.HasOne<FloorballTournamentGroup>()
                .WithMany()
                .HasForeignKey(m => m.TournamentGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // Playoff bracket fields
            builder.Property(m => m.PlayoffRound)
                .HasConversion<int?>()
                .IsRequired(false);

            builder.Property(m => m.PlayoffMatchOrder)
                .IsRequired(false);

            builder.Property(m => m.NextMatchId)
                .IsRequired(false);

            builder.Property(m => m.NextMatchSlot)
                .HasConversion<int?>()
                .IsRequired(false);

            // NextMatch is a self-reference inside the bracket. We don't expose a navigation property
            // to keep the entity lean — the application layer fetches the next match by id when needed.
            // Use Restrict to avoid cascade-delete cycles.
            builder.HasOne<FloorballMatch>()
                .WithMany()
                .HasForeignKey(m => m.NextMatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 
