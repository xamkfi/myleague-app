using Domain.Entities.Floorball;
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

            // Configure relationship with home team
            builder.HasOne(m => m.HomeTeam)
                .WithMany()
                .HasForeignKey(m => m.HomeTeamId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Configure relationship with away team
            builder.HasOne(m => m.AwayTeam)
                .WithMany()
                .HasForeignKey(m => m.AwayTeamId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Configure relationship with season
            builder.HasOne(m => m.Season)
                .WithMany(s => s.Matches)
                .HasForeignKey(m => m.SeasonId)
                .IsRequired();

            // Configure the relationship with referees
            builder.HasMany(m => m.Officials)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "FloorballMatchOfficial",
                    j => j.HasOne<FloorballReferee>().WithMany().HasForeignKey("OfficialsId"),
                    j => j.HasOne<FloorballMatch>().WithMany().HasForeignKey("MatchesId")
                );

            // Configure owned types for period scores
            builder.OwnsMany(m => m.PeriodScores, periodBuilder =>
            {
                periodBuilder.WithOwner().HasForeignKey("MatchId");
                periodBuilder.Property<int>("Id").ValueGeneratedOnAdd();
                periodBuilder.HasKey("Id");

                periodBuilder.Property(p => p.PeriodNumber)
                    .IsRequired();

                periodBuilder.Property(p => p.HomeScore)
                    .IsRequired();

                periodBuilder.Property(p => p.AwayScore)
                    .IsRequired();
            });

            // Configure owned types for match events
            builder.OwnsMany(m => m.Events, eventsBuilder =>
            {
                eventsBuilder.WithOwner().HasForeignKey("MatchId");
                eventsBuilder.Property<Guid>("Id");
                eventsBuilder.HasKey("Id");

                eventsBuilder.Property(e => e.MatchId).IsRequired();
                eventsBuilder.Property(e => e.TeamId).IsRequired();
                eventsBuilder.Property(e => e.PeriodNumber).IsRequired();
                eventsBuilder.Property(e => e.TimeInSeconds).IsRequired();
                eventsBuilder.Property(e => e.Description).HasMaxLength(500);

                // Configure discriminator for TPH mapping with string
                eventsBuilder.Property<string>("EventType").HasMaxLength(50).IsRequired();
                
                // Configure the event types
                eventsBuilder.HasData(
                    new { EventType = "Goal", Discriminator = nameof(FloorballGoalEvent) },
                    new { EventType = "Penalty", Discriminator = nameof(FloorballPenaltyEvent) }
                );

                // Configure properties for goal events
                eventsBuilder.OwnsOne<FloorballGoalEvent>("", goalConfig =>
                {
                    goalConfig.Property(g => g.ScoringPlayerId).HasColumnName("ScoringPlayerId");
                    goalConfig.Property(g => g.AssistingPlayerId).HasColumnName("AssistingPlayerId");
                    goalConfig.Property(g => g.GoalTypeId).HasColumnName("GoalTypeId");
                });

                // Configure properties for penalty events
                eventsBuilder.OwnsOne<FloorballPenaltyEvent>("", penaltyConfig =>
                {
                    penaltyConfig.Property(p => p.PlayerId).HasColumnName("PlayerId");
                    penaltyConfig.Property(p => p.PenaltyTypeId).HasColumnName("PenaltyTypeId");
                    penaltyConfig.Property(p => p.PenaltyMinutes).HasColumnName("PenaltyMinutes");
                });
            });
        }
    }
} 