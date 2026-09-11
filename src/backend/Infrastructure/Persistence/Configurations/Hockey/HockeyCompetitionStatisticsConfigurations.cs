using Domain.Entities.Hockey.Statistics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyPlayerCompetitionStatisticsConfiguration : BaseEntityConfiguration<HockeyPlayerCompetitionStatistics>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyPlayerCompetitionStatistics> builder)
    {
        builder.ToTable("HockeyPlayerCompetitionStatistics");

        builder.Property(s => s.PlayerId).IsRequired();
        builder.Property(s => s.TeamId).IsRequired();
        builder.Property(s => s.TeamPlayerId).IsRequired();
        builder.Property(s => s.CompetitionId).IsRequired();
        builder.Property(s => s.Scope).IsRequired().HasConversion<string>();
        builder.Property(s => s.CompetitionDivisionId);
        builder.Property(s => s.TournamentGroupId);
        builder.Property(s => s.PlayoffSeriesId);
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

        builder.HasOne(s => s.Player)
            .WithMany()
            .HasForeignKey(s => s.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Team)
            .WithMany()
            .HasForeignKey(s => s.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.TeamPlayer)
            .WithMany()
            .HasForeignKey(s => s.TeamPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Competition)
            .WithMany()
            .HasForeignKey(s => s.CompetitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.CompetitionDivision)
            .WithMany()
            .HasForeignKey(s => s.CompetitionDivisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.TournamentGroup)
            .WithMany()
            .HasForeignKey(s => s.TournamentGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.PlayoffSeries)
            .WithMany()
            .HasForeignKey(s => s.PlayoffSeriesId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new
            {
                s.PlayerId,
                s.TeamId,
                s.CompetitionId,
                s.Scope,
                s.CompetitionDivisionId,
                s.TournamentGroupId,
                s.PlayoffSeriesId
            })
            .IsUnique()
            .HasDatabaseName("IX_HockeyPlayerCompetitionStatistics_UniqueScope");

        builder.HasIndex(s => new { s.CompetitionId, s.Points })
            .HasDatabaseName("IX_HockeyPlayerCompetitionStatistics_Competition_Points");
    }
}

public class HockeyGoalieCompetitionStatisticsConfiguration : BaseEntityConfiguration<HockeyGoalieCompetitionStatistics>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyGoalieCompetitionStatistics> builder)
    {
        builder.ToTable("HockeyGoalieCompetitionStatistics");

        builder.Property(s => s.PlayerId).IsRequired();
        builder.Property(s => s.TeamId).IsRequired();
        builder.Property(s => s.TeamPlayerId).IsRequired();
        builder.Property(s => s.CompetitionId).IsRequired();
        builder.Property(s => s.Scope).IsRequired().HasConversion<string>();
        builder.Property(s => s.CompetitionDivisionId);
        builder.Property(s => s.TournamentGroupId);
        builder.Property(s => s.PlayoffSeriesId);
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

        builder.HasOne(s => s.Player)
            .WithMany()
            .HasForeignKey(s => s.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Team)
            .WithMany()
            .HasForeignKey(s => s.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.TeamPlayer)
            .WithMany()
            .HasForeignKey(s => s.TeamPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Competition)
            .WithMany()
            .HasForeignKey(s => s.CompetitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.CompetitionDivision)
            .WithMany()
            .HasForeignKey(s => s.CompetitionDivisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.TournamentGroup)
            .WithMany()
            .HasForeignKey(s => s.TournamentGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.PlayoffSeries)
            .WithMany()
            .HasForeignKey(s => s.PlayoffSeriesId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new
            {
                s.PlayerId,
                s.TeamId,
                s.CompetitionId,
                s.Scope,
                s.CompetitionDivisionId,
                s.TournamentGroupId,
                s.PlayoffSeriesId
            })
            .IsUnique()
            .HasDatabaseName("IX_HockeyGoalieCompetitionStatistics_UniqueScope");

        builder.HasIndex(s => new { s.CompetitionId, s.SavePercentage })
            .HasDatabaseName("IX_HockeyGoalieCompetitionStatistics_Competition_SavePct");
    }
}

public class HockeyTeamCompetitionStatisticsConfiguration : BaseEntityConfiguration<HockeyTeamCompetitionStatistics>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyTeamCompetitionStatistics> builder)
    {
        builder.ToTable("HockeyTeamCompetitionStatistics");

        builder.Property(s => s.TeamId).IsRequired();
        builder.Property(s => s.CompetitionId).IsRequired();
        builder.Property(s => s.Scope).IsRequired().HasConversion<string>();
        builder.Property(s => s.CompetitionDivisionId);
        builder.Property(s => s.TournamentGroupId);
        builder.Property(s => s.PlayoffSeriesId);
        builder.Property(s => s.GamesPlayed).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.RegulationWins).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.OvertimeWins).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.ShootoutWins).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.RegulationLosses).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.OvertimeLosses).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.ShootoutLosses).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Ties).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Wins).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Losses).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.Points).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.GoalsFor).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.GoalsAgainst).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.GoalDifference).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.ShotsFor).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.ShotsAgainst).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.ShotPercentage).IsRequired().HasPrecision(5, 2).HasDefaultValue(0m);
        builder.Property(s => s.PowerPlayGoals).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.PowerPlayOpportunities).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.PowerPlayPercentage).IsRequired().HasPrecision(5, 2).HasDefaultValue(0m);
        builder.Property(s => s.PenaltyKillOpportunities).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.PenaltyKillSuccesses).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.PenaltyKillPercentage).IsRequired().HasPrecision(5, 2).HasDefaultValue(0m);
        builder.Property(s => s.PenaltyMinutes).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.FaceoffWins).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.FaceoffAttempts).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.FaceoffPercentage).IsRequired().HasPrecision(5, 2).HasDefaultValue(0m);
        builder.Property(s => s.HomeWins).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.HomeLosses).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.AwayWins).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.AwayLosses).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.StandingRank).IsRequired().HasDefaultValue(0);

        builder.HasOne(s => s.Team)
            .WithMany()
            .HasForeignKey(s => s.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Competition)
            .WithMany()
            .HasForeignKey(s => s.CompetitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.CompetitionDivision)
            .WithMany()
            .HasForeignKey(s => s.CompetitionDivisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.TournamentGroup)
            .WithMany()
            .HasForeignKey(s => s.TournamentGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.PlayoffSeries)
            .WithMany()
            .HasForeignKey(s => s.PlayoffSeriesId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new
            {
                s.TeamId,
                s.CompetitionId,
                s.Scope,
                s.CompetitionDivisionId,
                s.TournamentGroupId,
                s.PlayoffSeriesId
            })
            .IsUnique()
            .HasDatabaseName("IX_HockeyTeamCompetitionStatistics_UniqueScope");

        builder.HasIndex(s => new { s.CompetitionId, s.Points })
            .HasDatabaseName("IX_HockeyTeamCompetitionStatistics_Competition_Points");
    }
}

public class HockeyStatisticsCacheConfiguration : BaseEntityConfiguration<HockeyStatisticsCache>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyStatisticsCache> builder)
    {
        builder.ToTable("HockeyStatisticsCache");

        builder.Property(c => c.CacheKey).IsRequired().HasMaxLength(200);
        builder.Property(c => c.CompetitionId);
        builder.Property(c => c.Scope).HasConversion<string>();
        builder.Property(c => c.CompetitionDivisionId);
        builder.Property(c => c.TournamentGroupId);
        builder.Property(c => c.PlayoffSeriesId);
        builder.Property(c => c.TeamId);
        builder.Property(c => c.PlayerId);
        builder.Property(c => c.MatchId);
        builder.Property(c => c.JsonData).IsRequired().HasColumnType("text");
        builder.Property(c => c.LastUpdated).IsRequired();
        builder.Property(c => c.ExpiresAt).IsRequired();

        builder.Ignore(c => c.IsExpired);

        builder.HasOne(c => c.Competition)
            .WithMany()
            .HasForeignKey(c => c.CompetitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.CompetitionDivision)
            .WithMany()
            .HasForeignKey(c => c.CompetitionDivisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.TournamentGroup)
            .WithMany()
            .HasForeignKey(c => c.TournamentGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.PlayoffSeries)
            .WithMany()
            .HasForeignKey(c => c.PlayoffSeriesId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Team)
            .WithMany()
            .HasForeignKey(c => c.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Player)
            .WithMany()
            .HasForeignKey(c => c.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Match)
            .WithMany()
            .HasForeignKey(c => c.MatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.CacheKey)
            .IsUnique()
            .HasDatabaseName("IX_HockeyStatisticsCache_CacheKey");

        builder.HasIndex(c => c.CompetitionId)
            .HasDatabaseName("IX_HockeyStatisticsCache_CompetitionId");
    }
}
