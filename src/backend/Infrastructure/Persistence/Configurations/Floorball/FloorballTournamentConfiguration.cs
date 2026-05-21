using System.Text.Json;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.ValueObjects.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballTournament entity.
    /// Handles tournament-specific properties in the TPH FloorballCompetition hierarchy.
    /// Shared properties (Name, StartDate, MatchRules, Teams, etc.) are configured in <see cref="FloorballCompetitionConfiguration"/>.
    /// </summary>
    public class FloorballTournamentConfiguration : IEntityTypeConfiguration<FloorballTournament>
    {
        // Serializer settings shared by all conversion calls. JSON is stored as-is in the
        // single `PlayoffSchedule` column; the column is nullable so older tournaments
        // without a pre-defined schedule see NULL.
        private static readonly JsonSerializerOptions PlayoffSlotJsonOptions = new()
        {
            // Keep payload terse — these rows aren't huge but there's no point pretty-printing.
            WriteIndented = false,
        };

        public void Configure(EntityTypeBuilder<FloorballTournament> builder)
        {
            builder.Property(t => t.ContentHtml)
                .HasMaxLength(50000);

            builder.Property(t => t.Venue)
                .HasMaxLength(200);

            builder.Property(t => t.TournamentStatus)
                .HasConversion<int>();

            builder.Property(t => t.ChampionTeamId)
                .IsRequired(false);

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

            // Pre-defined playoff schedule slots. Persisted as a single JSON column instead of
            // a child table because the data is small (max ~8 rows per tournament), only ever
            // read as a whole, and the slot order/round set is fully fixed by the bracket
            // builder. A JSON blob keeps queries cheap and avoids an extra join in the
            // tournament-detail read path. Old rows are NULL (no schedule) → empty list.
            ValueConverter<List<PlayoffScheduleSlot>, string> playoffScheduleConverter = new(
                slots => JsonSerializer.Serialize(
                    (slots ?? new List<PlayoffScheduleSlot>()).Select(PlayoffScheduleSlotJsonRow.From).ToList(),
                    PlayoffSlotJsonOptions),
                json => string.IsNullOrWhiteSpace(json)
                    ? new List<PlayoffScheduleSlot>()
                    : DeserializeSlots(json));

            ValueComparer<List<PlayoffScheduleSlot>> playoffScheduleComparer = new(
                (a, b) => a == null && b == null
                    || a != null && b != null && a.SequenceEqual(b),
                slots => slots == null ? 0 : slots.Aggregate(0, (hash, slot) => HashCode.Combine(hash, slot)),
                slots => slots == null ? new List<PlayoffScheduleSlot>() : slots.ToList());

            builder.Property<List<PlayoffScheduleSlot>>("_playoffSchedule")
                .HasField("_playoffSchedule")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("PlayoffSchedule")
                // PostgreSQL-native JSON column. `jsonb` over `json` because we never need to
                // preserve byte-for-byte input and `jsonb` enables indexed queries later if we
                // ever want to filter tournaments by scheduled playoff times.
                .HasColumnType("jsonb")
                .IsRequired(false)
                .HasConversion(playoffScheduleConverter)
                .Metadata.SetValueComparer(playoffScheduleComparer);
        }

        /// <summary>
        /// JSON-only serialization DTO. Avoids depending on the value object's private constructor /
        /// validation when reading back — invalid values are rejected at write time via the public
        /// constructor and the JSON storage is treated as already-validated.
        /// </summary>
        private sealed record PlayoffScheduleSlotJsonRow(
            FloorballPlayoffRound Round,
            int Order,
            DateTime ScheduledDateTime,
            string? Venue)
        {
            public static PlayoffScheduleSlotJsonRow From(PlayoffScheduleSlot slot) =>
                new(slot.Round, slot.Order, slot.ScheduledDateTime, slot.Venue);

            public PlayoffScheduleSlot ToValueObject() =>
                new(Round, Order, DateTime.SpecifyKind(ScheduledDateTime, DateTimeKind.Utc), Venue);
        }

        private static List<PlayoffScheduleSlot> DeserializeSlots(string json)
        {
            List<PlayoffScheduleSlotJsonRow>? rows = JsonSerializer.Deserialize<List<PlayoffScheduleSlotJsonRow>>(json, PlayoffSlotJsonOptions);
            if (rows == null || rows.Count == 0)
            {
                return new List<PlayoffScheduleSlot>();
            }
            return rows.Select(r => r.ToValueObject()).ToList();
        }
    }
}
