using Domain.Entities.Football.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Football;

public class FootballPeriodScoreConfiguration : BaseEntityConfiguration<FootballPeriodScore>
{
    protected override void ConfigureEntity(EntityTypeBuilder<FootballPeriodScore> builder)
    {
        builder.ToTable("FootballPeriodScores");
        builder.Property(p => p.MatchId).IsRequired();
        builder.Property(p => p.PeriodNumber).IsRequired();
        builder.Property(p => p.HomeTeamId).IsRequired();
        builder.Property(p => p.AwayTeamId).IsRequired();
        builder.Property(p => p.HomeScore).IsRequired().HasDefaultValue(0);
        builder.Property(p => p.AwayScore).IsRequired().HasDefaultValue(0);
        builder.Property(p => p.IsCompleted).IsRequired().HasDefaultValue(false);
        builder.HasOne<FootballMatch>().WithMany(m => m.PeriodScores).HasForeignKey(p => p.MatchId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(p => new { p.MatchId, p.PeriodNumber }).IsUnique().HasDatabaseName("IX_FootballPeriodScore_Match_Period");
    }
}

public class FootballMatchLineupPlayerConfiguration : BaseEntityConfiguration<FootballMatchLineupPlayer>
{
    protected override void ConfigureEntity(EntityTypeBuilder<FootballMatchLineupPlayer> builder)
    {
        builder.ToTable("FootballMatchLineupPlayers");
        builder.Property(p => p.MatchId).IsRequired();
        builder.Property(p => p.TeamId).IsRequired();
        builder.Property(p => p.PlayerId).IsRequired();
        builder.Property(p => p.Position).IsRequired().HasConversion<string>().HasMaxLength(32);
        builder.Property(p => p.IsOnField).IsRequired();
        builder.Property(p => p.IsSentOff).IsRequired();
        builder.HasOne<FootballMatch>().WithMany(m => m.Lineup).HasForeignKey(p => p.MatchId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Domain.Entities.Football.Teams.FootballPlayer>().WithMany().HasForeignKey(p => p.PlayerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(p => new { p.MatchId, p.TeamId, p.PlayerId }).IsUnique().HasDatabaseName("IX_FootballMatchLineupPlayer_Match_Team_Player");
    }
}
