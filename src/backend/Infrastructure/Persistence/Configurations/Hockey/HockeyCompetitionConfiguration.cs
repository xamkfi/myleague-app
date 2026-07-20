using System.Text.Json;
using Domain.Entities.Hockey.Competitions;
using Domain.Enums.Hockey.Competitions;
using Domain.ValueObjects.Hockey.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyCompetitionConfiguration : IEntityTypeConfiguration<HockeyCompetition>
{
    private static readonly JsonSerializerOptions PlayoffSlotJsonOptions = new() { WriteIndented = false };

    public void Configure(EntityTypeBuilder<HockeyCompetition> builder)
    {
        builder.ToTable("HockeyCompetitions");

        builder.HasKey(c => c.Id);

        builder.HasDiscriminator(c => c.CompetitionType)
            .HasValue<HockeySeason>(HockeyCompetitionType.Season)
            .HasValue<HockeyTournament>(HockeyCompetitionType.Tournament);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.StartDate).IsRequired();
        builder.Property(c => c.EndDate).IsRequired();
        builder.Property(c => c.Status).IsRequired().HasConversion<string>();

        builder.Ignore(c => c.IsActive);
        builder.Ignore(c => c.IsCompleted);

        builder.OwnsOne(c => c.CompetitionRules, rules =>
            HockeyCompetitionRulesOwnedConfiguration.ConfigureCompetitionRules(rules, "CompetitionRules"));

        builder.HasMany(c => c.Teams)
            .WithOne(t => t.Competition)
            .HasForeignKey(t => t.CompetitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Matches)
            .WithOne(m => m.Competition)
            .HasForeignKey(m => m.CompetitionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Divisions)
            .WithOne(d => d.Competition)
            .HasForeignKey(d => d.CompetitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.PlayoffSeries)
            .WithOne(s => s.Competition)
            .HasForeignKey(s => s.CompetitionId)
            .OnDelete(DeleteBehavior.Cascade);

        ValueConverter<List<HockeyPlayoffScheduleSlot>, string> playoffScheduleConverter = new(
            slots => JsonSerializer.Serialize(slots ?? new List<HockeyPlayoffScheduleSlot>(), PlayoffSlotJsonOptions),
            json => string.IsNullOrWhiteSpace(json)
                ? new List<HockeyPlayoffScheduleSlot>()
                : JsonSerializer.Deserialize<List<HockeyPlayoffScheduleSlot>>(json, PlayoffSlotJsonOptions) ?? new List<HockeyPlayoffScheduleSlot>());

        ValueComparer<List<HockeyPlayoffScheduleSlot>> playoffScheduleComparer = new(
            (a, b) => a == null && b == null || a != null && b != null && a.SequenceEqual(b),
            slots => slots == null ? 0 : slots.Aggregate(0, (hash, slot) => HashCode.Combine(hash, slot.GetHashCode())),
            slots => slots == null ? new List<HockeyPlayoffScheduleSlot>() : slots.ToList());

        builder.Property<List<HockeyPlayoffScheduleSlot>>("_playoffSchedule")
            .HasField("_playoffSchedule")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("PlayoffSchedule")
            .HasColumnType("jsonb")
            .IsRequired(false)
            .HasConversion(playoffScheduleConverter)
            .Metadata.SetValueComparer(playoffScheduleComparer);
    }
}
