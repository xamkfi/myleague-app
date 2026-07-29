using Domain.Entities.Hockey.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyMatchPlayerSelectionConfiguration : BaseEntityConfiguration<HockeyMatchPlayerSelection>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyMatchPlayerSelection> builder)
    {
        builder.ToTable("HockeyMatchPlayerSelections");

        builder.Property(s => s.MatchTeamId).IsRequired();
        builder.Property(s => s.Source).IsRequired().HasConversion<string>();
        builder.Property(s => s.CreatedByUserId);
        builder.Property(s => s.ConfirmedByUserId);
        builder.Property(s => s.ConfirmedAt);
        builder.Property(s => s.IsConfirmed).IsRequired();

        builder.HasMany(s => s.ActivePlayers)
            .WithOne(p => p.MatchPlayerSelection)
            .HasForeignKey(p => p.MatchPlayerSelectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.MatchTeamId)
            .IsUnique()
            .HasDatabaseName("IX_HockeyMatchPlayerSelections_MatchTeamId");
    }
}
