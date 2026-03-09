using Domain.Entities.Floorball.Tournament;
using Domain.ValueObjects.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballTournament entity.
    /// </summary>
    public class FloorballTournamentConfiguration : IEntityTypeConfiguration<FloorballTournament>
    {
        public void Configure(EntityTypeBuilder<FloorballTournament> builder)
        {
            builder.ToTable("FloorballTournaments", "floorball");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.DescriptionHtml)
                .HasColumnType("text");

            builder.Property(t => t.StartDate)
                .IsRequired();

            builder.Property(t => t.EndDate)
                .IsRequired();

            builder.Property(t => t.Location)
                .HasMaxLength(200);

            builder.Property(t => t.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(t => t.PlayoffFormat)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(t => t.GroupStageAdvancingCount)
                .IsRequired()
                .HasDefaultValue(1);

            // ImageUrls: JSON-serialised URI collection (same pattern as NewsArticle)
            builder.Property(t => t.ImageUrls)
                .HasConversion(
                    v => JsonSerializer.Serialize(v.Select(uri => uri.ToString()), (JsonSerializerOptions?)null),
                    v => (JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()).Select(s => new Uri(s)).ToList())
                .HasColumnType("text");

            // MatchRules as owned entity (same pattern as Season/Match)
            builder.OwnsOne(t => t.MatchRules, rules =>
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

            // One-to-many: Tournament -> Groups
            builder.HasMany(t => t.Groups)
                .WithOne(g => g.Tournament)
                .HasForeignKey(g => g.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(t => t.Groups)
                .HasField("_groups");

            // One-to-many: Tournament -> Matches (inverse configured in FloorballMatchConfiguration)
            builder.Navigation(t => t.Matches)
                .HasField("_matches");

            // Indexes
            builder.HasIndex(t => t.Status)
                .HasDatabaseName("IX_FloorballTournaments_Status");

            builder.HasIndex(t => t.StartDate)
                .HasDatabaseName("IX_FloorballTournaments_StartDate");
        }
    }
}
