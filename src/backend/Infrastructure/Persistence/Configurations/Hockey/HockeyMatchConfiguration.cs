using Domain.Entities.Hockey.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyMatchConfiguration : BaseEntityConfiguration<HockeyMatch>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyMatch> builder)
    {
        builder.ToTable("HockeyMatches");

        builder.Property(m => m.CompetitionId).IsRequired();
        builder.Property(m => m.HomeCompetitionTeamId);
        builder.Property(m => m.AwayCompetitionTeamId);
    }
}
