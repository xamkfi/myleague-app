using System.Text.Json;
using Domain.Entities.Football.Competitions;
using Domain.Enums.Football;
using Domain.ValueObjects.Football;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MyLeague.Infrastructure.Persistence.Configurations.Football;

public class FootballTournamentConfiguration : IEntityTypeConfiguration<FootballTournament>
{
    private static readonly JsonSerializerOptions PlayoffSlotJsonOptions = new() { WriteIndented = false };

    public void Configure(EntityTypeBuilder<FootballTournament> builder)
    {
        builder.Property(t => t.ContentHtml).HasMaxLength(50000);
        builder.Property(t => t.Venue).HasMaxLength(200);
        builder.Property(t => t.TournamentStatus).HasConversion<int>();
        builder.Property(t => t.ChampionTeamId).IsRequired(false);

        builder.OwnsOne(t => t.TournamentRules, tr =>
        {
            tr.OwnsOne(r => r.GroupStageMatchRules, gsm => FootballMatchRulesMapping.Map(gsm, "TournamentRules_GroupStage_"));
            tr.OwnsOne(r => r.PlayoffMatchRules, pm => FootballMatchRulesMapping.Map(pm, "TournamentRules_Playoff_"));
            tr.Property(r => r.TeamsAdvancingPerGroup).HasColumnName("TournamentRules_TeamsAdvancingPerGroup");
            tr.Property(r => r.HasPlayoffStage).HasColumnName("TournamentRules_HasPlayoffStage");
            tr.Property(r => r.HasThirdPlaceMatch).HasColumnName("TournamentRules_HasThirdPlaceMatch");
        });

        builder.HasMany(t => t.Groups).WithOne().HasForeignKey(g => g.TournamentId).OnDelete(DeleteBehavior.Cascade);

        ValueConverter<List<FootballPlayoffScheduleSlot>, string> playoffScheduleConverter = new(
            slots => JsonSerializer.Serialize(
                (slots ?? new List<FootballPlayoffScheduleSlot>()).Select(PlayoffScheduleSlotJsonRow.From).ToList(),
                PlayoffSlotJsonOptions),
            json => string.IsNullOrWhiteSpace(json)
                ? new List<FootballPlayoffScheduleSlot>()
                : DeserializeSlots(json));

        ValueComparer<List<FootballPlayoffScheduleSlot>> playoffScheduleComparer = new(
            (a, b) => a == null && b == null || a != null && b != null && a.SequenceEqual(b),
            slots => slots == null ? 0 : slots.Aggregate(0, (hash, slot) => HashCode.Combine(hash, slot)),
            slots => slots == null ? new List<FootballPlayoffScheduleSlot>() : slots.ToList());

        builder.Property<List<FootballPlayoffScheduleSlot>>("_playoffSchedule")
            .HasField("_playoffSchedule")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("PlayoffSchedule")
            .HasColumnType("jsonb")
            .IsRequired(false)
            .HasConversion(playoffScheduleConverter)
            .Metadata.SetValueComparer(playoffScheduleComparer);
    }

    private sealed record PlayoffScheduleSlotJsonRow(
        FootballPlayoffRound Round,
        int Order,
        DateTime ScheduledDateTime,
        string? Venue)
    {
        public static PlayoffScheduleSlotJsonRow From(FootballPlayoffScheduleSlot slot) =>
            new(slot.Round, slot.Order, slot.ScheduledDateTime, slot.Venue);

        public FootballPlayoffScheduleSlot ToValueObject() =>
            new(Round, Order, DateTime.SpecifyKind(ScheduledDateTime, DateTimeKind.Utc), Venue);
    }

    private static List<FootballPlayoffScheduleSlot> DeserializeSlots(string json)
    {
        List<PlayoffScheduleSlotJsonRow>? rows = JsonSerializer.Deserialize<List<PlayoffScheduleSlotJsonRow>>(json, PlayoffSlotJsonOptions);
        if (rows == null || rows.Count == 0)
            return new List<FootballPlayoffScheduleSlot>();
        return rows.Select(r => r.ToValueObject()).ToList();
    }
}
