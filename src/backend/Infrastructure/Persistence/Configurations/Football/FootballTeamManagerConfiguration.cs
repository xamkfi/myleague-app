using Domain.Entities.Football.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Football;

public class FootballTeamManagerConfiguration : IEntityTypeConfiguration<FootballTeamManager>
{
    public void Configure(EntityTypeBuilder<FootballTeamManager> builder)
    {
        builder.ToTable("FootballTeamManagers");
        builder.HasKey(tm => tm.Id);
        builder.Property(tm => tm.PersonId).IsRequired();
        builder.Property(tm => tm.TeamId).IsRequired();
        builder.Property(tm => tm.IsActive).IsRequired().HasDefaultValue(true);
        builder.HasOne<FootballTeam>().WithMany().HasForeignKey(tm => tm.TeamId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(tm => tm.PersonId).IsUnique().HasDatabaseName("IX_FootballTeamManager_PersonId");
        builder.HasIndex(tm => tm.TeamId).HasDatabaseName("IX_FootballTeamManager_TeamId");
    }
}
