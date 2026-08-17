using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Football;

public class FootballMatchConfiguration : IEntityTypeConfiguration<FootballMatch>
{
    public void Configure(EntityTypeBuilder<FootballMatch> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.ScheduledDateTime).IsRequired();
        builder.Property(m => m.Venue).HasMaxLength(200);
        builder.Property(m => m.HomeScore).IsRequired();
        builder.Property(m => m.AwayScore).IsRequired();
        builder.Property(m => m.Status).IsRequired().HasConversion<string>();
        builder.Property(m => m.WentToExtraTime).IsRequired();
        builder.Property(m => m.WentToPenaltyShootout).IsRequired();

        builder.OwnsOne(m => m.MatchRules, rules => FootballMatchRulesMapping.Map(rules, "MatchRules_"));

        builder.Property(m => m.HomeTeamId).IsRequired(false);
        builder.Property(m => m.AwayTeamId).IsRequired(false);
        builder.Property(m => m.CompetitionId).IsRequired();

        builder.HasOne(m => m.Competition)
            .WithMany(s => s.Matches)
            .HasForeignKey(m => m.CompetitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.HomeTeam).WithMany().HasForeignKey(m => m.HomeTeamId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.AwayTeam).WithMany().HasForeignKey(m => m.AwayTeamId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.Officials)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "FootballMatchOfficial",
                j => j.HasOne<FootballReferee>().WithMany().HasForeignKey("OfficialsId"),
                j => j.HasOne<FootballMatch>().WithMany().HasForeignKey("MatchesId"));

        builder.HasMany(m => m.PeriodScores).WithOne().HasForeignKey(p => p.MatchId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(m => m.PeriodScores).HasField("_periodScores").EnableLazyLoading(false);

        builder.HasMany(m => m.Lineup).WithOne().HasForeignKey(p => p.MatchId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(m => m.Lineup).HasField("_lineup").EnableLazyLoading(false);

        builder.Ignore(m => m.HomeOnFieldPlayers);
        builder.Ignore(m => m.AwayOnFieldPlayers);
        builder.Ignore(m => m.GoalEvents);
        builder.Ignore(m => m.CardEvents);
        builder.Ignore(m => m.SubstitutionEvents);

        builder.HasMany(m => m.Events).WithOne().HasForeignKey("MatchId").OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(m => m.Events).HasField("_events").EnableLazyLoading(false);

        builder.Property(m => m.TournamentStage).HasConversion<int?>().IsRequired(false);
        builder.Property(m => m.TournamentGroupId).IsRequired(false);
        builder.HasOne<FootballTournamentGroup>().WithMany().HasForeignKey(m => m.TournamentGroupId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(m => m.PlayoffRound).HasConversion<int?>().IsRequired(false);
        builder.Property(m => m.PlayoffMatchOrder).IsRequired(false);
        builder.Property(m => m.NextMatchId).IsRequired(false);
        builder.Property(m => m.NextMatchSlot).HasConversion<int?>().IsRequired(false);
        builder.HasOne<FootballMatch>().WithMany().HasForeignKey(m => m.NextMatchId).OnDelete(DeleteBehavior.Restrict);
    }
}
