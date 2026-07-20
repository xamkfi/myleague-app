using Domain.Entities.Hockey.Competitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyTournamentGroupConfiguration : BaseEntityConfiguration<HockeyTournamentGroup>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyTournamentGroup> builder)
    {
        builder.ToTable("HockeyTournamentGroups");

        builder.Property(g => g.TournamentId).IsRequired();
        builder.Property(g => g.Name).IsRequired().HasMaxLength(100);
        builder.Property(g => g.SortOrder).IsRequired();

        builder.HasMany(g => g.Teams)
            .WithOne(t => t.TournamentGroup)
            .HasForeignKey(t => t.TournamentGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(g => new { g.TournamentId, g.SortOrder })
            .HasDatabaseName("IX_HockeyTournamentGroups_Tournament_SortOrder");
    }
}
