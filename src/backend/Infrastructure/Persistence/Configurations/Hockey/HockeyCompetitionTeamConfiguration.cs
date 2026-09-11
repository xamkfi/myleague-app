using Domain.Entities.Hockey.Competitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyCompetitionTeamConfiguration : BaseEntityConfiguration<HockeyCompetitionTeam>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyCompetitionTeam> builder)
    {
        builder.ToTable("HockeyCompetitionTeams");

        builder.Property(t => t.CompetitionId).IsRequired();
        builder.Property(t => t.TeamId).IsRequired();
        builder.Property(t => t.JoinedAt).IsRequired();
        builder.Property(t => t.LeftAt);

        builder.Ignore(t => t.IsActive);

        builder.HasIndex(t => new { t.CompetitionId, t.TeamId })
            .IsUnique()
            .HasFilter("\"LeftAt\" IS NULL")
            .HasDatabaseName("IX_HockeyCompetitionTeams_Competition_Team_Active");
    }
}
