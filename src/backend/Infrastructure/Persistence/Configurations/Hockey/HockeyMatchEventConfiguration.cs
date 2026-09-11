using Domain.Entities.Hockey.Matches.Events;
using Domain.Enums.Hockey.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyMatchEventConfiguration : BaseEntityConfiguration<HockeyMatchEvent>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyMatchEvent> builder)
    {
        builder.ToTable("HockeyMatchEvents", t =>
        {
            t.HasCheckConstraint("CK_HockeyMatchEvent_PeriodNumber", "\"PeriodNumber\" >= 1");
        });

        builder.HasDiscriminator(e => e.EventType)
            .HasValue<HockeyPeriodEvent>(HockeyMatchEventType.Period)
            .HasValue<HockeyGoal>(HockeyMatchEventType.Goal)
            .HasValue<HockeyPenalty>(HockeyMatchEventType.Penalty)
            .HasValue<HockeyShot>(HockeyMatchEventType.Shot)
            .HasValue<HockeyFaceoff>(HockeyMatchEventType.Faceoff)
            .HasValue<HockeyStoppage>(HockeyMatchEventType.Stoppage)
            .HasValue<HockeyTimeout>(HockeyMatchEventType.Timeout)
            .HasValue<HockeyGoalieChange>(HockeyMatchEventType.GoalieChange)
            .HasValue<HockeyVideoReview>(HockeyMatchEventType.VideoReview)
            .HasValue<HockeyShootoutAttempt>(HockeyMatchEventType.ShootoutAttempt);

        builder.Property(e => e.MatchId).IsRequired();
        builder.Property(e => e.EventType).IsRequired();
        builder.Property(e => e.MatchTeamId);
        builder.Property(e => e.MatchActivePlayerId);
        builder.Property(e => e.PeriodNumber).IsRequired();
        builder.Property(e => e.GameTime).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.Ignore(e => e.FormattedGameTime);

        builder.HasOne(e => e.Match)
            .WithMany(m => m.Events)
            .HasForeignKey(e => e.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.MatchTeam)
            .WithMany()
            .HasForeignKey(e => e.MatchTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.MatchActivePlayer)
            .WithMany()
            .HasForeignKey(e => e.MatchActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.MatchId)
            .HasDatabaseName("IX_HockeyMatchEvents_MatchId");
        builder.HasIndex(e => new { e.MatchId, e.PeriodNumber, e.GameTime })
            .HasDatabaseName("IX_HockeyMatchEvents_MatchId_Period_GameTime");
    }
}

public class HockeyPeriodEventConfiguration : IEntityTypeConfiguration<HockeyPeriodEvent>
{
    public void Configure(EntityTypeBuilder<HockeyPeriodEvent> builder)
    {
        builder.Property(e => e.Action).IsRequired().HasConversion<string>();
    }
}

public class HockeyGoalConfiguration : IEntityTypeConfiguration<HockeyGoal>
{
    public void Configure(EntityTypeBuilder<HockeyGoal> builder)
    {
        builder.Property(e => e.ScoringMatchTeamId).IsRequired();
        builder.Property(e => e.ScorerActivePlayerId).IsRequired();
        builder.Property(e => e.PrimaryAssistActivePlayerId);
        builder.Property(e => e.SecondaryAssistActivePlayerId);
        builder.Property(e => e.GoalieActivePlayerId);
        builder.Property(e => e.RelatedShotId);
        builder.Property(e => e.GoalStrength).IsRequired().HasConversion<string>();
        builder.Property(e => e.IsGameWinningGoal).IsRequired();
        builder.Property(e => e.WasEmptyNet).IsRequired();
        builder.Property(e => e.WasDelayedPenalty).IsRequired();
        builder.Property(e => e.WasPenaltyShotGoal).IsRequired();

        builder.HasOne(e => e.ScoringMatchTeam)
            .WithMany()
            .HasForeignKey(e => e.ScoringMatchTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Scorer)
            .WithMany()
            .HasForeignKey(e => e.ScorerActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PrimaryAssist)
            .WithMany()
            .HasForeignKey(e => e.PrimaryAssistActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SecondaryAssist)
            .WithMany()
            .HasForeignKey(e => e.SecondaryAssistActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Goalie)
            .WithMany()
            .HasForeignKey(e => e.GoalieActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.RelatedShot)
            .WithMany()
            .HasForeignKey(e => e.RelatedShotId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class HockeyPenaltyConfiguration : IEntityTypeConfiguration<HockeyPenalty>
{
    public void Configure(EntityTypeBuilder<HockeyPenalty> builder)
    {
        builder.Property(e => e.PenaltyMatchTeamId).IsRequired();
        builder.Property(e => e.PenalizedActivePlayerId);
        builder.Property(e => e.ServedByActivePlayerId);
        builder.Property(e => e.Severity).IsRequired().HasConversion<string>();
        builder.Property(e => e.Offence).IsRequired().HasConversion<string>();
        builder.Property(e => e.PenaltyMinutes).IsRequired();
        builder.Property(e => e.PenaltyStartTime);
        builder.Property(e => e.PenaltyEndTime);
        builder.Property(e => e.IsBenchPenalty).IsRequired();
        builder.Property(e => e.IsDelayedPenalty).IsRequired();
        builder.Property(e => e.WasServed).IsRequired();

        builder.HasOne(e => e.PenaltyMatchTeam)
            .WithMany()
            .HasForeignKey(e => e.PenaltyMatchTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PenalizedPlayer)
            .WithMany()
            .HasForeignKey(e => e.PenalizedActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ServedByPlayer)
            .WithMany()
            .HasForeignKey(e => e.ServedByActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class HockeyShotConfiguration : IEntityTypeConfiguration<HockeyShot>
{
    public void Configure(EntityTypeBuilder<HockeyShot> builder)
    {
        builder.Property(e => e.ShootingMatchTeamId).IsRequired();
        builder.Property(e => e.ShooterActivePlayerId);
        builder.Property(e => e.GoalieActivePlayerId);
        builder.Property(e => e.ShotResult).IsRequired().HasConversion<string>();
        builder.Property(e => e.IsPowerPlayShot).IsRequired();
        builder.Property(e => e.IsShortHandedShot).IsRequired();
        builder.Property(e => e.IsShootoutShot).IsRequired();
        builder.Property(e => e.CountsAsShotOnGoal).IsRequired();

        builder.HasOne(e => e.ShootingMatchTeam)
            .WithMany()
            .HasForeignKey(e => e.ShootingMatchTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Shooter)
            .WithMany()
            .HasForeignKey(e => e.ShooterActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Goalie)
            .WithMany()
            .HasForeignKey(e => e.GoalieActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class HockeyFaceoffConfiguration : IEntityTypeConfiguration<HockeyFaceoff>
{
    public void Configure(EntityTypeBuilder<HockeyFaceoff> builder)
    {
        builder.Property(e => e.WinningMatchTeamId).IsRequired();
        builder.Property(e => e.LosingMatchTeamId).IsRequired();
        builder.Property(e => e.WinningActivePlayerId);
        builder.Property(e => e.LosingActivePlayerId);
        builder.Property(e => e.Zone).IsRequired().HasConversion<string>();
        builder.Property(e => e.Spot).IsRequired().HasConversion<string>();

        builder.HasOne(e => e.WinningMatchTeam)
            .WithMany()
            .HasForeignKey(e => e.WinningMatchTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.LosingMatchTeam)
            .WithMany()
            .HasForeignKey(e => e.LosingMatchTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.WinningPlayer)
            .WithMany()
            .HasForeignKey(e => e.WinningActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.LosingPlayer)
            .WithMany()
            .HasForeignKey(e => e.LosingActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class HockeyStoppageConfiguration : IEntityTypeConfiguration<HockeyStoppage>
{
    public void Configure(EntityTypeBuilder<HockeyStoppage> builder)
    {
        builder.Property(e => e.Reason).IsRequired().HasConversion<string>();
        builder.Property(e => e.ResponsibleMatchTeamId);
        builder.Property(e => e.ResponsibleActivePlayerId);
        builder.Property(e => e.NextFaceoffZone).HasConversion<string>();
        builder.Property(e => e.NextFaceoffSpot).HasConversion<string>();
        builder.Property(e => e.RuleReference).HasMaxLength(100);

        builder.HasOne(e => e.ResponsibleMatchTeam)
            .WithMany()
            .HasForeignKey(e => e.ResponsibleMatchTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ResponsiblePlayer)
            .WithMany()
            .HasForeignKey(e => e.ResponsibleActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class HockeyVideoReviewConfiguration : IEntityTypeConfiguration<HockeyVideoReview>
{
    public void Configure(EntityTypeBuilder<HockeyVideoReview> builder)
    {
        builder.Property(e => e.ReviewType).IsRequired().HasConversion<string>();
        builder.Property(e => e.OriginalDecision).IsRequired().HasConversion<string>();
        builder.Property(e => e.FinalDecision).IsRequired().HasConversion<string>();
        builder.Property(e => e.RequestedByMatchTeamId);
        builder.Property(e => e.IsCoachChallenge).IsRequired();
        builder.Property(e => e.WasSuccessful).IsRequired();
        builder.Property(e => e.ResultingPenaltyId);

        builder.HasOne(e => e.RequestedByMatchTeam)
            .WithMany()
            .HasForeignKey(e => e.RequestedByMatchTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ResultingPenalty)
            .WithMany()
            .HasForeignKey(e => e.ResultingPenaltyId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class HockeyGoalieChangeConfiguration : IEntityTypeConfiguration<HockeyGoalieChange>
{
    public void Configure(EntityTypeBuilder<HockeyGoalieChange> builder)
    {
        builder.Property(e => e.OutgoingGoalieActivePlayerId);
        builder.Property(e => e.IncomingGoalieActivePlayerId);
        builder.Property(e => e.Reason).HasMaxLength(200);

        builder.HasOne(e => e.OutgoingGoalie)
            .WithMany()
            .HasForeignKey(e => e.OutgoingGoalieActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.IncomingGoalie)
            .WithMany()
            .HasForeignKey(e => e.IncomingGoalieActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class HockeyShootoutAttemptConfiguration : IEntityTypeConfiguration<HockeyShootoutAttempt>
{
    public void Configure(EntityTypeBuilder<HockeyShootoutAttempt> builder)
    {
        builder.Property(e => e.ShooterActivePlayerId).IsRequired();
        builder.Property(e => e.GoalieActivePlayerId).IsRequired();
        builder.Property(e => e.ShotOrder).IsRequired();
        builder.Property(e => e.Result).IsRequired().HasConversion<string>();

        builder.HasOne(e => e.Shooter)
            .WithMany()
            .HasForeignKey(e => e.ShooterActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Goalie)
            .WithMany()
            .HasForeignKey(e => e.GoalieActivePlayerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
