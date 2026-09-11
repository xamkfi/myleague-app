using Domain.Entities.Football.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Football;

public class FootballRefereeConfiguration : IEntityTypeConfiguration<FootballReferee>
{
    public void Configure(EntityTypeBuilder<FootballReferee> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.PersonId).IsRequired();
        builder.Ignore(r => r.Person);
        builder.Property(r => r.IsActive).IsRequired();
        builder.Property(r => r.LicenseIssueDate).IsRequired(false);
        builder.Property(r => r.LicenseExpiryDate).IsRequired(false);
        builder.Property(r => r.MatchesOfficiated).IsRequired();
    }
}
