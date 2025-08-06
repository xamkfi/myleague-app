using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the EventSourcedFloorballMatch entity.
    /// </summary>
    public class EventSourcedFloorballMatchConfiguration : IEntityTypeConfiguration<EventSourcedFloorballMatch>
    {
        /// <summary>
        /// Configures the entity mapping for EventSourcedFloorballMatch.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<EventSourcedFloorballMatch> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.SeasonId)
                .IsRequired();

            builder.Property(m => m.HomeTeamId)
                .IsRequired();

            builder.Property(m => m.AwayTeamId)
                .IsRequired();

            builder.Property(m => m.ScheduledDateTime)
                .IsRequired();

            builder.Property(m => m.Venue)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(m => m.Status)
                .IsRequired()
                .HasConversion<string>();
            
            builder.Property(m => m.HomeScore)
                .IsRequired();
                
            builder.Property(m => m.AwayScore)
                .IsRequired();
                
            builder.Property(m => m.WentToOvertime)
                .IsRequired();
                
            builder.Property(m => m.WentToShootout)
                .IsRequired();

            builder.Property(m => m.Version)
                .IsRequired();

            // Foreign key relationships
            builder.HasOne<FloorballSeason>()
                .WithMany()
                .HasForeignKey(m => m.SeasonId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne<FloorballTeam>()
                .WithMany()
                .HasForeignKey(m => m.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne<FloorballTeam>()
                .WithMany()
                .HasForeignKey(m => m.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Store officials as a serialized JSON string - this will be managed by the domain model
            builder.Property<string>("OfficialIdsJson")
                .HasColumnType("text");

            // Event records will be handled by specialized storage
            // Consider moving them to a separate table or document store
            builder.Ignore(m => m.GoalEvents);
            builder.Ignore(m => m.PenaltyEvents);
            builder.Ignore(m => m.OfficialIds);
            builder.Ignore(m => m.PeriodScores);
            builder.Ignore(m => m.UncommittedEvents);
            builder.Ignore(m => m.SaveEvents);
        }
    }
} 