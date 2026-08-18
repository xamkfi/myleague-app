using Domain.Entities.Hockey.Competitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeySeasonConfiguration : IEntityTypeConfiguration<HockeySeason>
{
    public void Configure(EntityTypeBuilder<HockeySeason> builder)
    {
        builder.Property(s => s.SeasonCode).HasMaxLength(50);
        builder.Property(s => s.ChampionCompetitionTeamId);
    }
}
