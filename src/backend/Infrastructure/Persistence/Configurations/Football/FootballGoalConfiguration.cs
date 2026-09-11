using Domain.Entities.Football.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Football;

public class FootballGoalConfiguration : IEntityTypeConfiguration<FootballGoal>
{
    public void Configure(EntityTypeBuilder<FootballGoal> builder)
    {
        builder.Property(g => g.ScoringPlayerId).IsRequired(false);
        builder.Property(g => g.AssistingPlayerId).IsRequired(false);
        builder.Property(g => g.GoalType).IsRequired(false);
        builder.HasIndex(g => g.ScoringPlayerId).HasDatabaseName("IX_FootballMatchEvent_ScoringPlayerId");
        builder.HasIndex(g => g.AssistingPlayerId).HasDatabaseName("IX_FootballMatchEvent_AssistingPlayerId");
    }
}

public class FootballCardConfiguration : IEntityTypeConfiguration<FootballCard>
{
    public void Configure(EntityTypeBuilder<FootballCard> builder)
    {
        builder.Property(c => c.PlayerId).IsRequired();
        builder.Property(c => c.CardType).IsRequired().HasConversion<string>();
        builder.HasIndex(c => c.PlayerId).HasDatabaseName("IX_FootballMatchEvent_CardPlayerId");
    }
}

public class FootballSubstitutionConfiguration : IEntityTypeConfiguration<FootballSubstitution>
{
    public void Configure(EntityTypeBuilder<FootballSubstitution> builder)
    {
        builder.Property(s => s.PlayerOffId).IsRequired();
        builder.Property(s => s.PlayerOnId).IsRequired();
        builder.HasIndex(s => s.PlayerOffId).HasDatabaseName("IX_FootballMatchEvent_PlayerOffId");
        builder.HasIndex(s => s.PlayerOnId).HasDatabaseName("IX_FootballMatchEvent_PlayerOnId");
    }
}
