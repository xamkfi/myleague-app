using Domain.Entities.Floorball.Competitions;
using Domain.Enums.Common;
using Domain.Services.Common;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball;

public class FloorballSeasonConfiguration : IEntityTypeConfiguration<FloorballSeason>
{
    public void Configure(EntityTypeBuilder<FloorballSeason> builder)
    {
        builder.Property(season => season.TeamsAdvancing)
            .HasDefaultValue(0)
            .IsRequired();

        builder.OwnsOne(season => season.StandingRules, rules =>
        {
            rules.Property(rule => rule.WinPoints)
                .HasColumnName("StandingRules_WinPoints")
                .IsRequired()
                .HasDefaultValue(3)
                .ValueGeneratedNever();
            rules.Property(rule => rule.DrawPoints)
                .HasColumnName("StandingRules_DrawPoints")
                .IsRequired()
                .HasDefaultValue(1)
                .ValueGeneratedNever();
            rules.Property(rule => rule.OvertimeWinPoints)
                .HasColumnName("StandingRules_OvertimeWinPoints")
                .IsRequired()
                .HasDefaultValue(2)
                .ValueGeneratedNever();
            rules.Property(rule => rule.OvertimeLossPoints)
                .HasColumnName("StandingRules_OvertimeLossPoints")
                .IsRequired()
                .HasDefaultValue(1)
                .ValueGeneratedNever();
        });

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
