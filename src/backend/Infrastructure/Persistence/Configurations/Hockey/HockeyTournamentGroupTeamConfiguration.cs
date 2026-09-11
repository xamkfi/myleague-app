using Domain.Entities.Hockey.Competitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyTournamentGroupTeamConfiguration : BaseEntityConfiguration<HockeyTournamentGroupTeam>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyTournamentGroupTeam> builder)
    {
        builder.ToTable("HockeyTournamentGroupTeams");

        builder.Property(t => t.TournamentGroupId).IsRequired();
        builder.Property(t => t.CompetitionTeamId).IsRequired();

        builder.HasOne(t => t.CompetitionTeam)
            .WithMany()
            .HasForeignKey(t => t.CompetitionTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => new { t.TournamentGroupId, t.CompetitionTeamId })
            .IsUnique()
            .HasFilter("\"IsActive\" = true")
            .HasDatabaseName("IX_HockeyTournamentGroupTeams_Group_CompetitionTeam_Active");
    }
}
