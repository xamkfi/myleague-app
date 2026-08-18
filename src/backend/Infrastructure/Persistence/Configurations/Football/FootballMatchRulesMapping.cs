using Domain.ValueObjects.Football;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Football;

internal static class FootballMatchRulesMapping
{
    public static void Map<TOwner>(OwnedNavigationBuilder<TOwner, FootballMatchRules> rules, string prefix)
        where TOwner : class
    {
        rules.Property(r => r.NumberOfHalves)
            .HasColumnName($"{prefix}NumberOfHalves")
            .IsRequired()
            .HasDefaultValue(2);
        rules.Property(r => r.HalfDurationMinutes)
            .HasColumnName($"{prefix}HalfDurationMinutes")
            .IsRequired()
            .HasDefaultValue(45);
        rules.Property(r => r.PlayersOnField)
            .HasColumnName($"{prefix}PlayersOnField")
            .IsRequired()
            .HasDefaultValue(11);
        rules.Property(r => r.RequireGoalkeeper)
            .HasColumnName($"{prefix}RequireGoalkeeper")
            .IsRequired()
            .HasDefaultValue(true);
        rules.Property(r => r.MaxSubstitutions)
            .HasColumnName($"{prefix}MaxSubstitutions")
            .IsRequired()
            .HasDefaultValue(0);
        rules.Property(r => r.RequireOfficialsToStart)
            .HasColumnName($"{prefix}RequireOfficialsToStart")
            .IsRequired()
            .HasDefaultValue(false);
        rules.Property(r => r.AllowExtraTime)
            .HasColumnName($"{prefix}AllowExtraTime")
            .IsRequired()
            .HasDefaultValue(false);
        rules.Property(r => r.ExtraTimeHalfCount)
            .HasColumnName($"{prefix}ExtraTimeHalfCount")
            .IsRequired()
            .HasDefaultValue(0);
        rules.Property(r => r.ExtraTimeHalfDurationMinutes)
            .HasColumnName($"{prefix}ExtraTimeHalfDurationMinutes")
            .IsRequired()
            .HasDefaultValue(0);
        rules.Property(r => r.AllowPenaltyShootout)
            .HasColumnName($"{prefix}AllowPenaltyShootout")
            .IsRequired()
            .HasDefaultValue(false);
    }

    public static void MapStandingRules<TOwner>(OwnedNavigationBuilder<TOwner, FootballStandingRules> rules, string prefix)
        where TOwner : class
    {
        rules.Property(r => r.WinPoints)
            .HasColumnName($"{prefix}WinPoints")
            .IsRequired()
            .HasDefaultValue(3);
        rules.Property(r => r.DrawPoints)
            .HasColumnName($"{prefix}DrawPoints")
            .IsRequired()
            .HasDefaultValue(1);
        rules.Property(r => r.LossPoints)
            .HasColumnName($"{prefix}LossPoints")
            .IsRequired()
            .HasDefaultValue(0);
    }
}
