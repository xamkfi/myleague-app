using Domain.Entities.Football.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Football;

public class FootballTeamConfiguration : IEntityTypeConfiguration<FootballTeam>
{
    public void Configure(EntityTypeBuilder<FootballTeam> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Ignore(t => t.Division);
        builder.Property(t => t.DivisionId).IsRequired(false);
        builder.Property(t => t.HomeArena).IsRequired().HasMaxLength(100);
        builder.Property(t => t.PrimaryJerseyColor).IsRequired().HasMaxLength(50);
        builder.Property(t => t.SecondaryJerseyColor).HasMaxLength(50);
        builder.Property(t => t.LogoUrl)
            .HasConversion(v => v != null ? v.ToString() : null, v => v != null ? new Uri(v) : null);
        builder.Property(x => x.TeamCategory).IsRequired().HasConversion<string>();
        builder.Ignore(t => t.Club);
        builder.Property(t => t.ClubId).IsRequired();
        builder.HasMany(t => t.Roster)
            .WithOne()
            .HasForeignKey(p => p.TeamId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}
