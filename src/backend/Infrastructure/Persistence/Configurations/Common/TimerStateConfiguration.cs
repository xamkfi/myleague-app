using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Common
{
    /// <summary>
    /// Configuration for the TimerState entity
    /// </summary>
    public class TimerStateConfiguration : IEntityTypeConfiguration<TimerState>
    {
        /// <summary>
        /// Configures the TimerState entity
        /// </summary>
        /// <param name="builder">The entity type builder</param>
        public void Configure(EntityTypeBuilder<TimerState> builder)
        {
            // Table name and schema
            builder.ToTable("TimerStates", "common");

            // Primary key
            builder.HasKey(t => t.MatchId);

            // Properties
            builder.Property(t => t.MatchId)
                .IsRequired();

            builder.Property(t => t.PeriodNumber)
                .IsRequired(false);

            builder.Property(t => t.StartedAt)
                .IsRequired(false);

            builder.Property(t => t.PausedAt)
                .IsRequired(false);

            builder.Property(t => t.TotalPausedDuration)
                .IsRequired()
                .HasConversion(
                    v => v.Ticks,
                    v => TimeSpan.FromTicks(v));

            builder.Property(t => t.IsRunning)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(t => t.LastUpdated)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Indexes
            builder.HasIndex(t => t.IsRunning)
                .HasDatabaseName("IX_TimerStates_IsRunning");

            builder.HasIndex(t => t.LastUpdated)
                .HasDatabaseName("IX_TimerStates_LastUpdated");
        }
    }
} 