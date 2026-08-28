using Domain.Entities.Hockey.Competitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyPlayoffSeriesConfiguration : BaseEntityConfiguration<HockeyPlayoffSeries>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyPlayoffSeries> builder)
    {
        builder.ToTable("HockeyPlayoffSeries");

        builder.Property(s => s.CompetitionId).IsRequired();
        builder.Property(s => s.Round).IsRequired().HasConversion<string>();
        builder.Property(s => s.SeriesOrder).IsRequired();
        builder.Property(s => s.BestOf).IsRequired();
        builder.Property(s => s.Status).IsRequired().HasConversion<string>();

        builder.HasOne(s => s.HomeCompetitionTeam)
            .WithMany()
            .HasForeignKey(s => s.HomeCompetitionTeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.AwayCompetitionTeam)
            .WithMany()
            .HasForeignKey(s => s.AwayCompetitionTeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.WinnerCompetitionTeam)
            .WithMany()
            .HasForeignKey(s => s.WinnerCompetitionTeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(s => new { s.CompetitionId, s.Round, s.SeriesOrder })
            .IsUnique()
            .HasDatabaseName("IX_HockeyPlayoffSeries_Competition_Round_Order");
    }
}
