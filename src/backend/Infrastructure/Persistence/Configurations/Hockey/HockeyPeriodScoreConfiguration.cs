using Domain.Entities.Hockey.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyPeriodScoreConfiguration : BaseEntityConfiguration<HockeyPeriodScore>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyPeriodScore> builder)
    {
        builder.ToTable("HockeyPeriodScores");

        builder.Property(p => p.MatchId).IsRequired();
        builder.Property(p => p.PeriodNumber).IsRequired();
        builder.Property(p => p.PeriodType).IsRequired().HasConversion<string>();
        builder.Property(p => p.HomeMatchTeamId).IsRequired();
        builder.Property(p => p.AwayMatchTeamId).IsRequired();
        builder.Property(p => p.HomeGoals).IsRequired();
        builder.Property(p => p.AwayGoals).IsRequired();
        builder.Property(p => p.HomeShots).IsRequired();
        builder.Property(p => p.AwayShots).IsRequired();
        builder.Property(p => p.HomeFaceoffWins).IsRequired();
        builder.Property(p => p.AwayFaceoffWins).IsRequired();
        builder.Property(p => p.IsCompleted).IsRequired();

        builder.HasOne(p => p.HomeMatchTeam)
            .WithMany()
            .HasForeignKey(p => p.HomeMatchTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.AwayMatchTeam)
            .WithMany()
            .HasForeignKey(p => p.AwayMatchTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.MatchId, p.PeriodNumber })
            .IsUnique()
            .HasDatabaseName("IX_HockeyPeriodScores_Match_Period");
    }
}
