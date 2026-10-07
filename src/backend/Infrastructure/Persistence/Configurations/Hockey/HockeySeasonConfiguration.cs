using Domain.Entities.Hockey.Competitions;
using Domain.Enums.Common;
using Domain.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeySeasonConfiguration : IEntityTypeConfiguration<HockeySeason>
{
    public void Configure(EntityTypeBuilder<HockeySeason> builder)
    {
        builder.Property(s => s.SeasonCode).HasMaxLength(50);
        builder.Property(s => s.ChampionCompetitionTeamId);
        builder.Property(s => s.TeamsAdvancing)
            .HasDefaultValue(0)
            .IsRequired();

        // The domain edits this list in place, so EF needs a comparer that looks at the contents.
        ValueComparer<List<StandingSortCriterion>> rankingCriteriaComparer = new(
            (a, b) => a == null && b == null || a != null && b != null && a.SequenceEqual(b),
            criteria => criteria == null ? 0 : criteria.Aggregate(0, (hash, item) => HashCode.Combine(hash, item)),
            criteria => criteria == null ? new List<StandingSortCriterion>() : criteria.ToList());

        builder.Property<List<StandingSortCriterion>>("_rankingCriteria")
            .HasColumnName("RankingCriteria")
            .HasMaxLength(64)
            .IsRequired()
            .HasDefaultValueSql("'0,1,2,4,5'")
            .HasConversion(
                criteria => string.Join(',', criteria.Select(item => ((int)item).ToString())),
                stored => string.IsNullOrWhiteSpace(stored)
                    ? StandingSortCriteria.Default.ToList()
                    : stored.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(item => (StandingSortCriterion)int.Parse(item))
                        .ToList())
            .Metadata.SetValueComparer(rankingCriteriaComparer);

        builder.HasMany(season => season.ContentBlocks)
            .WithOne()
            .HasForeignKey(block => block.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(season => season.ContentBlocks)
            .HasField("_contentBlocks")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
