using Domain.Entities.Hockey.Competitions;
using Domain.Enums.Common;
using Domain.Services.Common;
using Microsoft.EntityFrameworkCore;
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
                        .ToList());

        builder.HasMany(season => season.ContentBlocks)
            .WithOne()
            .HasForeignKey(block => block.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(season => season.ContentBlocks)
            .HasField("_contentBlocks")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
