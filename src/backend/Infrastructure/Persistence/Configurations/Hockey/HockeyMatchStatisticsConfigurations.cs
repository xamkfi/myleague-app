using Domain.Entities.Hockey.Statistics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyMatchTeamStatisticsConfiguration : BaseEntityConfiguration<HockeyMatchTeamStatistics>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyMatchTeamStatistics> builder)
    {
        builder.ToTable("HockeyMatchTeamStatistics");

        builder.Property(s => s.MatchId).IsRequired();
        builder.Property(s => s.MatchTeamId).IsRequired();
        builder.Property(s => s.TeamId).IsRequired();
        builder.Property(s => s.GoalsFor).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.GoalsAgainst).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.ShotsOnGoal).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.ShotAttempts).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.MissedShots).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.BlockedShotAttempts).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.ShotPercentage).IsRequired().HasPrecision(5, 2).HasDefaultValue(0m);
        builder.Property(s => s.Saves).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.ShotsAgainst).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.TeamSavePercentage).IsRequired().HasPrecision(5, 2).HasDefaultValue(0m);
        builder.Property(s => s.FaceoffWins).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.FaceoffAttempts).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.FaceoffPercentage).IsRequired().HasPrecision(5, 2).HasDefaultValue(0m);
        builder.Property(s => s.PowerPlayOpportunities).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.PowerPlayGoals).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.PowerPlayPercentage).IsRequired().HasPrecision(5, 2).HasDefaultValue(0m);
        builder.Property(s => s.PenaltyKillOpportunities).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.PenaltyKillSuccesses).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.PenaltyKillPercentage).IsRequired().HasPrecision(5, 2).HasDefaultValue(0m);
        builder.Property(s => s.Penalties).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.PenaltyMinutes).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Hits).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.BlockedShots).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Takeaways).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Giveaways).IsRequired().HasDefaultValue(0);

        builder.HasOne(s => s.Match)
            .WithMany()
            .HasForeignKey(s => s.MatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.MatchTeam)
            .WithMany()
            .HasForeignKey(s => s.MatchTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Team)
            .WithMany()
            .HasForeignKey(s => s.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.MatchId, s.MatchTeamId })
            .IsUnique()
            .HasDatabaseName("IX_HockeyMatchTeamStatistics_Match_MatchTeam");
    }
}

public class HockeyMatchPlayerStatisticsConfiguration : BaseEntityConfiguration<HockeyMatchPlayerStatistics>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyMatchPlayerStatistics> builder)
    {
        builder.ToTable("HockeyMatchPlayerStatistics");

        builder.Property(s => s.MatchId).IsRequired();
        builder.Property(s => s.MatchTeamId).IsRequired();
        builder.Property(s => s.MatchActivePlayerId).IsRequired();
        builder.Property(s => s.TeamPlayerId).IsRequired();
        builder.Property(s => s.PlayerId).IsRequired();
        builder.Property(s => s.TeamId).IsRequired();
        builder.Property(s => s.GamesPlayed).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Goals).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Assists).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Points).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.PenaltyMinutes).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.PlusMinusRating).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.ShotsOnGoal).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.ShotAttempts).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.ShotPercentage).IsRequired().HasPrecision(5, 2).HasDefaultValue(0m);
        builder.Property(s => s.FaceoffWins).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.FaceoffAttempts).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.FaceoffPercentage).IsRequired().HasPrecision(5, 2).HasDefaultValue(0m);
        builder.Property(s => s.Hits).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.BlockedShots).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Takeaways).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Giveaways).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.TimeOnIceSeconds).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Shifts).IsRequired().HasDefaultValue(0);

        builder.HasOne(s => s.Match)
            .WithMany()
            .HasForeignKey(s => s.MatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.MatchTeam)
            .WithMany()
            .HasForeignKey(s => s.MatchTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.MatchActivePlayer)
            .WithMany()
            .HasForeignKey(s => s.MatchActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.TeamPlayer)
            .WithMany()
            .HasForeignKey(s => s.TeamPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Player)
            .WithMany()
            .HasForeignKey(s => s.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Team)
            .WithMany()
            .HasForeignKey(s => s.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.MatchId, s.MatchActivePlayerId })
            .IsUnique()
            .HasDatabaseName("IX_HockeyMatchPlayerStatistics_Match_ActivePlayer");
    }
}

public class HockeyGoalieMatchStatisticsConfiguration : BaseEntityConfiguration<HockeyGoalieMatchStatistics>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyGoalieMatchStatistics> builder)
    {
        builder.ToTable("HockeyGoalieMatchStatistics");

        builder.Property(s => s.MatchId).IsRequired();
        builder.Property(s => s.MatchTeamId).IsRequired();
        builder.Property(s => s.MatchActivePlayerId).IsRequired();
        builder.Property(s => s.TeamPlayerId).IsRequired();
        builder.Property(s => s.PlayerId).IsRequired();
        builder.Property(s => s.TeamId).IsRequired();
        builder.Property(s => s.WasStarter).IsRequired();
        builder.Property(s => s.Decision).IsRequired().HasConversion<string>();
        builder.Property(s => s.GamesPlayed).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.GamesStarted).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Wins).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Losses).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.OvertimeLosses).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.ShootoutLosses).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.NoDecisions).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Saves).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.ShotsAgainst).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.SavePercentage).IsRequired().HasPrecision(5, 2).HasDefaultValue(0m);
        builder.Property(s => s.GoalsAgainst).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.GoalsAgainstAverage).IsRequired().HasPrecision(4, 2).HasDefaultValue(0m);
        builder.Property(s => s.Shutouts).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.MinutesPlayed).IsRequired().HasDefaultValue(0);

        builder.HasOne(s => s.Match)
            .WithMany()
            .HasForeignKey(s => s.MatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.MatchTeam)
            .WithMany()
            .HasForeignKey(s => s.MatchTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.MatchActivePlayer)
            .WithMany()
            .HasForeignKey(s => s.MatchActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.TeamPlayer)
            .WithMany()
            .HasForeignKey(s => s.TeamPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Player)
            .WithMany()
            .HasForeignKey(s => s.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Team)
            .WithMany()
            .HasForeignKey(s => s.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.PeriodStatistics)
            .WithOne(p => p.GoalieMatchStatistics)
            .HasForeignKey(p => p.GoalieMatchStatisticsId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.MatchId, s.MatchActivePlayerId })
            .IsUnique()
            .HasDatabaseName("IX_HockeyGoalieMatchStatistics_Match_ActivePlayer");
    }
}

public class HockeyGoaliePeriodStatisticsConfiguration : BaseEntityConfiguration<HockeyGoaliePeriodStatistics>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyGoaliePeriodStatistics> builder)
    {
        builder.ToTable("HockeyGoaliePeriodStatistics");

        builder.Property(s => s.GoalieMatchStatisticsId).IsRequired();
        builder.Property(s => s.MatchId).IsRequired();
        builder.Property(s => s.MatchTeamId).IsRequired();
        builder.Property(s => s.MatchActivePlayerId).IsRequired();
        builder.Property(s => s.TeamPlayerId).IsRequired();
        builder.Property(s => s.PlayerId).IsRequired();
        builder.Property(s => s.TeamId).IsRequired();
        builder.Property(s => s.PeriodNumber).IsRequired();
        builder.Property(s => s.PeriodType).IsRequired().HasConversion<string>();
        builder.Property(s => s.TimeOnIceSeconds).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.ShotsAgainst).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Saves).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.GoalsAgainst).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.SavePercentage).IsRequired().HasPrecision(5, 2).HasDefaultValue(0m);

        builder.HasOne(s => s.Match)
            .WithMany()
            .HasForeignKey(s => s.MatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.MatchTeam)
            .WithMany()
            .HasForeignKey(s => s.MatchTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.MatchActivePlayer)
            .WithMany()
            .HasForeignKey(s => s.MatchActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.TeamPlayer)
            .WithMany()
            .HasForeignKey(s => s.TeamPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Player)
            .WithMany()
            .HasForeignKey(s => s.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Team)
            .WithMany()
            .HasForeignKey(s => s.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.MatchId, s.MatchActivePlayerId, s.PeriodNumber })
            .IsUnique()
            .HasDatabaseName("IX_HockeyGoaliePeriodStatistics_Match_ActivePlayer_Period");
    }
}
