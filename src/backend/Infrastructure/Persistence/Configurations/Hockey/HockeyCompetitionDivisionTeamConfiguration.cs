using Domain.Entities.Hockey.Competitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyCompetitionDivisionTeamConfiguration : BaseEntityConfiguration<HockeyCompetitionDivisionTeam>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyCompetitionDivisionTeam> builder)
    {
        builder.ToTable("HockeyCompetitionDivisionTeams");

        builder.Property(t => t.CompetitionDivisionId).IsRequired();
        builder.Property(t => t.CompetitionTeamId).IsRequired();

        builder.HasOne(t => t.CompetitionTeam)
            .WithMany()
            .HasForeignKey(t => t.CompetitionTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => new { t.CompetitionDivisionId, t.CompetitionTeamId })
            .IsUnique()
            .HasFilter("\"IsActive\" = true")
            .HasDatabaseName("IX_HockeyCompetitionDivisionTeams_Division_CompetitionTeam_Active");
    }
}
