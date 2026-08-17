using Domain.Entities.Hockey.Competitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyCompetitionDivisionConfiguration : BaseEntityConfiguration<HockeyCompetitionDivision>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyCompetitionDivision> builder)
    {
        builder.ToTable("HockeyCompetitionDivisions");

        builder.Property(d => d.CompetitionId).IsRequired();
        builder.Property(d => d.DivisionId).IsRequired();
        builder.Property(d => d.Name).IsRequired().HasMaxLength(100);
        builder.Property(d => d.SortOrder).IsRequired();
        builder.Property(d => d.IsActive).IsRequired();

        builder.HasMany(d => d.Teams)
            .WithOne(t => t.CompetitionDivision)
            .HasForeignKey(t => t.CompetitionDivisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.ChampionCompetitionTeam)
            .WithMany()
            .HasForeignKey(d => d.ChampionCompetitionTeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.OwnsOne(d => d.RulesOverride, rules =>
            HockeyCompetitionRulesOwnedConfiguration.ConfigureCompetitionRules(rules, "RulesOverride"));

        builder.HasIndex(d => new { d.CompetitionId, d.DivisionId })
            .IsUnique()
            .HasDatabaseName("IX_HockeyCompetitionDivisions_Competition_Division");
    }
}
