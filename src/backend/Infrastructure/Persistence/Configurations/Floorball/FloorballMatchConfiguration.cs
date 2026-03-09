using Domain.Entities.Floorball;
using Domain.Entities.Floorball.Tournament;
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

            // Configure foreign key relationships with navigation properties within the same context
            builder.Property(m => m.HomeTeamId)
                .IsRequired();

            builder.Property(m => m.AwayTeamId)
                .IsRequired();

            // SeasonId is nullable (null for tournament matches)
            builder.Property(m => m.SeasonId)
                .IsRequired(false);

            // TournamentId is nullable (null for season matches)
            builder.Property(m => m.TournamentId)
                .IsRequired(false);

            builder.Property(m => m.TournamentGroupId)
                .IsRequired(false);

            builder.Property(m => m.TournamentRound)
                .IsRequired(false)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Ignore computed convenience properties
            builder.Ignore(m => m.IsTournamentMatch);
            builder.Ignore(m => m.IsSeasonMatch);

            // Season relationship (optional for tournament matches)
            builder.HasOne(m => m.Season)
                .WithMany(s => s.Matches)
                .HasForeignKey(m => m.SeasonId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // Tournament relationship (optional for season matches)
            builder.HasOne(m => m.Tournament)
                .WithMany(t => t.Matches)
                .HasForeignKey(m => m.TournamentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // Tournament group relationship (optional)
            builder.HasOne(m => m.TournamentGroup)
                .WithMany()
                .HasForeignKey(m => m.TournamentGroupId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(m => m.HomeTeam)
                .WithMany()
                .HasForeignKey(m => m.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.AwayTeam)
                .WithMany()
                .HasForeignKey(m => m.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // Check constraint: match must belong to either a season or a tournament
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_FloorballMatches_SeasonOrTournament",
                "\"SeasonId\" IS NOT NULL OR \"TournamentId\" IS NOT NULL"));

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
                .EnableLazyLoading(false);        // optional
        }
    }
} 
