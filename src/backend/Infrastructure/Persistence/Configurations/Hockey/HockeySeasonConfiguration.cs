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

        builder.HasMany(season => season.ContentBlocks)
            .WithOne()
            .HasForeignKey(block => block.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(season => season.ContentBlocks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
