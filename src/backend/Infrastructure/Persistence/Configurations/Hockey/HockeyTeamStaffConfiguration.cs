using Domain.Entities.Hockey.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Hockey;

public class HockeyTeamStaffConfiguration : BaseEntityConfiguration<HockeyTeamStaff>
{
    protected override void ConfigureEntity(EntityTypeBuilder<HockeyTeamStaff> builder)
    {
        builder.ToTable("HockeyTeamStaff");

        builder.Property(s => s.PersonId).IsRequired();
        builder.Property(s => s.TeamId).IsRequired();
        builder.Property(s => s.Role).IsRequired().HasConversion<string>();
        builder.Property(s => s.JoinedAt).IsRequired();
        builder.Property(s => s.LeftAt);

        builder.Ignore(s => s.IsActive);
        builder.Ignore(s => s.Person);

        builder.HasIndex(s => new { s.TeamId, s.PersonId, s.Role, s.CompetitionId })
            .IsUnique()
            .HasFilter("\"LeftAt\" IS NULL")
            .HasDatabaseName("IX_HockeyTeamStaff_Team_Person_Role_Competition_Active");
    }
}
