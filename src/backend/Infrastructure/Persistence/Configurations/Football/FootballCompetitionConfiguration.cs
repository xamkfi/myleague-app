using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Football;

public class FootballCompetitionConfiguration : IEntityTypeConfiguration<FootballCompetition>
{
    public void Configure(EntityTypeBuilder<FootballCompetition> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasDiscriminator<string>("CompetitionType")
            .HasValue<FootballSeason>("Season")
            .HasValue<FootballTournament>("Tournament");

        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.StartDate).IsRequired();
        builder.Property(s => s.EndDate).IsRequired();
        builder.Property(s => s.IsActive).IsRequired();
        builder.Property(s => s.IsCompleted).IsRequired();
        builder.Property(s => s.TeamCategory).IsRequired().HasConversion<string>().HasDefaultValue(Domain.Enums.Common.TeamCategory.Adult);
        builder.HasIndex(s => s.TeamCategory);

        builder.OwnsOne(s => s.MatchRules, rules => FootballMatchRulesMapping.Map(rules, "MatchRules_"));
        builder.OwnsOne(s => s.StandingRules, rules => FootballMatchRulesMapping.MapStandingRules(rules, "StandingRules_"));

        builder.HasMany(s => s.Teams)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "FootballCompetitionTeam",
                j => j.HasOne<FootballTeam>().WithMany().HasForeignKey("TeamsId"),
                j => j.HasOne<FootballCompetition>().WithMany().HasForeignKey("CompetitionsId"));
    }
}
